Function Convert-Color
    {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$True)]
            [ConsoleFX.ValidateColor()]
            [Object]$ColorCode,
            [Parameter(Mandatory=$True)]
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