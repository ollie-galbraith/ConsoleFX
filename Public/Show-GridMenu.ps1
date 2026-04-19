Function Show-GridMenu
    {
        <#
        .SYNOPSIS
            Displays a grid menu in the console and allows the user to select an item. Show-GridMenu renders a grid menu in the console based on the provided items and configuration parameters. The user can navigate through the menu using arrow keys and select an item by pressing Enter. The function returns the selected item as a string.
        .EXAMPLE
            # Display a grid menu with a list of fruits and a title
            $fruits = @('Apple', 'Banana', 'Cherry', 'Date', 'Elderberry', 'Fig', 'Grape')
            $selectedFruit = Show-GridMenu -Items $fruits -Title "Select a Fruit"
            Write-Host "You selected: $selectedFruit"
        .EXAMPLE
            # Display a grid menu with custom colors and 3 columns
            $colors = @('Red', 'Green', 'Blue', 'Yellow', 'Cyan', 'Magenta')
            $selectedColor = Show-GridMenu -Items $colors -Columns 3 -TextColor 'White' -SelectionColor 'Black' -BorderColor 'Gray' -TitleColor 'Cyan' -Title "Select a Color"
            Write-Host "You selected: $selectedColor"
        #>

        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$true, HelpMessage='An array of strings to display as menu items')]
            [String[]]$Items,
            [Parameter(Mandatory=$False, HelpMessage='The title to display above the menu')]
            [String]$Title,
            [Parameter(Mandatory=$False, HelpMessage='The number of columns to display. Will automatically adjust based on console width if not specified')]
            [int]$Columns,
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
            [Parameter(Mandatory=$False, HelpMessage='Color for the border. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)')]
            [ConsoleFX.ValidateColor()]
            [ArgumentCompleter({
                param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
            })]
            [object]$BorderColor = 'Blue',
            [Parameter(Mandatory=$False, HelpMessage='Color for the border title. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)')]
            [ConsoleFX.ValidateColor()]
            [ArgumentCompleter({
                param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)
                [enum]::GetValues([ConsoleFX.Color]) | Where-Object {$_ -like "$wordToComplete*"} | ForEach-Object {$_}
            })]
            [object]$TitleColor = 'White',
            [Parameter(Mandatory=$false, HelpMessage='Whether to clear the console screen before rendering the menu')]
            [Bool]$ClearScreen = $True,
            [Parameter(Mandatory=$false, HelpMessage='Whether to expand the menu to the full width of the console')]
            [Bool]$FullWidth = $True
        )

        [ConsoleFX.Cursor]::Invisible
        $Menu = [ConsoleFX.GridMenu]::New($Items, $Columns, $TextColor, $SelectionColor, $true, $Title, $TitleColor, $BorderColor, $FullWidth, 'curved')
        $SelectedItem = $Menu.Render($ClearScreen)

        [ConsoleFX.Cursor]::Visible
        Return $SelectedItem
    }