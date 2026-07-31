## Split-LongString

### Description

Splits a long string into multiple lines based on the specified width and padding.

---

```
Split-LongString

	[-Text    <System.String[ ]>]
	[-Width    <System.Int32[ ]>]
	[-Padding  <System.Int32[ ]>]
	[<CommonParameters>]

```

---

### Source Code
[Split-LongString.ps1](../../../Private/Split-LongString.ps1)

### Examples

#### Example 1
```powershell
Split-LongString -Text $LongString -Width 80 -Padding 4
```


---

### Parameters

#### -Padding

The number of padding spaces to subtract from the width.

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 4            |


#### -Text

The long string to split into multiple lines.

|           |               |
| --------- | ------------- |
| Type      | System.String |
| Mandatory | True          |


#### -Width

The maximum width of each line.

|              |                        |
| ------------ | ---------------------- |
| Type         | System.Int32           |
| Mandatory    | False                  |
| DefaultValue | [Console]::WindowWidth |
