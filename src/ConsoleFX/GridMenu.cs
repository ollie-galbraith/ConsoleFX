using System;
using System.Text;

// using System.Collections.Generic;
// using System.IO;
// using System.Linq;

namespace ConsoleFX
{
    public class GridMenu
    {
        private List<string> _options;

        private int _columns;
        private int _effectiveColumns = 1;
        private int _padding          = 1;
        private int _selectedIndex    = 0;
        private int _startRow         = 0;
        private int _lastWindowWidth;
        private int _lastWindowHeight;

        private int _borderInset            = 0;
        private bool _isDimmed              = false;
        private string? _lastRenderedOutput = null;

        private bool _isRunning    = true;
        private bool _wasCancelled = false;
        private bool _wasShortcutPressed = false;
        private bool _border       = true;
        private bool _fullWidth    = false;

        private string? _borderTitle = null;

        private BorderStyle? _borderStyle;
        public GridMenu? UnderlayMenu { get; set; }
        public bool WasCancelled => _wasCancelled;
        public ConsoleKey? ShortcutKey { get; set; }
        public Func<ConsoleKeyInfo, bool>? ShortcutHandler { get; set; }
        public bool WasShortcutPressed => _wasShortcutPressed;
        public string? FooterText { get; set; }

        public string SelectedItemColor = ConsoleFX.Foreground.Red;
        public string DefaultColor      = ConsoleFX.Foreground.White;
        public string BorderColor       = ConsoleFX.Foreground.Blue;
        public string TitleColor        = ConsoleFX.Foreground.Blue;

        public GridMenu(
            List<string> options,
            int     columns     = 3,
            int     padding     = 1,
            bool    fullWidth   = false,
            bool    border      = true,
            string? borderStyle = null,
            string? title       = null,
            int     startRow    = 0,
            int     borderInset = 0,
            bool    isDimmed    = false
        )
        {
            _options     = options;
            _columns     = columns > 0 ? columns : 3;
            _border      = border;
            _padding     = padding;
            _fullWidth   = fullWidth;
            _startRow    = Math.Max(0, startRow);
            _borderInset = Math.Max(0, borderInset);
            _isDimmed    = isDimmed;

            if (border)
            {
                BorderManager borderManager = new BorderManager();

                var assembly = typeof(GridMenu).Assembly;
                var stream = assembly.GetManifestResourceStream("ConsoleFX.Resources.borders.json");
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
                    : borderStyle!;
                _borderStyle = borderManager.GetStyle(resolvedBorderStyle);
                if (_borderStyle == null)
                {
                    throw new InvalidOperationException(
                        $"Border style '{borderStyle ?? "curved"}' not found. Check if BorderManager.LoadJsonFromStream() properly deserialized the JSON."
                    );
                }

                if (!string.IsNullOrEmpty(title))
                {
                    _borderTitle = title;
                }
            }
        }

        private string ApplyDim(string? color)
        {
            if (!_isDimmed || string.IsNullOrEmpty(color))
                return color ?? "";
            return $"{ConsoleFX.Format.Faint}{color}";
        }

        public void SetColor(
            string? selectedItemColor = null,
            string? defaultColor      = null,
            string? borderColor       = null,
            string? titleColor        = null
        )
        {
            if (!string.IsNullOrEmpty(selectedItemColor))
            {
                SelectedItemColor = ApplyDim(selectedItemColor);
            }
            if (!string.IsNullOrEmpty(defaultColor))
            {
                DefaultColor = ApplyDim(defaultColor);
            }
            if (!string.IsNullOrEmpty(borderColor))
            {
                BorderColor = ApplyDim(borderColor);
            }
            if (!string.IsNullOrEmpty(titleColor))
            {
                TitleColor = ApplyDim(titleColor);
            }
        }

        public string Run()
        {
            Encoding originalOutputEncoding = Console.OutputEncoding;
            Encoding originalInputEncoding  = Console.InputEncoding;

              // Ensure box-drawing characters in border styles are encoded correctly.
            Console.OutputEncoding = new UTF8Encoding(false);
            Console.InputEncoding  = new UTF8Encoding(false);

            Console.Write(Cursor.Invisible);

            _isRunning    = true;
            _wasCancelled = false;
            _wasShortcutPressed = false;
            ClearFromStartRow();
                 _lastWindowWidth         = Console.WindowWidth;
                 _lastWindowHeight        = Console.WindowHeight;
            bool resizeWatcherInitialized = ConsoleResizeWatcher.Initialize();

            try
            {
                RenderWithUnderlay();
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
                        bool clearScreen = cols < _lastWindowWidth || rows < _lastWindowHeight;
                        _lastWindowWidth  = cols;
                        _lastWindowHeight = rows;
                        RenderWithUnderlay(clearScreen);
                    }

                    if (Console.KeyAvailable)
                    {
                        var keyInfo = Console.ReadKey(true);
                        HandleInput(keyInfo);
                        if (_isRunning)
                        {
                            RenderWithUnderlay();
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
                Console.Write(Cursor.Visible);
                Console.OutputEncoding = originalOutputEncoding;
                Console.InputEncoding  = originalInputEncoding;
            }

            if (_wasCancelled)
            {
                return string.Empty;
            }

            return this._options[_selectedIndex];
        }

        private void ClearFromStartRow()
        {
            // Erase from cursor to end of display without touching lines above _startRow.
            Console.SetCursorPosition(0, _startRow);
            Console.Write(Erase.ToScreenEnd);
            Console.SetCursorPosition(0, _startRow);
        }

        private void RenderWithUnderlay(bool clearScreen = false)
        {
            if (clearScreen)
            {
                ClearFromStartRow();
            }

            UnderlayMenu?.RenderBackground();
            Render();
        }

        private void Render(bool clearScreen = false)
        {
            if (clearScreen)
            {
                ClearFromStartRow();
            }
            Console.SetCursorPosition(0, _startRow);

            int inset = (this._fullWidth && this._borderInset > 0) ? this._borderInset : 0;
            int widthOffset = this._border ? 2 + (this._padding * 2) + (inset * 2) : 0;
            var grid = GridConverter.FromList(
                this._options,
                this._columns,
                defaultGap : 2,
                fullWidth  : this._fullWidth,
                widthOffset: widthOffset
            );
            _effectiveColumns = Math.Max(1, grid.Columns);

            // Apply DefaultStyle to all items
            for (int i = 0; i < this._options.Count; i++)
            {
                int row = i / grid.Columns;
                int col = i % grid.Columns;
                grid.SetCellValue($"{DefaultColor}{this._options[i]}", row, col);
            }

            // Override selected item with special styling
            int    selectedRow    = _selectedIndex / grid.Columns;
            int    selectedColumn = _selectedIndex % grid.Columns;
            string originalText   = _options[_selectedIndex];
            grid.SetCellValue(
                $"{ConsoleFX.Format.Italic}{ConsoleFX.Format.Bold}{SelectedItemColor}{originalText}{ConsoleFX.Format.Reset}{DefaultColor}",
                selectedRow,
                selectedColumn
            );

            if (!this._border)
            {
                grid.Render();
                RenderFooter();
                return;
            }

            string gridOutput   = grid.ToString();
            string[] gridString = gridOutput.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.RemoveEmptyEntries
            );

            bool       charMatrixFullWidth = this._fullWidth && inset == 0;
            CharMatrix charMatrix          = new CharMatrix(gridString, this._padding, charMatrixFullWidth);

            charMatrix.SetBorder(
                this._borderStyle
                    ?? throw new InvalidOperationException("Border style not initialized."),
                BorderColor
            );
            string? borderTitle = this._borderTitle;
            if (!string.IsNullOrEmpty(borderTitle))
            {
                charMatrix.SetTitle(borderTitle!, TitleColor);
            }
            _lastRenderedOutput = inset > 0 ? charMatrix.Render(x: inset + 1) : charMatrix.Render();
            Console.Write(_lastRenderedOutput);
            RenderFooter();
        }

        private void RenderFooter()
        {
            if (string.IsNullOrEmpty(FooterText))
            {
                return;
            }

            var width = Math.Max(1, Console.WindowWidth);
            var footer = FooterText.Length > width ? FooterText.Substring(0, width) : FooterText;
            var column = Math.Max(0, (width - footer.Length) / 2);
            Console.SetCursorPosition(column, Math.Max(0, Console.WindowHeight - 1));
            Console.Write(Erase.EntireLine);
            Console.Write(DefaultColor);
            Console.Write(footer);
            Console.Write(ConsoleFX.Format.Reset);
        }

        public string? SaveScreenOutput()
        {
            return _lastRenderedOutput;
        }

        public void SetSelectedIndex(int index)
        {
            if (_options.Count == 0)
            {
                _selectedIndex = 0;
                return;
            }

            _selectedIndex = Math.Max(0, Math.Min(index, _options.Count - 1));
        }

        public void SetSelectedItem(string? item)
        {
            if (string.IsNullOrEmpty(item))
            {
                return;
            }

            int index = _options.IndexOf(item!);
            if (index >= 0)
            {
                _selectedIndex = index;
            }
        }

        public void RestoreScreenOutput(string? output)
        {
            if (!string.IsNullOrEmpty(output))
            {
                Console.SetCursorPosition(0, _startRow);
                Console.Write(output);
            }
        }

        public void RenderBackground()
        {
            Encoding originalOutputEncoding = Console.OutputEncoding;
            Encoding originalInputEncoding  = Console.InputEncoding;

            Console.OutputEncoding = new UTF8Encoding(false);
            Console.InputEncoding  = new UTF8Encoding(false);

            try
            {
                _lastWindowWidth  = Console.WindowWidth;
                _lastWindowHeight = Console.WindowHeight;
                UnderlayMenu?.RenderBackground();
                Render();
            }
            finally
            {
                Console.OutputEncoding = originalOutputEncoding;
                Console.InputEncoding  = originalInputEncoding;
            }
        }

        private void HandleInput(ConsoleKeyInfo keyInfo)
        {
            if (ShortcutHandler?.Invoke(keyInfo) == true || ShortcutKey == keyInfo.Key)
            {
                _wasShortcutPressed = true;
                _isRunning = false;
                return;
            }

            int columns = Math.Max(1, _effectiveColumns);
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    if (_selectedIndex >= columns)
                        _selectedIndex -= columns;
                    break;
                case ConsoleKey.DownArrow:
                    if (_selectedIndex + columns < _options.Count)
                        _selectedIndex += columns;
                    break;
                case ConsoleKey.LeftArrow:
                    if (_selectedIndex % columns > 0)
                        _selectedIndex--;
                    break;
                case ConsoleKey.RightArrow:
                    if (
                        (_selectedIndex % columns) < (columns - 1)
                        && _selectedIndex < _options.Count - 1
                    )
                        _selectedIndex++;
                    break;
                case ConsoleKey.Enter:
                    _isRunning = false;
                    break;
                case ConsoleKey.Escape:
                    _wasCancelled = true;
                    _isRunning    = false;
                    break;
            }
        }
    }
}
