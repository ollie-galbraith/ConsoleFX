Function Add-DynamicParameter
    {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$True)]
            [System.Management.Automation.RuntimeDefinedParameterDictionary]$DynamicParameterDictionary,
            [Parameter(Mandatory=$True)]
            [string]$ParameterName,
            [Parameter(Mandatory=$True)]
            [Type]$ParameterType,
            [Parameter(Mandatory=$False)]
            [string]$HelpMessage,
            [Parameter(Mandatory=$False)]
            [string]$ParameterSetName,
            [Parameter(Mandatory=$False)]
            [Switch]$ValueFromPipeline,
            [Parameter(Mandatory=$False)]
            [Switch]$ValueFromPipelineByPropertyName,
            [Parameter(Mandatory=$False)]
            [string[]]$ValidateSet = $null,
            [Parameter(Mandatory=$False)]
            [string[]]$ValidateRange = $null,
            [Parameter(Mandatory=$False)]
            [scriptblock]$ArgumentCompleter,
            [Parameter(Mandatory=$False)]
            [System.Attribute[]]$CustomAttributes = @(),
            [Parameter(Mandatory=$False)]
            [Switch]$Mandatory,
            [Parameter(Mandatory=$False)]
            $DefaultValue = $null,
            [Parameter(Mandatory=$False)]
            [int]$Position = 0
        )

        $Attributes = New-Object System.Collections.ObjectModel.Collection[System.Attribute]

        $ParamAttribute = New-Object System.Management.Automation.ParameterAttribute
        $ParamAttribute.Mandatory = $Mandatory
        $ParamAttribute.Position = $Position
        $ParamAttribute.ValueFromPipeline = $ValueFromPipeline
        $ParamAttribute.ValueFromPipelineByPropertyName = $ValueFromPipelineByPropertyName
        
        If([String]::IsNullOrEmpty($HelpMessage) -eq $False)
            {
                $ParamAttribute.HelpMessage = $HelpMessage
            }

        If([String]::IsNullOrEmpty($ParameterSetName) -eq $false)
            {
                $ParamAttribute.ParameterSetName = $ParameterSetName
            }

        $Attributes.Add($ParamAttribute)

        ForEach($Attribute in $CustomAttributes)
            {
                $Attributes.Add($Attribute)
            }

        If($ValidateSet)
            {
                $ValidateSetAttribute = New-Object System.Management.Automation.ValidateSetAttribute($ValidateSet)
                $Attributes.Add($ValidateSetAttribute)
            }
        If($ValidateRange)
            {
                $ValidateRangeAttribute = New-Object System.Management.Automation.ValidateRangeAttribute($ValidateRange)
                $Attributes.Add($ValidateRangeAttribute)
            }
        If($ArgumentCompleter)
            {
                $CompleterAttribute = New-Object System.Management.Automation.ArgumentCompleterAttribute($ArgumentCompleter)
                $Attributes.Add($CompleterAttribute)
            }

        $RuntimeDefinedParameter = [System.Management.Automation.RuntimeDefinedParameter]::new($ParameterName, $ParameterType, $Attributes)
        If($null -ne $DefaultValue)
            {
                $RuntimeDefinedParameter.Value = $DefaultValue
            }

        $DynamicParameterDictionary.Add($ParameterName, $RuntimeDefinedParameter)
    }