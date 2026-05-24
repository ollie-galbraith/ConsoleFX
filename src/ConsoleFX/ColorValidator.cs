using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;

// Assuming these exist in your project based on your snippet
using ConsoleFX.ColorModel; 

namespace ConsoleFX
{
    public class ValidateColorAttribute : ValidateArgumentsAttribute
    {
        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            // 1. Check for null
            if (arguments == null)
            {
                throw new ValidationMetadataException("Color argument cannot be null.");
            }

            // 2. Handle single Integer (0-255)
            if (arguments is int intValue)
            {
                if (intValue < 0 || intValue > 255)
                {
                    throw new ValidationMetadataException($"{intValue} is not a valid color integer (0-255).");
                }
                return;
            }

            // 3. Handle IEnumerable (Arrays/Lists) but not strings
            if (arguments is IEnumerable enumerable && !(arguments is string))
            {
                // Convert to object array for easier counting/indexing
                object[] array = enumerable.Cast<object>().ToArray();

                if (array.Length != 3)
                {
                    throw new ValidationMetadataException($"Color array must have exactly 3 values (RGB). You provided {array.Length}.");
                }

                foreach (object item in array)
                {
                    if (item is int rgbValue)
                    {
                        if (rgbValue < 0 || rgbValue > 255)
                        {
                            throw new ValidationMetadataException($"RGB value '{rgbValue}' is out of range (0-255).");
                        }
                    }
                    else
                    {
                        throw new ValidationMetadataException($"'{item}' is not an integer.");
                    }
                }
                return;
            }

            // 4. Handle String (Enum check)
            if (arguments is string stringValue)
            {
                stringValue = stringValue.Trim();
                // Using Enum.TryParse with ignoreCase = true to allow for case-insensitive matching
                if (!Enum.TryParse<ConsoleFX.Color>(stringValue, true, out _))
                {
                    throw new ValidationMetadataException($"'{stringValue}' is not a valid [ConsoleFX.Color] color.");
                }
                return;
            }

            // 5. Handle ConsoleFX.Color enum values directly
            if (arguments is ConsoleFX.Color)
            {
                return;
            }

            // 6. Handle Specific Color Models
            if (arguments is RGB || 
                arguments is Hex || 
                arguments is HSV || 
                arguments is HSL || 
                arguments is CMYK)
            {
                return;
            }

            // 6. Fallback Error
            throw new ValidationMetadataException("Invalid color type. Must be a valid Color Model (RGB, Hex, etc), a single int (0-255), an RGB int array, or a valid ConsoleFX.Colors string.");
        }
    }
}