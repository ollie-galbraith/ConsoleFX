Function ConvertTo-Case
    {
        <#
        .SYNOPSIS
        Converts a string from one casing type to another
        .EXAMPLE
        PS C:\> ConvertTo-Case -InputString 'PascalToSnakeCase' -From Pascal -To Snake
        pascal_to_snake_case
        .EXAMPLE
        PS C:\> ConvertTo-Case -InputString 'cascalToKebabCase' -From Camel -To Kebab
        cascal-to-kebab-case
        .EXAMPLE
        PS C:\> ConvertTo-Case -InputString 'snake_to_pascal_case' -From Snake -To Pascal
        SnakeToPascalCase
        #>
        
        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
            [string]$InputString,
            [Parameter(Mandatory=$true)]
            [ValidateSet('Pascal', 'Camel', 'Snake', 'Kebab')]
            [String]$From,
            [Parameter(Mandatory=$true)]
            [ValidateSet('Pascal', 'Camel', 'Snake', 'Kebab')]
            [String]$To
        )

        Begin
            {
                If($From -eq $To)
                    {
                        Write-Error "No point converting to the same case"
                        Break
                    }

                $DetectionRegexMap = @{
                    Pascal = '(?<=[0-9a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])'
                    Camel = '(?<=[0-9a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])'
                    Snake = '_+'
                    Kebab = '-+'
                }

                $ReplacementRegexMap = @{
                    Snake = '_'
                    Kebab = '-'
                }
            }

        Process
            {
                $StringSplit = [regex]::Split($InputString, $DetectionRegexMap[$From])
                
                Switch($To)
                    {
                        'Pascal'
                            {
                                $Converted = ($StringSplit | ForEach-Object {
                                    $_.Substring(0,1).ToUpper() + $_.Substring(1).ToLower()
                                }) -join ''
                            }
                        'Camel'
                            {
                                $FirstWord = $StringSplit[0].ToLower()
                                $RemainingWords = $StringSplit[1..($StringSplit.Count - 1)] | ForEach-Object {
                                    $_.Substring(0,1).ToUpper() + $_.Substring(1).ToLower()
                                }
                                $Converted = @($FirstWord) + $RemainingWords -join ''
                            }
                        'Snake'
                            {
                                $StringJoin = $StringSplit -join $ReplacementRegexMap[$To]
                                $Converted = $StringJoin.ToLower()
                            }
                        'Kebab'
                            {
                                $StringJoin = $StringSplit -join $ReplacementRegexMap[$To]
                                $Converted = $StringJoin.ToLower()
                            }
                    }

                Return $Converted
            }
    }