## Convert-Color

### Description

Converts a color code to a different color model.

---

```
Convert-Color

	[-ColorCode <System.Object[ ]>]
	[-To        <System.String[ ]>]
	[<CommonParameters>]

```

---

### Source Code
[Convert-Color.ps1](../Public/Convert-Color.ps1)

### Examples

#### Example 1
```powershell
Convert-Color -ColorCode $Color -To 'RGB'
```

---

### Parameters

#### -ColorCode

The color code to convert.

|           |               |
| --------- | ------------- |
| Type      | System.Object |
| Mandatory | True          |


#### -To

The color model to convert the color code to.

|             |                          |
| ----------- | ------------------------ |
| Type        | System.String            |
| Mandatory   | False                    |
| ValidateSet | RGB, Hex, HSV, HSL, CMYK |
