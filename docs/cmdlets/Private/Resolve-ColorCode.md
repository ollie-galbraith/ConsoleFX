## Resolve-ColorCode

### Description

Resolves a color code to a ConsoleFX color object for use in foreground or background coloring.

---

```
Resolve-ColorCode

	[-ColorCode                                    <System.Object[ ]>]
	[-ColorLocation                                <System.String[ ]>]
	[-Vivid         <System.Management.Automation.SwitchParameter[ ]>]
	[<CommonParameters>]

```

---

### Source Code
[Resolve-ColorCode.ps1](../../../Private/Resolve-ColorCode.ps1)

### Examples

#### Example 1
```powershell
Resolve-ColorCode -ColorCode 'Red' -ColorLocation 'Foreground'
```
#### Example 2
```powershell
Resolve-ColorCode -ColorCode 'Magenta' -ColorLocation 'Background' -Vivid
```


---

### Parameters

#### -ColorCode

The color code to resolve.

|           |               |
| --------- | ------------- |
| Type      | System.Object |
| Mandatory | True          |


#### -ColorLocation

The location of the color (foreground or background).

|             |                        |
| ----------- | ---------------------- |
| Type        | System.String          |
| Mandatory   | False                  |
| ValidateSet | Foreground, Background |


#### -Vivid

Specifies if the color should be vivid.

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |
