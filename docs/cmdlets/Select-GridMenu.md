## Select-GridMenu

### Description

Displays a grid menu in the console and allows the user to select an item. Select-GridMenu renders a grid menu in the console based on the provided items and configuration parameters. The user can navigate through the menu using arrow keys and select an item by pressing Enter. The function returns the selected item as a string.

---

```
Select-GridMenu

	[-Items                                         <System.String[][ ]>]
	[-Columns                                          <System.Int32[ ]>]
	[-Border           <System.Management.Automation.SwitchParameter[ ]>]
	[-TextColor                                       <System.Object[ ]>]
	[-SelectionColor                                  <System.Object[ ]>]
	[-FullWidth                                      <System.Boolean[ ]>]
	[-StartRow                                         <System.Int32[ ]>]
	[-BorderInset                                      <System.Int32[ ]>]
	[-IsDimmed         <System.Management.Automation.SwitchParameter[ ]>]
	[-UnderlayGridMenu                                <System.Object[ ]>]
	[-PassThru         <System.Management.Automation.SwitchParameter[ ]>]
	[<CommonParameters>]

```

---

### Source Code
[Select-GridMenu.ps1](../Public/Select-GridMenu.ps1)

### Examples

#### Example 1
```powershell
# Display a grid menu with a list of fruits and a title
$fruits = @('Apple', 'Banana', 'Cherry', 'Date', 'Elderberry', 'Fig', 'Grape')
$selectedFruit = Select-GridMenu -Items $fruits -Title "Select a Fruit"
Write-Host "You selected: $selectedFruit"
```
#### Example 2
```powershell
# Display a grid menu with custom colors and 3 columns
$colors = @('Red', 'Green', 'Blue', 'Yellow', 'Cyan', 'Magenta')
$selectedColor = Select-GridMenu -Items $colors -Columns 3 -TextColor 'White' -SelectionColor 'Black' -BorderColor 'Gray' -TitleColor 'Cyan' -Title "Select a Color"
Write-Host "You selected: $selectedColor"
```

---

### Parameters

#### -Border

Renders the menu with a border

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -BorderInset

Horizontal inset (chars) from each edge when FullWidth is true, keeping the border centered

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 0            |


#### -Columns

The number of columns to display. Will automatically adjust based on console width if too many specified

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 3            |


#### -FullWidth

Whether to expand the menu to the full width of the console

|              |                |
| ------------ | -------------- |
| Type         | System.Boolean |
| Mandatory    | False          |
| DefaultValue | $True          |


#### -IsDimmed

Render the menu dimmed (for use as background when layering menus).

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -Items

An array of strings to display as menu items

|           |                 |
| --------- | --------------- |
| Type      | System.String[] |
| Mandatory | True            |


#### -PassThru

Return a PSCustomObject { Result; Menu } where Menu is a dimmed copy for chaining as underlay to the next menu.

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -SelectionColor

Color for the current selection. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)

|              |               |
| ------------ | ------------- |
| Type         | System.Object |
| Mandatory    | False         |
| DefaultValue | 'Magenta'     |


#### -StartRow

The row (0-based) at which the grid is rendered

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 0            |


#### -TextColor

Color for the text. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)

|              |               |
| ------------ | ------------- |
| Type         | System.Object |
| Mandatory    | False         |
| DefaultValue | 'White'       |


#### -UnderlayGridMenu

A previously-returned dimmed GridMenu to render as background underlay behind this menu.

|           |               |
| --------- | ------------- |
| Type      | System.Object |
| Mandatory | False         |
