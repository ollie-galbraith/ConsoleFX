## Read-TextInput

### Description

Reads text input from the user with various customization options.

---

```
Read-TextInput

	[-Prompt                                        <System.String[ ]>]
	[-InitialValue                                  <System.String[ ]>]
	[-VariableName                                  <System.String[ ]>]
	[-PassThru       <System.Management.Automation.SwitchParameter[ ]>]
	[-Border         <System.Management.Automation.SwitchParameter[ ]>]
	[-FullWidth                                    <System.Boolean[ ]>]
	[-Padding                                        <System.Int32[ ]>]
	[-StartRow                                       <System.Int32[ ]>]
	[-BorderInset                                    <System.Int32[ ]>]
	[-PromptInline   <System.Management.Automation.SwitchParameter[ ]>]
	[-AsSecureString <System.Management.Automation.SwitchParameter[ ]>]
	[-PromptColor                                   <System.Object[ ]>]
	[-TextColor                                     <System.Object[ ]>]
	[<CommonParameters>]

```

---

### Source Code
[Read-TextInput.ps1](../../Public/Read-TextInput.ps1)

### Examples

#### Example 1
```powershell
Read-TextInput -Prompt 'Enter your name:' -VariableName 'UserName'
```
#### Example 2
```powershell
Read-TextInput -Prompt 'Enter your password:' -AsSecureString -VariableName 'Password'
```
#### Example 3
```powershell
Read-TextInput -Prompt 'Enter your email:' -Border -Title 'User Information' -TitleColor 'Yellow' -BorderColor 'Green'
```


---

### Parameters

#### -AsSecureString

Mask the entered text and return the value as a SecureString

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -Border

Whether to render the input inside a border

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -BorderInset

Horizontal inset from each edge when FullWidth is true

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 0            |


#### -FullWidth

Whether to expand the input to the full width of the console

|              |                |
| ------------ | -------------- |
| Type         | System.Boolean |
| Mandatory    | False          |
| DefaultValue | $true          |


#### -InitialValue

Initial value to populate in the input field

|              |               |
| ------------ | ------------- |
| Type         | System.String |
| Mandatory    | False         |
| DefaultValue | ''            |


#### -Padding

Padding inside the border or around the rendered text

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 1            |


#### -PassThru

Also return the value when VariableName is supplied

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -Prompt

Prompt text shown above the input field

|           |               |
| --------- | ------------- |
| Type      | System.String |
| Mandatory | True          |


#### -PromptColor

Color for the prompt text

|              |               |
| ------------ | ------------- |
| Type         | System.Object |
| Mandatory    | False         |
| DefaultValue | 'Cyan'        |


#### -PromptInline

Render the input field on the same line as the prompt

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -StartRow

The row (0-based) at which the grid is rendered

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 0            |


#### -TextColor

Color for the entered text

|              |               |
| ------------ | ------------- |
| Type         | System.Object |
| Mandatory    | False         |
| DefaultValue | 'White'       |


#### -VariableName

Store the result in a variable in the caller scope instead of only returning it

|           |               |
| --------- | ------------- |
| Type      | System.String |
| Mandatory | False         |
