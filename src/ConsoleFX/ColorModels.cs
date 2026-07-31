using System;

namespace ConsoleFX.ColorModel
{
    internal static class ColorConvert
    {
        public static (double hue, double delta, double max, double min) HueFromRBG(
            double red,
            double green,
            double blue
        )
        {
            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));

            double delta = max - min;
            double hue = 0;

            if (delta == 0)
                hue = 0;
            else if (max == red)
            {
                hue = 60 * (((green - blue) / delta) % 6);
            }
            else if (max == green)
            {
                hue = 60 * (((blue - red) / delta) + 2);
            }
            else if (max == blue)
            {
                hue = 60 * (((red - green) / delta) + 4);
            }

            if (hue < 0)
                hue += 360;

            return (hue, delta, max, min);
        }

        public static RGB HueToRGB(double chroma, double mix, double brightness, int sector)
        {
            double red = 0;
            double green = 0;
            double blue = 0;

            switch (sector)
            {
                case 0:
                    red = chroma;
                    green = mix;
                    blue = 0;
                    break;
                case 1:
                    red = mix;
                    green = chroma;
                    blue = 0;
                    break;
                case 2:
                    red = 0;
                    green = chroma;
                    blue = mix;
                    break;
                case 3:
                    red = 0;
                    green = mix;
                    blue = chroma;
                    break;
                case 4:
                    red = mix;
                    green = 0;
                    blue = chroma;
                    break;
                case 5:
                    red = chroma;
                    green = 0;
                    blue = mix;
                    break;
            }

            return new RGB(
                (int)((red + brightness) * 255), // Red
                (int)((green + brightness) * 255), // Green
                (int)((blue + brightness) * 255) // Blue
            );
        }
    }

    internal static class Validate
    {
        public static void PercentageValue(int value, string name)
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value,
                    "Value must be between 0 and 100"
                );
            }
        }

        public static void DegreesValue(int value, string name)
        {
            if (value < 0 || value > 360)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value,
                    "Value must be between 0 and 360"
                );
            }
        }

        public static void BitValue(int value, string name)
        {
            if (value < 0 || value > 255)
            {
                throw new ArgumentOutOfRangeException(
                    name,
                    value,
                    "Value must be between 0 and 255"
                );
            }
        }
    }

    public class CMYK
    {
        public int Cyan { get; }
        public int Magenta { get; }
        public int Yellow { get; }
        public int Key { get; }

        public CMYK(int cyan, int magenta, int yellow, int key)
        {
            Validate.PercentageValue(cyan, nameof(Cyan));
            Validate.PercentageValue(magenta, nameof(Magenta));
            Validate.PercentageValue(yellow, nameof(Yellow));
            Validate.PercentageValue(key, nameof(Key));

            Cyan = cyan;
            Magenta = magenta;
            Yellow = yellow;
            Key = key;
        }

        public static CMYK FromArray(int[] values)
        {
            if (values == null || values.Length != 4)
            {
                throw new ArgumentException("Array must have 4 elements", nameof(values));
            }
            return new CMYK(values[0], values[1], values[2], values[3]);
        }

        public override string ToString()
        {
            return $"CMYK({Cyan},{Magenta},{Yellow},{Key})";
        }

        public RGB ToRGB()
        {
            return new RGB(
                (int)Math.Round(255 * (1 - ((double)Cyan / 100)) * (1 - ((double)Key / 100))),
                (int)Math.Round(255 * (1 - ((double)Magenta / 100)) * (1 - ((double)Key / 100))),
                (int)Math.Round(255 * (1 - ((double)Yellow / 100)) * (1 - ((double)Key / 100)))
            );
        }
    }

    public class Hex
    {
        public string Value { get; }

        public Hex(string value)
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2);
            }
            string cleanValue = value.TrimStart('#');
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleanValue, "^[0-9a-fA-F]{6}$"))
            {
                throw new ArgumentException("Invalid hex color format", cleanValue);
            }
            Value = cleanValue.ToUpper();
        }

        public override string ToString()
        {
            return Value;
        }

        public RGB ToRGB()
        {
            return new RGB(
                Convert.ToInt32(Value.Substring(0, 2), 16), // Red
                Convert.ToInt32(Value.Substring(2, 2), 16), // Green
                Convert.ToInt32(Value.Substring(4, 2), 16) // Blue
            );
        }
    }

    public class HSL
    {
        public int Hue { get; }
        public int Saturation { get; }
        public int Luminance { get; }

        public HSL(int hue, int saturation, int luminance)
        {
            Validate.DegreesValue(hue, nameof(Hue));
            Validate.PercentageValue(saturation, nameof(Saturation));
            Validate.PercentageValue(luminance, nameof(Luminance));

            Hue = hue;
            Saturation = saturation;
            Luminance = luminance;
        }

        public static HSL FromArray(int[] values)
        {
            if (values == null || values.Length != 3)
            {
                throw new ArgumentException("Array must have 3 elements", nameof(values));
            }
            return new HSL(values[0], values[1], values[2]);
        }

        public override string ToString()
        {
            return $"HSL({Hue},{Saturation},{Luminance})";
        }

        public RGB ToRGB()
        {
            double saturation = Saturation / 100.0;
            double luminance = Luminance / 100.0;

            double chroma = (1 - Math.Abs((2 * luminance) - 1)) * saturation;
            double mix = chroma * (1 - Math.Abs((Hue / 60.0 % 2) - 1));
            double brightness = luminance - (chroma / 2);

            int sector = (int)Math.Floor((double)(Hue % 360 / 60));
            return ColorConvert.HueToRGB(chroma, mix, brightness, sector);
        }
    }

    public class HSV
    {
        public int Hue { get; }
        public int Saturation { get; }
        public int Value { get; }

        public HSV(int hue, int saturation, int value)
        {
            Validate.DegreesValue(hue, nameof(Hue));
            Validate.PercentageValue(saturation, nameof(Saturation));
            Validate.PercentageValue(value, nameof(Value));

            Hue = hue;
            Saturation = saturation;
            Value = value;
        }

        public static HSV FromArray(int[] values)
        {
            if (values == null || values.Length != 3)
            {
                throw new ArgumentException("Array must have 3 elements", nameof(values));
            }
            return new HSV(values[0], values[1], values[2]);
        }

        public override string ToString()
        {
            return $"HSV({Hue},{Saturation},{Value})";
        }

        public RGB ToRGB()
        {
            double saturation = Saturation / 100.0;
            double value = Value / 100.0;

            double chroma = value * saturation;
            double mix = chroma * (1 - Math.Abs((Hue / 60.0 % 2) - 1));
            double brightness = value - chroma;

            int sector = (int)Math.Floor((double)(Hue % 360 / 60));
            return ColorConvert.HueToRGB(chroma, mix, brightness, sector);
        }
    }

    public class RGB
    {
        public int Red { get; }
        public int Green { get; }
        public int Blue { get; }

        public RGB(int red, int green, int blue)
        {
            Validate.BitValue(red, nameof(Red));
            Validate.BitValue(green, nameof(Green));
            Validate.BitValue(blue, nameof(Blue));

            Red = red;
            Green = green;
            Blue = blue;
        }

        public static RGB FromArray(int[] values)
        {
            if (values == null || values.Length != 3)
            {
                throw new ArgumentException("Array must have 3 elements", nameof(values));
            }
            return new RGB(values[0], values[1], values[2]);
        }

        public override string ToString()
        {
            return $"RGB({Red},{Green},{Blue})";
        }

        public CMYK ToCMYK()
        {
            double red = Red     / 255.0;
            double green = Green / 255.0;
            double blue = Blue   / 255.0;

            double black = 1 - Math.Max(red, Math.Max(green, blue));
            if (black == 1)
            {
                return new CMYK(0, 0, 0, 100);
            }

            double cyan    = (1 - red - black)   / (1 - black);
            double magenta = (1 - green - black) / (1 - black);
            double yellow  = (1 - blue - black)  / (1 - black);

            return new CMYK(
                (int)(cyan * 100),    // Cyan
                (int)(magenta * 100), // Magenta
                (int)(yellow * 100),  // Yellow
                (int)(black * 100)    // Key
            );
        }

        public Hex ToHex()
        {
            return new Hex($"{Red:X2}{Green:X2}{Blue:X2}");
        }

        public HSL ToHSL()
        {
            double red = Red     / 255.0;
            double green = Green / 255.0;
            double blue = Blue   / 255.0;

            var (hue, delta, max, min) = ColorConvert.HueFromRBG(red, green, blue);
            double luminance = (max + min) / 2;
            double saturation = 0;

            if (delta != 0)
            {
                if (luminance <= 0.5)
                {
                    saturation = (max - min) / (max + min);
                }
                if (luminance > 0.5)
                {
                    saturation =
                        (max - min) / (2 - max - min);
                }
            }

            return new HSL(
                (int)Math.Round(hue, 2), // Hue
                (int)Math.Round(saturation * 100, 2), // Saturation
                (int)Math.Round(luminance * 100, 2) // Luminance
            );
        }

        public HSV ToHSV()
        {
            double red = Red / 255.0;
            double green = Green / 255.0;
            double blue = Blue / 255.0;

            var (hue, delta, max, min) = ColorConvert.HueFromRBG(red, green, blue);
            double value = max;
            double saturation = 0;
            if (max != 0)
            {
                saturation = delta / max;
            }

            return new HSV(
                (int)Math.Round(hue, 2), // Hue
                (int)Math.Round(saturation * 100, 2), // Saturation
                (int)Math.Round(value * 100, 2) // Value
            );
        }
    }
}
