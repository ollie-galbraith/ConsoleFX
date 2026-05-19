Function Read-TextInput
    {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$true, HelpMessage='Prompt text shown above the input field')]
            [string]$Prompt,
            [Parameter(Mandatory=$false, HelpMessage='Initial value to populate in the input field')]
            [string]$InitialValue = '',
            [Parameter(Mandatory=$false, HelpMessage='Store the result in a variable in the caller scope instead of only returning it')]
            [string]$VariableName,
            [Parameter(Mandatory=$false, HelpMessage='Also return the value when VariableName is supplied')]
            [switch]$PassThru,
            [Parameter(Mandatory=$false, HelpMessage='Whether to render the input inside a border')]
            [switch]$Border,
            [Parameter(Mandatory=$false, HelpMessage='Whether to expand the input to the full width of the console')]
            [bool]$FullWidth = $true,
            [Parameter(Mandatory=$false, HelpMessage='Padding inside the border or around the rendered text')]
            [int]$Padding = 1,
            [Parameter(Mandatory=$false, HelpMessage='The row (0-based) at which the grid is rendered')]
            [int]$StartRow = 0,
            [Parameter(Mandatory=$false, HelpMessage='Horizontal inset from each edge when FullWidth is true')]
            [int]$BorderInset = 0,
            [Parameter(Mandatory=$false, HelpMessage='Render the input field on the same line as the prompt')]
            [switch]$PromptInline,
            [Parameter(Mandatory=$false, HelpMessage='Mask the entered text and return the value as a SecureString')]
            [switch]$AsSecureString,
            [Parameter(Mandatory=$false, HelpMessage='Color for the prompt text')]
            [ConsoleFX.ValidateColor()]
            [object]$PromptColor = 'Cyan',
            [Parameter(Mandatory=$false, HelpMessage='Color for the entered text')]
            [ConsoleFX.ValidateColor()]
            [object]$TextColor = 'White'
        )

        DynamicParam
            {
                $DynamicParameterDictionary = [System.Management.Automation.RuntimeDefinedParameterDictionary]::new()

                $NewDynamicParams = @{
                    DynamicParameterDictionary = $DynamicParameterDictionary
                }

                $BorderJson = Get-Content -Path $MyInvocation.MyCommand.Module.FileList[1] -Raw -Encoding UTF8
                $ColorArguementCompleter = {
                    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                    [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
                }
                $ColorValidator = New-Object ConsoleFX.ValidateColorAttribute

                if ($Border)
                    {
                        $BorderParamList = @(
                            @{
                                ParameterName = 'Title'
                                ParameterType = ([string])
                                Mandatory = $false
                            },
                            @{
                                ParameterName = 'TitleColor'
                                ParameterType = ([object])
                                Mandatory = $false
                                DefaultValue = 'White'
                                CustomAttributes = $ColorValidator
                                ArgumentCompleter = $ColorArguementCompleter
                            },
                            @{
                                ParameterName = 'BorderType'
                                ParameterType = ([string])
                                Mandatory = $false
                                DefaultValue = 'Curved'
                                ValidateSet = [array]($BorderJson | ConvertFrom-Json | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name | ConvertTo-Case -From Snake -To Pascal)
                            },
                            @{
                                ParameterName = 'BorderColor'
                                ParameterType = ([object])
                                Mandatory = $false
                                DefaultValue = 'Blue'
                                CustomAttributes = $ColorValidator
                                ArgumentCompleter = $ColorArguementCompleter
                            }
                        )

                        foreach ($Param in $BorderParamList)
                            {
                                Add-DynamicParameter @NewDynamicParams @Param
                            }
                    }

                return $DynamicParameterDictionary
            }

        Begin
            {
                $Title = $null
                $BorderType = $null
                $BorderColor = $null
                $TitleColor = $null

                if ($Border)
                    {
                        $Title = $DynamicParameterDictionary['Title'].Value
                        $BorderType = $DynamicParameterDictionary['BorderType'].Value | ConvertTo-Case -From Pascal -To Snake
                        $BorderColor = $DynamicParameterDictionary['BorderColor'].Value
                        $TitleColor = $DynamicParameterDictionary['TitleColor'].Value
                    }
            }

        End
            {
                $ResolvedPromptColor = Resolve-ColorCode -ColorCode $PromptColor -ColorLocation 'Foreground'
                $ResolvedTextColor = Resolve-ColorCode -ColorCode $TextColor -ColorLocation 'Foreground'
                $ResolvedBorderColor = if ($BorderColor) { Resolve-ColorCode -ColorCode $BorderColor -ColorLocation 'Foreground' } else { $null }
                $ResolvedTitleColor = if ($TitleColor) { Resolve-ColorCode -ColorCode $TitleColor -ColorLocation 'Foreground' } else { $null }

                $Input = [ConsoleFX.TextInput]::New($Prompt, $Padding, $FullWidth, $Border, $BorderType, $Title, $StartRow, 0, $BorderInset, $PromptInline, $AsSecureString)
                $Input.SetColor($ResolvedPromptColor, $ResolvedTextColor, $ResolvedBorderColor, $ResolvedTitleColor)
                $Result = $Input.Run($InitialValue)

                if ($VariableName)
                    {
                        Set-Variable -Name $VariableName -Value $Result -Scope 1
                    }

                if (-not $VariableName -or $PassThru)
                    {
                        return $Result
                    }
            }
    }