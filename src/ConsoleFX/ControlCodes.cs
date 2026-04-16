using System;

namespace ConsoleFX
{
    public class Ansi
        {
            public static bool Debug { get; set; } = false;
            
            public const char Bell           = '\x07';
            public const char Backspace      = '\x08';
            public const char HorizontalTab  = '\x09';
            public const char NewLine        = '\x0A';
            public const char VerticalTab    = '\x0B';
            public const char FormFeed       = '\x0C';
            public const char CarriageReturn = '\x0D';
            public const char Escape         = '\x1B';
            public const char DeleteChar     = '\x7F';

            public static readonly string CSI = Escape.ToString() + "[";

            public static string SGR(params int[] codes)
                {
                    string sequence = $"{CSI}{string.Join(";", codes)}m";
                    if(Debug)
                        {
                            Console.WriteLine($"[DEBUG] Generated ANSI: {sequence.Replace("\x1b", "\\x1b")}");
                        }
                    return sequence;
                }
        }

    public class TerminalStyle
        {
            public string Enable { get; }
            public string Reset { get; }

            public TerminalStyle(string enable, string reset)
                {
                    Enable = enable;
                    Reset = reset;
                }

            public override string ToString() => Enable;
        }

    public class Cursor
        {
            public static string HomePosition                    => $"{Ansi.CSI}H";
            public static string JumpTo   (int line, int column) => $"{Ansi.CSI}{line};{column}H";
            public static string Up       (int num = 1)          => $"{Ansi.CSI}{num}A";
            public static string Down     (int num = 1)          => $"{Ansi.CSI}{num}B";
            public static string Right    (int num = 1)          => $"{Ansi.CSI}{num}C";
            public static string Left     (int num = 1)          => $"{Ansi.CSI}{num}D";
            public static string LineDown (int num = 1)          => $"{Ansi.CSI}{num}E";
            public static string LineUp   (int num = 1)          => $"{Ansi.CSI}{num}F";
            public static string Column   (int num = 1)          => $"{Ansi.CSI}{num}G";
            public static string Save                            => $"{Ansi.CSI}s";
            public static string Restore                         => $"{Ansi.CSI}u";
            public static string Invisible                       => $"{Ansi.CSI}?25l";
            public static string Visible                         => $"{Ansi.CSI}?25h";
        }

    public class Erase
        {
            public static string ToScreenEnd   => $"{Ansi.CSI}0J";
            public static string ToScreenStart => $"{Ansi.CSI}1J";
            public static string EntireScreen  => $"{Ansi.CSI}2J";
            public static string SavedLine     => $"{Ansi.CSI}3J";
            public static string ToLineEnd     => $"{Ansi.CSI}0K";
            public static string ToLineStart   => $"{Ansi.CSI}1K";
            public static string EntireLine    => $"{Ansi.CSI}2K";

        }

    public class Screen
        {
            public static TerminalStyle LineWrapping      => new($"{Ansi.CSI}=7h", $"{Ansi.CSI}=7l");
            public static TerminalStyle AlternativeBuffer => new($"{Ansi.CSI}?1049h", $"{Ansi.CSI}?1049l");
            public static string Save    => $"{Ansi.CSI}?47h";
            public static string Restore => $"{Ansi.CSI}?47l";
        }

    public class ScreenMode
        {
            public class Monochrome
                {
                    public static TerminalStyle Text_40_25              => new($"{Ansi.CSI}=0h", $"{Ansi.CSI}=0l");
                    public static TerminalStyle Text_80_25              => new($"{Ansi.CSI}=2h", $"{Ansi.CSI}=2l");
                    public static TerminalStyle Graphics_320_200        => new($"{Ansi.CSI}=5h", $"{Ansi.CSI}=5l");
                    public static TerminalStyle Graphics_640_200        => new($"{Ansi.CSI}=6h", $"{Ansi.CSI}=6l");
                    public static TerminalStyle Graphics_640_350_2color => new($"{Ansi.CSI}=15h", $"{Ansi.CSI}=15l");
                    public static TerminalStyle Graphics_640_480_2color => new($"{Ansi.CSI}=17h", $"{Ansi.CSI}=17l");
                }

            public class Color
                {
                    public static TerminalStyle Text_40_25                => new($"{Ansi.CSI}=1h", $"{Ansi.CSI}=1l");
                    public static TerminalStyle Text_80_25                => new($"{Ansi.CSI}=3h", $"{Ansi.CSI}=3l");
                    public static TerminalStyle Graphics_320_200_4color   => new($"{Ansi.CSI}=4h", $"{Ansi.CSI}=4l");
                    public static TerminalStyle Graphics_320_200          => new($"{Ansi.CSI}=13h", $"{Ansi.CSI}=13l");
                    public static TerminalStyle Graphics_640_200_16color  => new($"{Ansi.CSI}=14h", $"{Ansi.CSI}=14l");
                    public static TerminalStyle Graphics_640_350_16color  => new($"{Ansi.CSI}=16h", $"{Ansi.CSI}=16l");
                    public static TerminalStyle Graphics_640_480_16color  => new($"{Ansi.CSI}=18h", $"{Ansi.CSI}=18l");
                    public static TerminalStyle Graphics_320_200_256color => new($"{Ansi.CSI}=19h", $"{Ansi.CSI}=19l");
                }
        }

    // SGR based codes
    public enum Color
        {
            Black   = 0,
            Red     = 1,
            Green   = 2,
            Yellow  = 3,
            Blue    = 4,
            Magenta = 5,
            Cyan    = 6,
            White   = 7
        }

    public enum FormatType
        {
            Reset         = 0,
            Bold          = 1,
            Faint         = 2,
            Italic        = 3,
            Underline     = 4,
            FlashSlow     = 5,
            Flash         = 6,
            Negative      = 7,
            Conceal       = 8,
            Strikethrough = 9
        }

    public class Format
        {
            public static string Reset                => Ansi.SGR((int)FormatType.Reset);
            public static TerminalStyle Bold          => new(Ansi.SGR((int)FormatType.Bold),          Ansi.SGR(20 + (int)FormatType.Bold));
            public static TerminalStyle Faint         => new(Ansi.SGR((int)FormatType.Faint),         Ansi.SGR(20 + (int)FormatType.Faint));
            public static TerminalStyle Italic        => new(Ansi.SGR((int)FormatType.Italic),        Ansi.SGR(20 + (int)FormatType.Italic));
            public static TerminalStyle Underline     => new(Ansi.SGR((int)FormatType.Underline),     Ansi.SGR(20 + (int)FormatType.Underline));
            public static TerminalStyle FlashSlow     => new(Ansi.SGR((int)FormatType.FlashSlow),     Ansi.SGR(20 + (int)FormatType.FlashSlow));
            public static TerminalStyle Flash         => new(Ansi.SGR((int)FormatType.Flash),         Ansi.SGR(20 + (int)FormatType.Flash));
            public static TerminalStyle Negative      => new(Ansi.SGR((int)FormatType.Negative),      Ansi.SGR(20 + (int)FormatType.Negative));
            public static TerminalStyle Conceal       => new(Ansi.SGR((int)FormatType.Conceal),       Ansi.SGR(20 + (int)FormatType.Conceal));
            public static TerminalStyle Strikethrough => new(Ansi.SGR((int)FormatType.Strikethrough), Ansi.SGR(20 + (int)FormatType.Strikethrough));
        }

    

    public class Foreground
        {
            public static string Black         => ColorToAnsi(Color.Black);
            public static string Red           => ColorToAnsi(Color.Red);
            public static string Green         => ColorToAnsi(Color.Green);
            public static string Yellow        => ColorToAnsi(Color.Yellow);
            public static string Blue          => ColorToAnsi(Color.Blue);
            public static string Magenta       => ColorToAnsi(Color.Magenta);
            public static string Cyan          => ColorToAnsi(Color.Cyan);
            public static string White         => ColorToAnsi(Color.White);

            public static string BrightBlack   => ColorToAnsi(Color.Black,   bright: true);
            public static string BrightRed     => ColorToAnsi(Color.Red,     bright: true);
            public static string BrightGreen   => ColorToAnsi(Color.Green,   bright: true);
            public static string BrightYellow  => ColorToAnsi(Color.Yellow,  bright: true);
            public static string BrightBlue    => ColorToAnsi(Color.Blue,    bright: true);
            public static string BrightMagenta => ColorToAnsi(Color.Magenta, bright: true);
            public static string BrightCyan    => ColorToAnsi(Color.Cyan,    bright: true);
            public static string BrightWhite   => ColorToAnsi(Color.White,   bright: true);

            public static string Index(int index)                  => Ansi.SGR(38, 5, index);
            public static string RGB(int red, int green, int blue) => Ansi.SGR(38, 2, red, green, blue);

            private static string ColorToAnsi(Color color, bool bright = false)
                {
                    int code = (bright ? 90 : 30) + (int)color;
                    return Ansi.SGR(code);
                }
        }

    public class Background
        {
            public static string Black         => ColorToAnsi(Color.Black);
            public static string Red           => ColorToAnsi(Color.Red);
            public static string Green         => ColorToAnsi(Color.Green);
            public static string Yellow        => ColorToAnsi(Color.Yellow);
            public static string Blue          => ColorToAnsi(Color.Blue);
            public static string Magenta       => ColorToAnsi(Color.Magenta);
            public static string Cyan          => ColorToAnsi(Color.Cyan);
            public static string White         => ColorToAnsi(Color.White);

            public static string BrightBlack   => ColorToAnsi(Color.Black,   bright: true);
            public static string BrightRed     => ColorToAnsi(Color.Red,     bright: true);
            public static string BrightGreen   => ColorToAnsi(Color.Green,   bright: true);
            public static string BrightYellow  => ColorToAnsi(Color.Yellow,  bright: true);
            public static string BrightBlue    => ColorToAnsi(Color.Blue,    bright: true);
            public static string BrightMagenta => ColorToAnsi(Color.Magenta, bright: true);
            public static string BrightCyan    => ColorToAnsi(Color.Cyan,    bright: true);
            public static string BrightWhite   => ColorToAnsi(Color.White,   bright: true);

            public static string Index(int index)                  => Ansi.SGR(48, 5, index);
            public static string RGB(int red, int green, int blue) => Ansi.SGR(48, 2, red, green, blue);

            private static string ColorToAnsi(Color color, bool bright = false)
                {
                    int code = (bright ? 100 : 40) + (int)color;
                    return Ansi.SGR(code);
                }
        }
}