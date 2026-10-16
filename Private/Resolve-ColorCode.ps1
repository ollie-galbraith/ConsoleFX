Function Resolve-ColorCode
    {
        <#
        .SYNOPSIS
        Resolves a color code to a ConsoleFX color object for use in foreground or background coloring.
        .EXAMPLE
        Resolve-ColorCode -ColorCode 'Red' -ColorLocation 'Foreground'
        .EXAMPLE
        Resolve-ColorCode -ColorCode 'Magenta' -ColorLocation 'Background' -Vivid
        #>

        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$True, Position = 0, HelpMessage = 'The color code to resolve.')]
            [ConsoleFX.ValidateColor()]
            [Object]$ColorCode,
            [Parameter(Mandatory=$True, Position = 1, HelpMessage = 'The location of the color (foreground or background).')]
            [ValidateSet('Foreground', 'Background')]
            [String]$ColorLocation,
            [Parameter(Mandatory=$False, Position = 2, HelpMessage = 'Specifies if the color should be vivid.')]
            [Switch]$Vivid
        )

        $ConsoleFXColorModels = @(
            ([ConsoleFx.ColorModel.RGB]).Name,
            ([ConsoleFx.ColorModel.Hex]).Name,
            ([ConsoleFx.ColorModel.HSV]).Name,
            ([ConsoleFx.ColorModel.HSL]).Name,
            ([ConsoleFx.ColorModel.CMYK]).Name
        )

        Switch($ColorLocation)
            {
                Foreground
                    {
                        $ColorObject = New-Object ConsoleFX.Foreground
                    }
                Background
                    {
                        $ColorObject = New-Object ConsoleFX.Background
                    }
            }

        If($ColorCode -is [System.Collections.IEnumerable] -and $ColorCode.Count -eq 3)
            {
                $r, $g, $b = $ColorCode
                $RBGCode = $ColorObject::RGB($r, $g, $b)

                Return $RBGCode
            }
        ElseIf($ConsoleFXColorModels -contains $ColorCode.GetType().Name)
            {
                If($ColorCode -isnot [ConsoleFx.ColorModel.RGB])
                    {
                        $ColorConversionParams = @{
                            To = 'RGB'
                            ColorCode = $ColorCode
                        }
                        $ColorCode = Convert-Color @ColorConversionParams
                    }

                $RBGCode = $ColorObject::RGB($ColorCode.Red, $ColorCode.Green, $ColorCode.Blue)

                Return $RBGCode
            }

        ElseIf($ColorCode -is [int])
            {
                Return ($ColorObject::Index($ColorCode))
            }
        Else
            {
                $BrightMode = If($Vivid -eq $true){'Bright'}Else{''}

                $Color = "$BrightMode$ColorCode"
                Return ($ColorObject::$Color)
            }
    }