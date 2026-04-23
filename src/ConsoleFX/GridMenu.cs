using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;

namespace ConsoleFX
    {
        public class GridMenu
            {
                private List<string> _options;

                private int _columns;
                private int _padding = 1;
                private int _selectedIndex = 0;

                private bool _isRunning = true;
                private bool _border = true;
                private bool _fullWidth = false;

                private string? _borderTitle = null;

                private BorderStyle? _borderStyle;

                public string SelectedItemColor = ConsoleFX.Foreground.Red;
                public string DefaultColor = ConsoleFX.Foreground.White;
                public string BorderColor = ConsoleFX.Foreground.Blue;
                public string TitleColor = ConsoleFX.Foreground.Blue;

                public GridMenu (List<string> options, int columns = 3, int padding = 1, bool fullWidth = false, bool border = true, string? borderStyle = null, string? title = null)
                    {
                        _options = options;
                        _columns = columns;
                        _border = border;
                        _padding = padding;
                        _fullWidth = fullWidth;

                        if (border)
                            {
                                BorderManager borderManager = new BorderManager();

                                var assembly = typeof(GridMenu).Assembly;
                                // borderManager.LoadJson("{\"curved\":{\"top_left\":\"╭\",\"top_right\":\"╮\",\"bottom_left\":\"╰\",\"bottom_right\":\"╯\",\"horizontal\":\"─\",\"vertical\":\"│\",\"vertical_left\":\"├\",\"vertical_right\":\"┤\",\"horizontal_top\":\"┬\",\"horizontal_bottom\":\"┴\"},\"single\":{\"top_left\":\"┌\",\"top_right\":\"┐\",\"bottom_left\":\"└\",\"bottom_right\":\"┘\",\"horizontal\":\"─\",\"vertical\":\"│\",\"vertical_left\":\"├\",\"vertical_right\":\"┤\",\"horizontal_top\":\"┬\",\"horizontal_bottom\":\"┴\"},\"double\":{\"top_left\":\"╔\",\"top_right\":\"╗\",\"bottom_left\":\"╚\",\"bottom_right\":\"╝\",\"horizontal\":\"═\",\"vertical\":\"║\",\"vertical_left\":\"╠\",\"vertical_right\":\"╣\",\"horizontal_top\":\"╦\",\"horizontal_bottom\":\"╩\"},\"dotted\":{\"top_left\":\"┌\",\"top_right\":\"┐\",\"bottom_left\":\"└\",\"bottom_right\":\"┘\",\"horizontal\":\"┄\",\"vertical\":\"┆\",\"vertical_left\":\"├\",\"vertical_right\":\"┤\",\"horizontal_top\":\"┬\",\"horizontal_bottom\":\"┴\"}}");
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
                                        throw new InvalidOperationException("Failed to load embedded resource 'ConsoleFX.Resources.borders.json'");
                                    }

                                _borderStyle = borderManager.GetStyle(string.IsNullOrEmpty(borderStyle) ? "curved" : borderStyle);
                                if (_borderStyle == null)
                                    {
                                        throw new InvalidOperationException($"Border style '{borderStyle ?? "curved"}' not found. Check if BorderManager.LoadJsonFromStream() properly deserialized the JSON.");
                                    }

                                if (!string.IsNullOrEmpty(title))
                                    {
                                        _borderTitle = title;
                                    }
                            }
                    }

                public void SetColor(string? selectedItemColor = null, string? defaultColor = null, string? borderColor = null , string? titleColor = null)
                    {
                        if (!string.IsNullOrEmpty(selectedItemColor))
                            {
                                SelectedItemColor = selectedItemColor;
                            }
                        if (!string.IsNullOrEmpty(defaultColor))
                            {
                                DefaultColor = defaultColor;
                            }
                        if (!string.IsNullOrEmpty(borderColor))
                            {
                                BorderColor = borderColor;
                            }
                        if (!string.IsNullOrEmpty(titleColor))
                            {
                                TitleColor = titleColor;
                            }
                    }

                public string Run()
                    {
                        Console.CursorVisible = false;

                        _isRunning = true;
                        Console.SetCursorPosition(0,0);
                        Console.Clear();

                        while (_isRunning)
                            {
                                Render();
                                var key = Console.ReadKey(true).Key;
                                HandleInput(key);
                            }

                        Console.CursorVisible = true;
                        return this._options[_selectedIndex];
                    }

                private void Render()
                    {
                        int widthOffset = this._border ? 2 + (this._padding * 2) : 0;
                        var grid = GridConverter.FromList(
                            this._options,
                            this._columns,
                            defaultGap: 2,
                            fullWidth: this._fullWidth,
                            widthOffset: widthOffset
                        );
                        
                        // Apply DefaultStyle to all items
                        for (int i = 0; i < this._options.Count; i++)
                            {
                                int row = i / grid.Columns;
                                int col = i % grid.Columns;
                                grid.SetCellValue($"{DefaultColor}{this._options[i]}", row, col);
                            }
                        
                        // Override selected item with special styling
                        int selectedRow = _selectedIndex / grid.Columns;
                        int selectedColumn = _selectedIndex % grid.Columns;
                        string originalText = _options[_selectedIndex];
                        grid.SetCellValue($"{ConsoleFX.Format.Italic}{ConsoleFX.Format.Bold}{SelectedItemColor}{originalText}{ConsoleFX.Format.Reset}{DefaultColor}", selectedRow, selectedColumn);

                        Console.SetCursorPosition(0,0);

                        if (!this._border)
                            {
                                grid.Render();
                                return;
                            }

                        string gridOutput     = grid.ToString();
                        string[] gridString   = gridOutput.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                        CharMatrix charMatrix = new CharMatrix(gridString, this._padding, this._fullWidth);

                        charMatrix.SetBorder(this._borderStyle ?? throw new InvalidOperationException("Border style not initialized."), BorderColor);
                        if (!string.IsNullOrEmpty(this._borderTitle))
                            {
                                charMatrix.SetTitle(this._borderTitle, TitleColor);
                            }
                        Console.Write(charMatrix.Render());
                    }

                private void HandleInput(ConsoleKey key)
                    {
                        int rows = (int)Math.Ceiling((double)_options.Count / _columns);
                        switch (key)
                            {
                                case ConsoleKey.UpArrow:
                                    if (_selectedIndex >= _columns) _selectedIndex -= _columns;
                                    break;
                                case ConsoleKey.DownArrow:
                                    if (_selectedIndex + _columns < _options.Count) _selectedIndex += _columns;
                                    break;
                                case ConsoleKey.LeftArrow:
                                    if (_selectedIndex % _columns > 0) _selectedIndex--;
                                    break;
                                case ConsoleKey.RightArrow:
                                    if (_selectedIndex < _options.Count - 1) _selectedIndex++;
                                    break;
                                case ConsoleKey.Enter:
                                    _isRunning = false;
                                    break;
                                case ConsoleKey.Escape:
                                    _selectedIndex = -1; // Indicate cancellation
                                    _isRunning = false;
                                    break;
                            }
                    }
            }
    }

