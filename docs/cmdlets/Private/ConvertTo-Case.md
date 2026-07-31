## ConvertTo-Case

### Description

Converts a string from one casing type to another

---

```
ConvertTo-Case

	[-InputString <System.String[ ]>]
	[-From        <System.String[ ]>]
	[-To          <System.String[ ]>]
	[<CommonParameters>]

```

---

### Source Code
[ConvertTo-Case.ps1](../../../Private/ConvertTo-Case.ps1)

### Examples

#### Example 1
```powershell
PS C:\> ConvertTo-Case -InputString 'PascalToSnakeCase' -From Pascal -To Snake
pascal_to_snake_case
```
#### Example 2
```powershell
PS C:\> ConvertTo-Case -InputString 'cascalToKebabCase' -From Camel -To Kebab
cascal-to-kebab-case
```
#### Example 3
```powershell
PS C:\> ConvertTo-Case -InputString 'snake_to_pascal_case' -From Snake -To Pascal
SnakeToPascalCase
```


---

### Parameters

#### -From

The casing type of the input string.

|             |                             |
| ----------- | --------------------------- |
| Type        | System.String               |
| Mandatory   | False                       |
| ValidateSet | Pascal, Camel, Snake, Kebab |


#### -InputString

The string to convert from one casing type to another.

|           |               |
| --------- | ------------- |
| Type      | System.String |
| Mandatory | True          |


#### -To

The casing type to convert the input string to.

|             |                             |
| ----------- | --------------------------- |
| Type        | System.String               |
| Mandatory   | False                       |
| ValidateSet | Pascal, Camel, Snake, Kebab |
