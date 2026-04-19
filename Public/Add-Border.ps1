Function Add-Border
    {
        <#
        .SYNOPSIS
            Takes input text and renders it within a styled border in the console. You can choose from various border styles, customize colors for the text, border, and title, and apply text formatting options. The function dynamically adjusts the border size based on the content and supports full-width borders that span the entire console width.
        .EXAMPLE
            PS C:\> "Hello, World!" | Add-Border -Title "Greeting" -TextColor Cyan -BorderColor Yellow -TitleColor Magenta -TextFormatting Bold, Italic
            ╭─ Greeting ────╮
            │ Hello, World! │
            ╰───────────────╯
        .EXAMPLE
            PS C:\> Add-Border "This is a longer string than before" -Title 'Example 2'
            ╭─ Example 2 ─────────────────────────╮
            │ This is a longer string than before │
            ╰─────────────────────────────────────╯
        .EXAMPLE
            PS C:\> Add-Border "Different border types" -Title 'Example 3' -Type Double
            ╔═ Example 3 ════════════╗
            ║ Different border types ║
            ╚════════════════════════╝
        .EXAMPLE
            PS C:\> Add-Border "This is a full width border" -Title 'Example 4' -FullWidth
            ╭─ Example 4 ─────────────────────────────────────────────────────────────────────────────╮
            │ This is a full width border                                                             │
            ╰─────────────────────────────────────────────────────────────────────────────────────────╯
        .EXAMPLE
            PS C:\> $FormattedString = @"
            >> This is a formatted string with some YAML
            >> ---
            >> list:
            >>   - item1
            >>   - item2
            >> dict:
            >>   key1: value1
            >>   key2: value2
            >>   list1:
            >>     - list_item1
            >>     - list_item2
            >> "@
            PS C:\>
            PS C:\> $FormattedString | Add-Border -Title "Formatted Strings"
            ╭─ Formatted Strings ───────────────────────╮
            │ This is a formatted string with some YAML │
            │ ---                                       │
            │ list:                                     │
            │   - item1                                 │
            │   - item2                                 │
            │ dict:                                     │
            │   key1: value1                            │
            │   key2: value2                            │
            │   list1:                                  │
            │     - list_item1                          │
            │     - list_item2                          │
            ╰───────────────────────────────────────────╯
        #>

        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$True, ValueFromPipeline=$True, ValueFromPipelineByPropertyName=$True, HelpMessage='The text to display inside the border. Can be a single string or an array of strings. If a single string is provided, it will be split into multiple lines if it exceeds the console width.')]
            [string[]]$Object,
            [Parameter(Mandatory=$False, ValueFromPipelineByPropertyName=$True, HelpMessage='The title to display in the border.')]
            [string]$Title,
            [Parameter(Mandatory=$False, HelpMessage='Color for the text. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)')]
            [ConsoleFX.ValidateColor()]
            [ArgumentCompleter({
                param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
            })]
            [object]$TextColor,
            [Parameter(Mandatory=$False, HelpMessage='Text formatting options to apply. Can be one or more ConsoleFX.Formatting enum values')]
            [ConsoleFX.FormatType[]]$TextFormatting,
            [Parameter(Mandatory=$False, HelpMessage='Color for the border. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)')]
            [ConsoleFX.ValidateColor()]
            [ArgumentCompleter({
                param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
            })]
            [object]$BorderColor,
            [Parameter(Mandatory=$False, HelpMessage='Color for the title. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)')]
            [ConsoleFX.ValidateColor()]
            [ArgumentCompleter({
                param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
            })]
            [object]$TitleColor,
            [Parameter(Mandatory=$False, HelpMessage='Text formatting options to apply. Can be one or more ConsoleFX.Formatting enum values')]
            [ConsoleFX.FormatType[]]$TitleFormatting,
            [Parameter(Mandatory=$False, ValueFromPipelineByPropertyName=$True, HelpMessage='Whether to use a full width border that spans the entire console width. If not specified, the border will be sized to fit the content with padding.')]
            [Switch]$FullWidth,
            [Parameter(Mandatory=$False, HelpMessage='The padding to add around the text inside the border. Default is 1')]
            [int]$Padding = 1,
            [Parameter(Mandatory=$False, HelpMessage='Do not perform word-wrapping or collapse whitespace. Use this when the input already contains the exact spacing you want to preserve.')]
            [Switch]$NoWrap,
            [Parameter(Mandatory=$False, HelpMessage='The X position of the border. Default is the current cursor position')]
            [Nullable[Int]]$XPosition,
            [Parameter(Mandatory=$False, HelpMessage='The Y position of the border. Default is the current cursor position')]
            [Nullable[Int]]$YPosition
        )

        DynamicParam
            {
                $DynamicParameterDictionary = [System.Management.Automation.RuntimeDefinedParameterDictionary]::new()

                $NewDynamicParams = @{
                    DynamicParameterDictionary = $DynamicParameterDictionary
                }

                $BorderJson = Get-Content -Path $MyInvocation.MyCommand.Module.FileList[1] -Raw -Encoding UTF8

                $ParamList = @(
                    @{
                        ParameterName    = 'Type'
                        DefaultValue     = 'Curved'
                        ParameterType    = ([string])
                        Mandatory        = $False
                        ValidateSet      = [array]($BorderJson | ConvertFrom-Json | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name | ConvertTo-Case -From Snake -To Pascal)
                        Position         = 0
                    }
                )

                ForEach($Param in $ParamList)
                    {
                        Add-DynamicParameter @NewDynamicParams @Param
                    }
                Return $DynamicParameterDictionary
            }

        Begin
            {
                $BorderType = $DynamicParameterDictionary['Type'].Value | ConvertTo-Case -From Pascal -To Snake
                $BorderManager = [ConsoleFX.BorderManager]::New()
                $BorderManager.LoadJson($BorderJson)
                $Border = $BorderManager.GetStyle($BorderType)

                [System.Collections.ArrayList]$BorderControlCodes = @()
                [System.Collections.ArrayList]$TextControlCodes = @()
                [System.Collections.ArrayList]$TitleControlCodes = @()

                $AccumulatedText = @()
            }
        Process
            {
                $AccumulatedText += $Object
            }
        End
            {
                If($NoWrap)
                    {
                        $TextArray = $AccumulatedText
                    }
                Else
                    {
                        $Joined = $AccumulatedText -join "`n"
                        $TextArray = $Joined | Split-LongString -Padding (2 + ($Padding * 2))
                    }

                $CharMatrix = [ConsoleFX.CharMatrix]::New($TextArray, $Padding, $FullWidth)

                If($BorderColor)
                    {
                        $BorderAnsiCode = Resolve-ColorCode -ColorCode $BorderColor -ColorLocation 'Foreground'
                    }

                $CharMatrix.SetBorder($Border, $BorderAnsiCode)

                If($TextFormatting -or $TextColor)
                    {
                        If($TextFormatting)
                            {
                                ForEach($Item in $TextFormatting)
                                    {
                                        [void]$TextControlCodes.Add([ConsoleFX.Format]::$Item)
                                    }
                            }
                        If($TextColor)
                            {
                                [void]$TextControlCodes.Add((Resolve-ColorCode -ColorCode $TextColor -ColorLocation 'Foreground'))
                            }

                        $CharMatrix.SetText($TextArray, ($TextControlCodes -join ''))
                    }

                If($Title)
                    {
                        If($TitleFormatting -or $TitleColor)
                            {
                                If($TitleFormatting)
                                    {
                                        ForEach($Item in $TitleFormatting)
                                            {
                                                [void]$TitleControlCodes.Add([ConsoleFX.Format]::$Item)
                                            }
                                    }
                                If($TitleColor)
                                    {
                                        [void]$TitleControlCodes.Add((Resolve-ColorCode -ColorCode $TitleColor -ColorLocation 'Foreground'))
                                    }
                            }

                        $CharMatrix.SetTitle($Title, ($TitleControlCodes -join ''))
                    }

                $CharMatrix.Render($XPosition, $YPosition)
            }
    }