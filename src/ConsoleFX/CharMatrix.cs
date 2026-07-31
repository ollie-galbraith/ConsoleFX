using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleFX
{
    public class CharMatrix
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Padding { get; set; }
        public bool FullWidth { get; private set; }

        // Use '!' to tell the compiler these are initialized in the constructor/SetSize
        public char[,] Buffer { get; private set; } = null!;
        public string?[,] StyleBuffer { get; private set; } = null!;

        public Dictionary<string, object?> Border { get; private set; } =
            new Dictionary<string, object?>();

        private const string ESC = "\x1B";
        private static readonly Regex AnsiRegex = new Regex(
            @"\x1B\[[0-9;]*m",
            RegexOptions.Compiled
        );

        public static int PrintableLength(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
            return AnsiRegex.Replace(text, "").Length;
        }

        public CharMatrix(string[] text, int padding = 1, bool fullWidth = false)
        {
            int calculatedWidth;
            if (fullWidth)
            {
                calculatedWidth = Console.BufferWidth;
            }
            else
            {
                int max = 0;
                foreach (var line in text)
                {
                    int len = PrintableLength(line);
                    if (len > max)
                        max = len;
                }
                calculatedWidth = max + (2 + (padding * 2));
            }

            int calculatedHeight = text.Length + 2;
            this.Padding = padding;
            this.FullWidth = fullWidth;

            // Initializing arrays here satisfies CS8618
            SetSize(calculatedWidth, calculatedHeight);
            SetText(text, null);
        }

        public void SetSize(int width, int height, char fill = ' ')
        {
            this.Width = width;
            this.Height = height;
            this.Buffer = new char[height, width];
            this.StyleBuffer = new string?[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Buffer[y, x] = fill;
                    StyleBuffer[y, x] = string.Empty;
                }
            }
        }

        public void Set(int x, int y, char c)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                Buffer[y, x] = c;
        }

        // Changed 'string ansi' to 'string? ansi'
        public void SetStyled(int x, int y, char c, string? ansi)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Buffer[y, x] = c;
                StyleBuffer[y, x] = string.IsNullOrEmpty(ansi) ? null : ansi;
            }
        }

        private void _SetBorder(
            char topLeft,
            char topRight,
            char bottomLeft,
            char bottomRight,
            char horizontal,
            char vertical,
            char verticalLeft,
            char verticalRight,
            char horizontalTop,
            char horizontalBottom,
            string? ansi
        )
        {
            string? styleCode = string.IsNullOrEmpty(ansi) ? null : $"{ESC}{ansi}";

            for (int x = 0; x < Width; x++)
            {
                                SetStyled(x,          0, horizontal, styleCode);
                SetStyled(x, Height - 1, horizontal, styleCode);
            }
            for (int y = 0; y < Height; y++)
            {
                                SetStyled(        0, y, vertical, styleCode);
                SetStyled(Width - 1, y, vertical, styleCode);
            }

            SetStyled(        0,          0, topLeft,     styleCode);
            SetStyled(Width - 1,          0, topRight,    styleCode);
            SetStyled(        0, Height - 1, bottomLeft,  styleCode);
            SetStyled(Width - 1, Height - 1, bottomRight, styleCode);

            Border["topLeft"]          = topLeft;
            Border["topRight"]         = topRight;
            Border["bottomLeft"]       = bottomLeft;
            Border["bottomRight"]      = bottomRight;
            Border["horizontal"]       = horizontal;
            Border["vertical"]         = vertical;
            Border["verticalLeft"]     = verticalLeft;
            Border["verticalRight"]    = verticalRight;
            Border["horizontalTop"]    = horizontalTop;
            Border["horizontalBottom"] = horizontalBottom;
            Border["ansi"]             = ansi;
        }

        public void SetBorder(BorderStyle style, string? ansi)
        {
            _SetBorder(
                style.TopLeft,
                style.TopRight,
                style.BottomLeft,
                style.BottomRight,
                style.Horizontal,
                style.Vertical,
                style.VerticalLeft,
                style.VerticalRight,
                style.HorizontalTop,
                style.HorizontalBottom,
                ansi
            );
        }

        public void SetTitle(string title, string? ansi)
        {
            int plainLength = PrintableLength(title);
            int requiredWidth = plainLength + 2 + (Padding * 2) + 2;

            if (requiredWidth > Width)
            {
                Resize(requiredWidth, Height);
                // Safely cast dictionary values
                _SetBorder(
                    (char)(Border["topLeft"] ?? ' '),
                    (char)(Border["topRight"] ?? ' '),
                    (char)(Border["bottomLeft"] ?? ' '),
                    (char)(Border["bottomRight"] ?? ' '),
                    (char)(Border["horizontal"] ?? ' '),
                    (char)(Border["vertical"] ?? ' '),
                    (char)(Border["verticalLeft"] ?? ' '),
                    (char)(Border["verticalRight"] ?? ' '),
                    (char)(Border["horizontalTop"] ?? ' '),
                    (char)(Border["horizontalBottom"] ?? ' '),
                    (string?)Border["ansi"]
                );
            }

            string? styleCode = string.IsNullOrEmpty(ansi) ? null : $"{ESC}{ansi}";
            string titleText = $"{Border["verticalRight"]} {title} {Border["verticalLeft"]}";
            int column = 2;
            string? currentStyle = styleCode;

            for (int i = 0; i < titleText.Length; i++)
            {
                if (i == 0 || i == (titleText.Length - 1))
                {
                    SetStyled(column++, 0, titleText[i], (string?)Border["ansi"]);
                    continue;
                }
                else if (titleText[i] == '\x1B')
                {
                    string sequence = ExtractAnsi(titleText, ref i);
                    currentStyle =
                        (sequence.EndsWith("[0m") || sequence.EndsWith("[m"))
                            ? styleCode
                            : (currentStyle + sequence);
                    continue;
                }
                SetStyled(column++, 0, titleText[i], currentStyle);
            }
        }

        public void SetText(string[] text, string? ansi)
        {
            string? styleCode = string.IsNullOrEmpty(ansi) ? null : $"{ESC}{ansi}";
            for (int row = 0; row < text.Length; row++)
            {
                string line = text[row];
                int col = 0;
                string? currentStyle = styleCode;

                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] == '\x1B')
                    {
                        string sequence = ExtractAnsi(line, ref i);
                        currentStyle =
                            (sequence.EndsWith("[0m") || sequence.EndsWith("[m"))
                                ? styleCode
                                : (currentStyle + sequence);
                        continue;
                    }
                    SetStyled(col + (1 + Padding), row + 1, line[i], currentStyle);
                    col++;
                }
            }
        }

        private string ExtractAnsi(string input, ref int index)
        {
            StringBuilder sb = new StringBuilder();
            while (index < input.Length)
            {
                char c = input[index];
                sb.Append(c);
                if (c == 'm')
                    break;
                index++;
            }
            return sb.ToString();
        }

        public string Render(int? x = null, int? y = null)
        {
            string reset = $"{ESC}[0m";
            StringBuilder content = new StringBuilder();

            for (int yy = 0; yy < Height; yy++)
            {
                for (int xx = 0; xx < Width; xx++)
                {
                    string? style = StyleBuffer[yy, xx];
                    bool hasStyle = !string.IsNullOrEmpty(style);

                    if (hasStyle)
                        content.Append(style);
                    content.Append(Buffer[yy, xx]);
                    if (hasStyle)
                        content.Append(reset);
                }
                if (yy < Height - 1)
                    content.AppendLine();
            }

            if (FullWidth && !y.HasValue)
                return content.ToString();

            int? effectiveX = null;
            if (x.HasValue && !FullWidth)
            {
                int maxX = Math.Max(1, Console.BufferWidth - Width);
                effectiveX = Math.Min(Math.Max(x.Value, 1), maxX);
            }

            StringBuilder sb = new StringBuilder();
            if (y.HasValue)
            {
                sb.Append(Erase.EntireScreen);
                sb.Append(Cursor.JumpTo(y.Value, effectiveX ?? 1));
            }
            else if (effectiveX.HasValue)
            {
                sb.Append(Cursor.Column(effectiveX.Value));
            }

            string matrixContent = content.ToString();
            if (effectiveX.HasValue)
            {
                matrixContent = matrixContent.Replace("\n", "\n" + Cursor.Column(effectiveX.Value));
            }
            sb.Append(matrixContent);
            return sb.ToString();
        }

        public void Resize(int newWidth, int newHeight)
        {
            char[,] oldBuffer = Buffer;
            string?[,] oldStyle = StyleBuffer;
            int oldWidth = Width;
            int oldHeight = Height;

            SetSize(newWidth, newHeight);

            for (int y = 1; y < Math.Min(oldHeight, newHeight) - 1; y++)
            {
                for (int x = 1; x < Math.Min(oldWidth, newWidth) - 1; x++)
                {
                    Buffer[y, x] = oldBuffer[y, x];
                    StyleBuffer[y, x] = oldStyle[y, x];
                }
            }
        }
    }
}
