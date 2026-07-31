## Add-DynamicParameter

### Description

Adds a dynamic parameter to a PowerShell function or script. For use in advanced functions or scripts with dynamic parameters defined in the DynamicParameters() block.
For more examples of how to use this function, see the DynamicParam{} block in the [Start-AnsibleJobTemplate](../../../LTF-Ansible/Public/Controller/Job/Start-AnsibleJobTemplate.ps1) cmdlet in the LTF-AnsiblePS module.

---

```
Add-DynamicParameter

	[-DynamicParameterDictionary      <System.Management.Automation.RuntimeDefinedParameterDictionary[ ]>]
	[-ParameterName                                                                    <System.String[ ]>]
	[-CustomAttributes                                                            <System.Attribute[][ ]>]
	[-Mandatory                                         <System.Management.Automation.SwitchParameter[ ]>]
	[-DefaultValue                                                                     <System.Object[ ]>]
	[-Position                                                                          <System.Int32[ ]>]
	[-ParameterType                                                                      <System.Type[ ]>]
	[-HelpMessage                                                                      <System.String[ ]>]
	[-ParameterSetName                                                                 <System.String[ ]>]
	[-ValueFromPipeline                                 <System.Management.Automation.SwitchParameter[ ]>]
	[-ValueFromPipelineByPropertyName                   <System.Management.Automation.SwitchParameter[ ]>]
	[-ValidateSet                                                                    <System.String[][ ]>]
	[-ValidateRange                                                                  <System.String[][ ]>]
	[-ArgumentCompleter                                     <System.Management.Automation.ScriptBlock[ ]>]
	[<CommonParameters>]

```

---

### Source Code
[Add-DynamicParameter.ps1](../../../Private/Add-DynamicParameter.ps1)

### Examples

#### Example 1
```powershell
# Add a dynamic parameter named "ExampleParam" of type string
Add-DynamicParameter -DynamicParameterDictionary $DynamicParameters -ParameterName "ExampleParam" -ParameterType [string]
```


---

### Parameters

#### -ArgumentCompleter

Argument completer scriptblock for the generated dynamic parameter.

|           |                                          |
| --------- | ---------------------------------------- |
| Type      | System.Management.Automation.ScriptBlock |
| Mandatory | False                                    |


#### -CustomAttributes

Additional attributes to apply to the generated dynamic parameter.

|              |                    |
| ------------ | ------------------ |
| Type         | System.Attribute[] |
| Mandatory    | False              |
| DefaultValue | @()                |


#### -DefaultValue

Default value assigned to the generated dynamic parameter.

|              |               |
| ------------ | ------------- |
| Type         | System.Object |
| Mandatory    | False         |
| DefaultValue | $null         |


#### -DynamicParameterDictionary

Runtime defined parameter dictionary to add the parameter into.

|           |                                                                |
| --------- | -------------------------------------------------------------- |
| Type      | System.Management.Automation.RuntimeDefinedParameterDictionary |
| Mandatory | True                                                           |


#### -HelpMessage

Help message text for the generated dynamic parameter.

|           |               |
| --------- | ------------- |
| Type      | System.String |
| Mandatory | False         |


#### -Mandatory

Marks the generated dynamic parameter as mandatory.

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -ParameterName

Name of the dynamic parameter to create.

|           |               |
| --------- | ------------- |
| Type      | System.String |
| Mandatory | True          |


#### -ParameterSetName

Parameter set name for the generated dynamic parameter.

|           |               |
| --------- | ------------- |
| Type      | System.String |
| Mandatory | False         |


#### -ParameterType

Type of the dynamic parameter.

|           |             |
| --------- | ----------- |
| Type      | System.Type |
| Mandatory | True        |


#### -Position

Position assigned to the generated dynamic parameter.

|              |              |
| ------------ | ------------ |
| Type         | System.Int32 |
| Mandatory    | False        |
| DefaultValue | 0            |


#### -ValidateRange

ValidateRange bounds for the generated dynamic parameter.

|              |                 |
| ------------ | --------------- |
| Type         | System.String[] |
| Mandatory    | False           |
| DefaultValue | $null           |


#### -ValidateSet

ValidateSet values for the generated dynamic parameter.

|              |                 |
| ------------ | --------------- |
| Type         | System.String[] |
| Mandatory    | False           |
| DefaultValue | $null           |


#### -ValueFromPipeline

Sets ValueFromPipeline on the generated dynamic parameter.

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |


#### -ValueFromPipelineByPropertyName

Sets ValueFromPipelineByPropertyName on the generated dynamic parameter.

|           |                                              |
| --------- | -------------------------------------------- |
| Type      | System.Management.Automation.SwitchParameter |
| Mandatory | False                                        |
