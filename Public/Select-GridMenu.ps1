Function Select-GridMenu
    {
        <#
        .SYNOPSIS
            Displays a grid menu in the console and allows the user to select an item. Select-GridMenu renders a grid menu in the console based on the provided items and configuration parameters. The user can navigate through the menu using arrow keys and select an item by pressing Enter. The function returns the selected item as a string.
        .EXAMPLE
            # Display a grid menu with a list of fruits and a title
            $fruits = @('Apple', 'Banana', 'Cherry', 'Date', 'Elderberry', 'Fig', 'Grape')
            $selectedFruit = Select-GridMenu -Items $fruits -Title "Select a Fruit"
            Write-Host "You selected: $selectedFruit"
        .EXAMPLE
            # Display a grid menu with custom colors and 3 columns
            $colors = @('Red', 'Green', 'Blue', 'Yellow', 'Cyan', 'Magenta')
            $selectedColor = Select-GridMenu -Items $colors -Columns 3 -TextColor 'White' -SelectionColor 'Black' -BorderColor 'Gray' -TitleColor 'Cyan' -Title "Select a Color"
            Write-Host "You selected: $selectedColor"
        #>

        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$True, HelpMessage='An array of strings to display as menu items')]
            [String[]]$Items,
            [Parameter(Mandatory=$False, HelpMessage='The number of columns to display. Will automatically adjust based on console width if too many specified')]
            [int]$Columns = 3,
            [Parameter(Mandatory=$False, HelpMessage='Renders the menu with a border')]
            [switch]$Border,
            [Parameter(Mandatory=$False, HelpMessage='Color for the text. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)')]
            [ConsoleFX.ValidateColor()]
            [ArgumentCompleter({
                param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
            })]
            [object]$TextColor = 'White',
            [Parameter(Mandatory=$False, HelpMessage='Color for the current selection. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)')]
            [ConsoleFX.ValidateColor()]
            [ArgumentCompleter({
                param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
            })]
            [object]$SelectionColor = 'Magenta',
            [Parameter(Mandatory=$false, HelpMessage='Whether to expand the menu to the full width of the console')]
            [Bool]$FullWidth = $True,
            [Parameter(Mandatory=$false, HelpMessage='The row (0-based) at which the grid is rendered')]
            [int]$StartRow = 0,
            [Parameter(Mandatory=$false, HelpMessage='Horizontal inset (chars) from each edge when FullWidth is true, keeping the border centered')]
            [int]$BorderInset = 0,
            [Parameter(Mandatory=$false, HelpMessage='Render the menu dimmed (for use as background when layering menus).')]
            [switch]$IsDimmed,
            [Parameter(Mandatory=$false, HelpMessage='A previously-returned dimmed GridMenu to render as background underlay behind this menu.')]
            [object]$UnderlayGridMenu,
            [Parameter(Mandatory=$false, HelpMessage='Return a PSCustomObject { Result; Menu } where Menu is a dimmed copy for chaining as underlay to the next menu.')]
            [switch]$PassThru
        )

        DynamicParam
            {
                $DynamicParameterDictionary = [System.Management.Automation.RuntimeDefinedParameterDictionary]::new()

                $NewDynamicParams = @{
                    DynamicParameterDictionary = $DynamicParameterDictionary
                }

                $BorderJson = Get-Content -Path ($MyInvocation.MyCommand.Module.FileList | Where-Object { $_ -like "*borders.json" } | Select-Object -First 1) -Raw -Encoding UTF8

                $ColorArguementCompleter = {
                    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                    [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
                }
                $ColorValidator = New-Object ConsoleFX.ValidateColorAttribute

                $BorderParamList = @(
                    @{
                        ParameterName    = 'Title'
                        HelpMessage      = 'The title to display above the menu'
                        DefaultValue     = $null
                        ParameterType    = ([string])
                        Mandatory        = $False
                        Position         = 0
                    },
                    @{
                        ParameterName     = 'TitleColor'
                        HelpMessage       = 'Color for the title. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)'
                        DefaultValue      = 'White'
                        ParameterType     = ([object])
                        CustomAttributes  = $ColorValidator
                        Mandatory         = $False
                        ArgumentCompleter = $ColorArguementCompleter
                        Position          = 0
                    },
                    @{
                        ParameterName    = 'BorderType'
                        HelpMessage      = 'The type of border to display'
                        DefaultValue     = 'Curved'
                        ParameterType    = ([string])
                        Mandatory        = $False
                        ValidateSet      = [array]($BorderJson | ConvertFrom-Json | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name | ConvertTo-Case -From Snake -To Pascal)
                        Position         = 0
                    },
                    @{
                        ParameterName     = 'BorderColor'
                        HelpMessage       = 'Color for the border. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)'
                        DefaultValue      = 'Blue'
                        ParameterType     = ([object])
                        CustomAttributes  = $ColorValidator
                        Mandatory         = $False
                        ArgumentCompleter = $ColorArguementCompleter
                        Position          = 0
                    }
                )

                If($Border)
                    {
                        ForEach($Param in $BorderParamList)
                            {
                                Add-DynamicParameter @NewDynamicParams @Param
                            }
                    }
                Return $DynamicParameterDictionary
            }

        Begin
            {
                $Title = $DynamicParameterDictionary['Title'].Value
                $BorderType = $DynamicParameterDictionary['BorderType'].Value | ConvertTo-Case -From Pascal -To Snake
                $BorderColor = $DynamicParameterDictionary['BorderColor'].Value

                $TitleColor = $DynamicParameterDictionary['TitleColor'].Value
            }

        Process
            {
                If($SelectionColor)
                    {
                        $ResolvedSelectionColor = Resolve-ColorCode -ColorCode $SelectionColor -ColorLocation 'Foreground'
                    }
                Else
                    {
                        $ResolvedSelectionColor = $SelectionColor
                    }

                If($TextColor)
                    {
                        $ResolvedTextColor = Resolve-ColorCode -ColorCode $TextColor -ColorLocation 'Foreground'
                    }
                Else
                    {
                        $ResolvedTextColor = $TextColor
                    }

                If($BorderColor)
                    {
                        $ResolvedBorderColor = Resolve-ColorCode -ColorCode $BorderColor -ColorLocation 'Foreground'
                    }
                Else
                    {
                        $ResolvedBorderColor = $BorderColor
                    }

                If($TitleColor)
                    {
                        $ResolvedTitleColor = Resolve-ColorCode -ColorCode $TitleColor -ColorLocation 'Foreground'
                    }
                Else
                    {
                        $ResolvedTitleColor = $TitleColor
                    }
            }

        End
            {
                [Console]::Write([ConsoleFX.Cursor]::HomePosition)
                [Console]::Write([ConsoleFX.Erase]::ToScreenEnd)
                $Menu = [ConsoleFX.GridMenu]::New($Items, $Columns, 1, $FullWidth, $Border, $BorderType, $Title, $StartRow, $BorderInset, $IsDimmed)
                $Menu.SetColor(
                    $ResolvedSelectionColor,
                    $ResolvedTextColor,
                    $ResolvedBorderColor,
                    $ResolvedTitleColor
                )
                If($UnderlayGridMenu)
                    {
                        $Menu.UnderlayMenu = $UnderlayGridMenu
                    }

                $SelectedItem = $Menu.Run()

                [ConsoleFX.Cursor]::Visible

                If($PassThru)
                    {
                        $DimmedMenu = [ConsoleFX.GridMenu]::New($Items, $Columns, 1, $FullWidth, $Border, $BorderType, $Title, $StartRow, $BorderInset, $true)
                        $DimmedMenu.SetColor(
                            $ResolvedSelectionColor,
                            $ResolvedTextColor,
                            $ResolvedBorderColor,
                            $ResolvedTitleColor
                        )
                        If($UnderlayGridMenu)
                            {
                                $DimmedMenu.UnderlayMenu = $UnderlayGridMenu
                            }
                        Return [PSCustomObject]@{
                            Result = $SelectedItem
                            Menu   = $DimmedMenu
                        }
                    }

                Return $SelectedItem
            }
    }