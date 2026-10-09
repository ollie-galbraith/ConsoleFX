using System;
using System.Security;
using System.Text;

namespace ConsoleFX
{
    public sealed class TextInputCompletionOption
    {
        public string Value { get; }
        public string DisplayText { get; }

        public TextInputCompletionOption(string value, string displayText)
        {
            Value = value;
            DisplayText = displayText;
        }
    }

    public class TextInput
    {
        private string _prompt;
        private string _value      = string.Empty;
        private int _padding       = 1;
        private int _startRow      = 0;
        private int _startColumn   = 0;
        private int _borderInset   = 0;
        private bool _promptInline = false;
        private bool _secureInput  = false;
        private int _lastWindowWidth;
        private int _lastWindowHeight;

        private bool _isRunning    = true;
        private bool _wasCancelled = false;
        private bool _border       = true;
        private bool _fullWidth    = false;

        private string? _borderTitle = null;
        private BorderStyle? _borderStyle;
        private IReadOnlyList<TextInputCompletionOption> _tabCompletionOptions = [];
        private int _selectedTabCompletion = -1;
        private string? _tabCompletionDisplay;

        public bool WasCancelled => _wasCancelled;
        public string Value      => _value;
        public GridMenu? UnderlayMenu { get; set; }
        public Func<string, string?>? TabCompletionHandler { get; set; }
        public Func<string, IReadOnlyList<TextInputCompletionOption>>? TabCompletionOptionsHandler { get; set; }

        public string PromptColor = ConsoleFX.Foreground.Cyan;
        public string InputColor  = ConsoleFX.Foreground.White;
        public string BorderColor = ConsoleFX.Foreground.Blue;
        public string TitleColor  = ConsoleFX.Foreground.Blue;

        public TextInput(
            string prompt,
            int     padding      = 1,
            bool    fullWidth    = false,
            bool    border       = true,
            string? borderStyle  = null,
            string? title        = null,
            int     startRow     = 0,
            int     startColumn  = 0,
            int     borderInset  = 0,
            bool    promptInline = false,
            bool    secureInput  = false
        )
        {
            _prompt       = prompt ?? string.Empty;
            _padding      = Math.Max(0, padding);
            _fullWidth    = fullWidth;
            _border       = border;
            _startRow     = Math.Max(0, startRow);
            _startColumn  = Math.Max(0, startColumn);
            _borderInset  = Math.Max(0, borderInset);
            _promptInline = promptInline;
            _secureInput  = secureInput;

            if (border)
            {
                BorderManager borderManager = new BorderManager();

                var assembly = typeof(TextInput).Assembly;
                var stream   = assembly.GetManifestResourceStream("ConsoleFX.Resources.borders.json");
                if (stream != null)
                {
                    using (stream)
                    {
                        borderManager.LoadJsonFromStream(stream);
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        "Failed to load embedded resource 'ConsoleFX.Resources.borders.json'"
                    );
                }

                string resolvedBorderStyle = string.IsNullOrEmpty(borderStyle)
                    ? "curved"
                    :  borderStyle!;
                _borderStyle = borderManager.GetStyle(resolvedBorderStyle);
                if (_borderStyle == null)
                {
                    throw new InvalidOperationException(
                        $"Border style '{borderStyle ?? "curved"}' not found."
                    );
                }

                if (!string.IsNullOrEmpty(title))
                {
                    _borderTitle = title;
                }
            }
        }

        public void SetColor(
            string? promptColor = null,
            string? inputColor  = null,
            string? borderColor = null,
            string? titleColor  = null
        )
        {
            if (!string.IsNullOrEmpty(promptColor))
            {
                PromptColor = promptColor!;
            }
            if (!string.IsNullOrEmpty(inputColor))
            {
                InputColor = inputColor!;
            }
            if (!string.IsNullOrEmpty(borderColor))
            {
                BorderColor = borderColor!;
            }
            if (!string.IsNullOrEmpty(titleColor))
            {
                TitleColor = titleColor!;
            }
        }

        public object Run(string? initialValue = null)
        {
            string result = RunInternal(initialValue);
            if (!_secureInput)
            {
                return result;
            }

            SecureString secureResult = new SecureString();
            foreach (char c in result)
            {
                secureResult.AppendChar(c);
            }
            secureResult.MakeReadOnly();
            return secureResult;
        }

        private string RunInternal(string? initialValue = null)
        {
            Encoding originalOutputEncoding = Console.OutputEncoding;
            Encoding originalInputEncoding  = Console.InputEncoding;

            Console.OutputEncoding = new UTF8Encoding(false);
            Console.InputEncoding  = new UTF8Encoding(false);
            Console.Write(Cursor.Visible);

            _value            = initialValue ?? string.Empty;
            _isRunning        = true;
            _wasCancelled     = false;
            ClearTabCompletion();
            _lastWindowWidth  = Console.WindowWidth;
            _lastWindowHeight = Console.WindowHeight;

            bool resizeWatcherInitialized = ConsoleResizeWatcher.Initialize();

            try
            {
                Render(clearScreen: true);

                while (_isRunning)
                {
                    if (
                        resizeWatcherInitialized
                        && ConsoleResizeWatcher.TryReadResizeEvent(
                            100,
                            out short cols,
                            out short rows
                        )
                    )
                    {
                        bool clearScreen       = cols < _lastWindowWidth || rows < _lastWindowHeight;
                             _lastWindowWidth  = cols;
                             _lastWindowHeight = rows;
                        Render(clearScreen);
                    }

                    if (Console.KeyAvailable)
                    {
                        var keyInfo = Console.ReadKey(true);
                        HandleInput(keyInfo);
                        if (_isRunning)
                        {
                            Render(clearScreen: true);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            }
            finally
            {
                if (resizeWatcherInitialized)
                {
                    ConsoleResizeWatcher.Restore();
                }

                Console.OutputEncoding = originalOutputEncoding;
                Console.InputEncoding  = originalInputEncoding;
                Console.Write(Cursor.Invisible);
            }

            MoveToNextLine();
            return _wasCancelled ? string.Empty : _value;
        }

        private void MoveToNextLine()
        {
            // Calculate the last row occupied by the rendered input
            int contentRows = (_promptInline ? 1 : 2) + (_tabCompletionDisplay is null ? 0 : 1);
            int totalRows   = _border ? contentRows + 2 : contentRows;                       // +2 for top/bottom border
            int lastRow     = _startRow + totalRows - 1;
            int nextRow     = Math.Min(lastRow + 1, Math.Max(Console.BufferHeight - 1, 0));
            Console.Write(Cursor.JumpTo(nextRow + 1, 1));
            Console.SetCursorPosition(0, nextRow);
        }

        private void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter: 
                    _isRunning = false;
                    return;
                case ConsoleKey.Escape: 
                    _wasCancelled = true;
                    _isRunning    = false;
                    return;
                case ConsoleKey.Tab:
                    if (TabCompletionOptionsHandler is not null)
                    {
                        var reverse = (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;
                        CompleteTabOptions(reverse);
                        return;
                    }

                    var completion = TabCompletionHandler?.Invoke(_value);
                    if (completion is not null)
                    {
                        _value = completion;
                    }
                    return;
                case ConsoleKey.Backspace: 
                    ClearTabCompletion();
                    if (_value.Length > 0)
                    {
                        _value = _value.Substring(0, _value.Length - 1);
                    }
                    return;
            }

            if (!char.IsControl(keyInfo.KeyChar))
            {
                ClearTabCompletion();
                _value += keyInfo.KeyChar;
            }
        }

        private void CompleteTabOptions(bool reverse)
        {
            if (_tabCompletionOptions.Count > 1 &&
                _selectedTabCompletion >= 0 &&
                _value == _tabCompletionOptions[_selectedTabCompletion].Value)
            {
                var step = reverse ? _tabCompletionOptions.Count - 1 : 1;
                _selectedTabCompletion = (_selectedTabCompletion + step) % _tabCompletionOptions.Count;
            }
            else
            {
                _tabCompletionOptions = TabCompletionOptionsHandler?.Invoke(_value) ?? [];
                if (_tabCompletionOptions.Count == 0)
                {
                    ClearTabCompletion();
                    return;
                }

                _selectedTabCompletion = reverse ? _tabCompletionOptions.Count - 1 : 0;
            }

            _value = _tabCompletionOptions[_selectedTabCompletion].Value;
            var labels = _tabCompletionOptions.Select((option, index) =>
                index == _selectedTabCompletion ? $"[{option.DisplayText}]" : option.DisplayText);
            _tabCompletionDisplay = $"Options: {string.Join("  ", labels)}";
        }

        private void ClearTabCompletion()
        {
            _tabCompletionOptions = [];
            _selectedTabCompletion = -1;
            _tabCompletionDisplay = null;
        }

        private void ClearFromStartPosition()
        {
            int safeColumn = Math.Min(
                Math.Max(_startColumn, 0),
                Math.Max(Console.WindowWidth - 1, 0)
            );
            int safeRow = Math.Min(Math.Max(_startRow, 0), Math.Max(Console.WindowHeight - 1, 0));
            Console.SetCursorPosition(safeColumn, safeRow);
            Console.Write(Erase.ToScreenEnd);
            Console.SetCursorPosition(safeColumn, safeRow);
        }

        private void Render(bool clearScreen = false)
        {
            if (UnderlayMenu is not null)
            {
                UnderlayMenu.RenderBackground();
                clearScreen = false;
            }

            if (clearScreen)
            {
                ClearFromStartPosition();
            }

            int inset              = (_fullWidth && _borderInset > 0) ? _borderInset : 0;
            int leftOffset         = _startColumn + inset;
            int availableTextWidth = GetAvailableTextWidth(leftOffset);
            int inputWidth         = GetInputWidth(availableTextWidth);

            string rawVisible       = GetVisibleInput(inputWidth);
            int    rawVisibleLength = rawVisible.Length;
            string displayVisible   = _secureInput ? new string('*', rawVisibleLength) : rawVisible;

            string promptDisplay   = PreparePromptDisplay(availableTextWidth, inputWidth);
            int    rawPromptLength = _promptInline
                ? (_prompt.Length + (_prompt.Length > 0 ? 1 : 0))
                :  promptDisplay.Length;
            string visibleInput = PrepareInputDisplay(displayVisible, inputWidth);

            if (_border)
            {
                RenderBordered(
                    promptDisplay,
                    visibleInput,
                    leftOffset,
                    rawPromptLength,
                    rawVisibleLength
                );
            }
            else
            {
                RenderUnbordered(
                    promptDisplay,
                    visibleInput,
                    leftOffset,
                    rawPromptLength,
                    rawVisibleLength
                );
            }
        }

        private int GetAvailableTextWidth(int leftOffset)
        {
            if (!_fullWidth)
            {
                if (_promptInline)
                {
                    return Math.Max(_prompt.Length + 1 + _value.Length, 1);
                }

                return Math.Max(Math.Max(_prompt.Length, _value.Length), 1);
            }

            int rightInset = _fullWidth && _border ? _borderInset : 0;
            int width = Console.WindowWidth - leftOffset - rightInset - (_border ? (2 + (_padding * 2)) : 0);
            return Math.Max(width, 1);
        }

        private int GetInputWidth(int availableTextWidth)
        {
            if (!_promptInline)
            {
                return availableTextWidth;
            }

            int promptWidth = _prompt.Length + (_prompt.Length > 0 ? 1 : 0);
            return Math.Max(availableTextWidth - promptWidth, 1);
        }

        private string GetVisibleInput(int width)
        {
            if (width <= 0)
            {
                return string.Empty;
            }

            if (_value.Length <= width)
            {
                return _value;
            }

            return _value.Substring(_value.Length - width, width);
        }

        private string PreparePromptDisplay(int availableTextWidth, int inputWidth)
        {
            if (_promptInline)
            {
                string promptPrefix = string.IsNullOrEmpty(_prompt) ? string.Empty : _prompt + " ";
                if (_fullWidth)
                {
                    int    promptWidth = Math.Max(availableTextWidth - inputWidth, 0);
                    string trimmed     = 
                        promptPrefix.Length > promptWidth
                            ? promptPrefix.Substring(0, promptWidth)
                            :  promptPrefix;
                    return trimmed.PadRight(promptWidth);
                }

                return promptPrefix;
            }

            return PrepareDisplayLine(_prompt, availableTextWidth);
        }

        private string PrepareInputDisplay(string value, int width)
        {
            return PrepareDisplayLine(value, width);
        }

        private string PrepareDisplayLine(string value, int width)
        {
            if (_fullWidth)
            {
                string trimmed = value.Length > width ? value.Substring(0, width) : value;
                return trimmed.PadRight(width);
            }

            return value;
        }

        private void RenderBordered(
            string promptDisplay,
            string visibleInput,
            int leftOffset,
            int rawPromptLength,
            int rawInputLength
        )
        {
            bool   charMatrixFullWidth = _fullWidth && leftOffset == 0;
            var lines = new List<string>();
            if (_promptInline)
            {
                lines.Add($"{PromptColor}{promptDisplay}{InputColor}{visibleInput}");
            }
            else
            {
                lines.Add($"{PromptColor}{promptDisplay}");
                lines.Add($"{InputColor}{visibleInput}");
            }
            if (_tabCompletionDisplay is not null)
            {
                lines.Add(_tabCompletionDisplay);
            }

            CharMatrix charMatrix = new CharMatrix(lines.ToArray(), _padding, charMatrixFullWidth);
            charMatrix.SetBorder(
                _borderStyle
                    ?? throw new InvalidOperationException("Border style not initialized."),
                BorderColor
            );

            if (!string.IsNullOrEmpty(_borderTitle))
            {
                charMatrix.SetTitle(_borderTitle!, TitleColor);
            }

            Console.SetCursorPosition(
                Math.Min(Math.Max(_startColumn, 0), Math.Max(Console.WindowWidth - 1, 0)),
                Math.Min(Math.Max(_startRow, 0), Math.Max(Console.WindowHeight - 1, 0))
            );
            Console.Write(
                leftOffset > 0 ? charMatrix.Render(x: leftOffset + 1) : charMatrix.Render()
            );

            int promptOffset = _promptInline ? rawPromptLength : 0;
            int cursorColumn = leftOffset + 1 + _padding + promptOffset + rawInputLength;
            int cursorRow    = _startRow + (_promptInline ? 1 : 2);
            PositionCursor(cursorColumn, cursorRow);
        }

        private void RenderUnbordered(
            string promptDisplay,
            string visibleInput,
            int leftOffset,
            int rawPromptLength,
            int rawInputLength
        )
        {
            int safeColumn = Math.Min(
                Math.Max(leftOffset, 0),
                Math.Max(Console.WindowWidth - 1, 0)
            );
            int promptRow = Math.Min(Math.Max(_startRow, 0), Math.Max(Console.WindowHeight - 1, 0));
            int inputRow = Math.Min(
                Math.Max(_startRow + (_promptInline ? 0 : 1), 0),
                Math.Max(Console.WindowHeight - 1, 0)
            );

            Console.SetCursorPosition(safeColumn, promptRow);
            if (_promptInline)
            {
                Console.Write(
                    $"{PromptColor}{promptDisplay}{InputColor}{visibleInput}{ConsoleFX.Format.Reset}"
                );
                    if (_tabCompletionDisplay is not null)
                    {
                        Console.SetCursorPosition(safeColumn, Math.Min(promptRow + 1, Math.Max(Console.WindowHeight - 1, 0)));
                        Console.Write($"{_tabCompletionDisplay}{ConsoleFX.Format.Reset}");
                    }
                PositionCursor(safeColumn + rawPromptLength + rawInputLength, inputRow);
                return;
            }

            Console.Write($"{PromptColor}{promptDisplay}{ConsoleFX.Format.Reset}");
            Console.SetCursorPosition(safeColumn, inputRow);
            Console.Write($"{InputColor}{visibleInput}{ConsoleFX.Format.Reset}");
            if (_tabCompletionDisplay is not null)
            {
                Console.SetCursorPosition(safeColumn, Math.Min(inputRow + 1, Math.Max(Console.WindowHeight - 1, 0)));
                Console.Write($"{_tabCompletionDisplay}{ConsoleFX.Format.Reset}");
            }

            PositionCursor(safeColumn + rawInputLength, inputRow);
        }

        private void PositionCursor(int column, int row)
        {
            int safeColumn = Math.Min(Math.Max(column, 0), Math.Max(Console.WindowWidth - 1, 0));
            int safeRow    = Math.Min(Math.Max(row, 0),    Math.Max(Console.WindowHeight - 1, 0));
            Console.Write(Cursor.JumpTo(safeRow + 1, safeColumn + 1));
            Console.SetCursorPosition(safeColumn, safeRow);
        }
    }
}
