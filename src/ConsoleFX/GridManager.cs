using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleFX
{
    public class GridManager
    {
        private readonly string[,] _strings;
        private readonly int[,] _spacing;
        private readonly int[,] _visualLengths;
        private bool _recalculate = true;

        private static readonly Regex AnsiRegex = new Regex(
            @"\x1B\[[^mK]*[mK]",
            RegexOptions.Compiled
        );

        public int[] _cachedWidths;
        private readonly int _defaultGap;
        private readonly int _widthOffset;
        public int Padding { get; set; }
        public bool FullWidth { get; set; }
        public int WidthOffset => _widthOffset;
        public int Rows => _strings.GetLength(0);
        public int Columns => _strings.GetLength(1);

        public GridManager(
            int rows,
            int columns,
            int defaultGap = 1,
            bool fullWidth = false,
            int widthOffset = 0
        )
        {
            _strings = new string[rows, columns];
            _spacing = new int[rows, Math.Max(0, columns - 1)];
            _cachedWidths = new int[columns];
            _visualLengths = new int[rows, columns];
            _defaultGap = defaultGap;
            FullWidth = fullWidth;
            _widthOffset = widthOffset;

            // Initialize with empty defaults
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                    _strings[row, column] = "";
                for (int column = 0; column < columns - 1; column++)
                    _spacing[row, column] = defaultGap;
            }
        }

        // Helper to calculate the actual X position for rendering
        public int GetColumnOffset(int row, int column)
        {
            int x = this.Padding;
            for (int i = 0; i < column; i++)
            {
                x += this._cachedWidths[i];
                x += this._spacing[row, i];
            }
            return x;
        }

        public void SetCellValue(string value, int row, int column)
        {
            this._strings[row, column] = value ?? "";
            this._visualLengths[row, column] = GetVisualLength(this._strings[row, column]);
            this._recalculate = true;
        }

        public void SetSpacing(int value, int row, int column)
        {
            this._spacing[row, column] = value;
            this._recalculate = true;
        }

        public int GetSpacing(int row, int column)
        {
            return this._spacing[row, column];
        }

        public void UpdateWidths()
        {
            if (!this._recalculate)
                return;

            for (int column = 0; column < this.Columns; column++)
            {
                int max = 0;
                for (int row = 0; row < this.Rows; row++)
                {
                    // No Regex here, just looking up the cached integer
                    if (this._visualLengths[row, column] > max)
                        max = this._visualLengths[row, column];
                }
                this._cachedWidths[column] = max;
            }
            this._recalculate = false;
        }

        private void ApplyFullWidthSpacing()
        {
            if (!FullWidth || this.Columns <= 1)
                return;

            int availableWidth = Console.WindowWidth - (Padding * 2) - _widthOffset;
            if (availableWidth <= 0)
                return;

            int contentWidth = 0;
            for (int column = 0; column < this.Columns; column++)
            {
                contentWidth += this._cachedWidths[column];
            }

            int gapCount = this.Columns - 1;
            int minTotalGap = _defaultGap * gapCount;
            int targetTotalGap = Math.Max(minTotalGap, availableWidth - contentWidth);
            int baseGap = targetTotalGap / gapCount;
            int extra = targetTotalGap % gapCount;

            for (int row = 0; row < this.Rows; row++)
            {
                for (int column = 0; column < gapCount; column++)
                {
                    this._spacing[row, column] = baseGap + (column < extra ? 1 : 0);
                }
            }
        }

        private int GetVisualLength(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;
            // Remove ANSI codes before measuring length
            return AnsiRegex.Replace(input, "").Length;
        }

        public override string ToString() => ToString(false);

        public string ToString(bool verticalPadding)
        {
            UpdateWidths();
            ApplyFullWidthSpacing();

            var stringBuilder = new StringBuilder();
            string horizontalPadding = new string(' ', this.Padding);

            if (verticalPadding)
            {
                for (int i = 0; i < this.Padding; i++)
                    stringBuilder.AppendLine();
            }

            for (int row = 0; row < this.Rows; row++)
            {
                // Left padding
                stringBuilder.Append(horizontalPadding);

                for (int column = 0; column < this.Columns; column++)
                {
                    // Add string value
                    stringBuilder.Append(this._strings[row, column]);
                    int cellPadding = this._cachedWidths[column] - this._visualLengths[row, column];
                    stringBuilder.Append(new string(' ', cellPadding));

                    if (column < (this.Columns - 1))
                    {
                        int gap = this._spacing[row, column];
                        if (gap > 0)
                        {
                            stringBuilder.Append(new string(' ', gap));
                        }
                    }
                }
                // Right padding
                stringBuilder.Append(horizontalPadding);
                // Next line
                stringBuilder.AppendLine();
            }

            if (verticalPadding)
            {
                for (int i = 0; i < this.Padding; i++)
                    stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public void Render()
        {
            Console.Write(ToString(false));
        }
    }
}
