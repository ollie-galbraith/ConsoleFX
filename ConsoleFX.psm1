$assemblyPath = Join-Path $PSScriptRoot "lib/ConsoleFX.dll"
$PublicPath = "$PSScriptRoot\Public"

$DotSourcedPaths = @(
    "$PSScriptRoot\Private",
    $PublicPath
)

Add-Type -Path $assemblyPath

$PublicMembers = Get-ChildItem -Path $PublicPath | ForEach-Object {$_.Name -replace ".ps1"}
$DotSourceMembers = Get-ChildItem -Path $DotSourcedPaths -Include "*.ps1" -Recurse -ErrorAction SilentlyContinue

ForEach($File in $DotSourceMembers)
    {
        . $File.FullName
    }

Export-ModuleMember -Function $PublicMembers

