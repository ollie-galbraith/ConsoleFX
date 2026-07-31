## Show-LayeredGridMenu

### Description

Displays a stack of grid menus based on a list of menu definition objects.
Menus are rendered in ascending order by Index when provided; otherwise original list order is used.
Each completed menu is redrawn dimmed in the background while the next menu is shown over the top.
Pressing Escape on a menu goes back one menu (if available).

---

```
Show-LayeredGridMenu

	[-MenuItems <System.Object[][ ]>]
	[<CommonParameters>]

```

---

### Source Code
[Show-LayeredGridMenu.ps1](../../Public/Show-LayeredGridMenu.ps1)

### Examples

#### Example 1
```powershell
$MenuItems = @(
    @{
        Index = 0
        Title = "FirstMenu"
        Items = @('Choice 1', 'Choice 2', 'Choice 3')
        StartRow = 0
        BorderInset = 0
        Border = $true
    },
    @{
        Index = 1
        Title = "SecondMenu"
        Items = @('Choice 1', 'Choice 2', 'Choice 3')
        StartRow = 2
        BorderInset = 4
        Border = $true
    }
)

Show-LayeredGridMenu -MenuItems $MenuItems
```


---

### Parameters

#### -MenuItems

An array of menu definition objects. Each object should contain properties for Title, Items, StartRow, BorderInset, and Border.

|           |                 |
| --------- | --------------- |
| Type      | System.Object[] |
| Mandatory | True            |
