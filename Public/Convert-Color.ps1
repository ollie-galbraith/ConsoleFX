Function Convert-Color
    {
        <#
        .SYNOPSIS
        Converts a color code to a different color model.
        .EXAMPLE
        Convert-Color -ColorCode $Color -To 'RGB'
        #>
        
        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$True, Position = 0, HelpMessage = 'The color code to convert.')]
            [ConsoleFX.ValidateColor()]
            [Object]$ColorCode,
            [Parameter(Mandatory=$True, Position = 1, HelpMessage = 'The color model to convert the color code to.')]
            [ValidateSet('RGB', 'Hex', 'HSV', 'HSL', 'CMYK')]
            [String]$To
        )

        $ConsoleFXColorModels = @(
            ([ConsoleFx.ColorModel.RGB]).Name,
            ([ConsoleFx.ColorModel.Hex]).Name,
            ([ConsoleFx.ColorModel.HSV]).Name,
            ([ConsoleFx.ColorModel.HSL]).Name,
            ([ConsoleFx.ColorModel.CMYK]).Name
        )

        If($To -eq $ColorCode.GetType().Name)
            {
                Return $ColorCode
            }
        ElseIf($To -eq 'RGB')
            {
                Return $ColorCode.ToRGB()
            }
        Else
            {
                Switch($To)
                    {
                        Hex
                            {
                                Return $ColorCode.ToRGB().ToHex()
                            }
                        HSV
                            {
                                Return $ColorCode.ToRGB().ToHSV()
                            }
                        HSL
                            {
                                Return $ColorCode.ToRGB().ToHSL()
                            }
                        CMYK
                            {
                                Return $ColorCode.ToRGB().ToCMYK()
                            }
                    }
            }
    }