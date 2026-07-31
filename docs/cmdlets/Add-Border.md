## Add-Border

### Description

Takes input text and renders it within a styled border in the console. You can choose from various border styles, customize colors for the text, border, and title, and apply text formatting options. The function dynamically adjusts the border size based on the content and supports full-width borders that span the entire console width.

---

```
Add-Border

	[-Object                                                                                                                        <System.String[][ ]>]
	[-Title                                                                                                                           <System.String[ ]>]
	[-TextColor                                                                                                                       <System.Object[ ]>]
	[-TextFormatting                                                                                                         <ConsoleFX.FormatType[][ ]>]
	[-BorderColor                                                                                                                     <System.Object[ ]>]
	[-TitleColor                                                                                                                      <System.Object[ ]>]
	[-TitleFormatting                                                                                                        <ConsoleFX.FormatType[][ ]>]
	[-FullWidth                                                                                        <System.Management.Automation.SwitchParameter[ ]>]
	[-Padding                                                                                                                          <System.Int32[ ]>]
	[-NoWrap                                                                                           <System.Management.Automation.SwitchParameter[ ]>]
	[-XPosition       <System.Nullable`1[[System.Int32, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]][ ]>]
	[-YPosition       <System.Nullable`1[[System.Int32, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]][ ]>]
	[-Type                                                                                                                                   <String[ ]>]
	[<CommonParameters>]

```

---

### Source Code
[Add-Border.ps1](../Public/Add-Border.ps1)

### Examples

#### Example 1
```powershell
PS C:\> "Hello, World!" | Add-Border -Title "Greeting" -TextColor Cyan -BorderColor Yellow -TitleColor Magenta -TextFormatting Bold, Italic
╭─ Greeting ────╮
│ Hello, World! │
╰───────────────╯
```
#### Example 2
```powershell
PS C:\> Add-Border "This is a longer string than before" -Title 'Example 2'
╭─ Example 2 ─────────────────────────╮
│ This is a longer string than before │
╰─────────────────────────────────────╯
```
#### Example 3
```powershell
PS C:\> Add-Border "Different border types" -Title 'Example 3' -Type Double
╔═ Example 3 ════════════╗
║ Different border types ║
╚════════════════════════╝
```
#### Example 4
```powershell
PS C:\> Add-Border "This is a full width border" -Title 'Example 4' -FullWidth
╭─ Example 4 ─────────────────────────────────────────────────────────────────────────────╮
│ This is a full width border                                                             │
╰─────────────────────────────────────────────────────────────────────────────────────────╯
```
#### Example 5
```powershell
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
```

---

### Parameters

#### -BorderColor

Color for the border. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)

|           |               |
| --------- | ------------- |
| Type      | System.Object |
| Mandatory | False         |


#### -FullWidth

Whether to use a full width border that spans the entire console width. If not specified, the border will be sized to fit the content with padding.

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -NoWrap

Do not perform word-wrapping or collapse whitespace. Use this when the input already contains the exact spacing you want to preserve.

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -Object

The text to display inside the border. Can be a single string or an array of strings. If a single string is provided, it will be split into multiple lines if it exceeds the console width.

|           |                 |
| --------- | --------------- |
| Type      | System.String[] |
| Mandatory | True            |


#### -Padding

The padding to add around the text inside the border. Default is 1

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 1            |


#### -TextColor

Color for the text. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)

|           |               |
| --------- | ------------- |
| Type      | System.Object |
| Mandatory | False         |


#### -TextFormatting

Text formatting options to apply. Can be one or more ConsoleFX.Formatting enum values

|           |                        |
| --------- | ---------------------- |
| Type      | ConsoleFX.FormatType[] |
| Mandatory | False                  |


#### -Title

The title to display in the border.

|           |               |
| --------- | ------------- |
| Type      | System.String |
| Mandatory | False         |


#### -TitleColor

Color for the title. Can be a ConsoleFX.Colors enum value, a ConsoleFX.ColorModel object, a 256-bit color integer (0-255), or an RGB array of three integers (0-255)

|           |               |
| --------- | ------------- |
| Type      | System.Object |
| Mandatory | False         |


#### -TitleFormatting

Text formatting options to apply. Can be one or more ConsoleFX.Formatting enum values

|           |                        |
| --------- | ---------------------- |
| Type      | ConsoleFX.FormatType[] |
| Mandatory | False                  |


#### -Type
|                   |                    |
| ----------------- | ------------------ |
| Type              | String             |
| Mandatory         | False              |
| ParameterSetNames | __AllParameterSets |


#### -XPosition

The X position of the border. Default is the current cursor position

|           |                                                                                                                               |
| --------- | ----------------------------------------------------------------------------------------------------------------------------- |
| Type      | System.Nullable`1[[System.Int32, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]] |
| Mandatory | False                                                                                                                         |


#### -YPosition

The Y position of the border. Default is the current cursor position

|           |                                                                                                                               |
| --------- | ----------------------------------------------------------------------------------------------------------------------------- |
| Type      | System.Nullable`1[[System.Int32, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]] |
| Mandatory | False                                                                                                                         |
