Function Resolve-ColorCode
    {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$True)]
            [ConsoleFX.ValidateColor()]
            [Object]$ColorCode,
            [Parameter(Mandatory=$True)]
            [ValidateSet('Foreground', 'Background')]
            [String]$ColorLocation,
            [Parameter(Mandatory=$False)]
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
        ElseIf($ConsoleFXColorModels -contains $ColorCode.GetType())
            {
                If($ColorCode -isnot [ConsoleFx.ColorModel.RGB])
                    {
                        $Type = $ColorCode.GetType().Name
                        $ColorConversionParams = @{
                            To = 'RGB'
                        }
                        $ColorConversionParams[$Type] = $ColorCode
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