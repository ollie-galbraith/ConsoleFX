$TargetFramework = if ($PSEdition -eq 'Desktop') { 'net48' } else { 'net6.0' }
$AssemblyPath = Join-Path $PSScriptRoot "lib/$TargetFramework/ConsoleFX.dll"
$PublicPath = "$PSScriptRoot\Public"

$DotSourcedPaths = @(
    "$PSScriptRoot\Private",
    $PublicPath
)

if (-not (Test-Path -Path $AssemblyPath))
{
    throw "ConsoleFX.dll for target '$TargetFramework' not found at path: $AssemblyPath. Build the project before importing the module."
}
Add-Type -Path $AssemblyPath

$PublicMembers = Get-ChildItem -Path $PublicPath | ForEach-Object {$_.Name -replace ".ps1"}
$DotSourceMembers = Get-ChildItem -Path $DotSourcedPaths -Include "*.ps1" -Recurse -ErrorAction SilentlyContinue

ForEach($File in $DotSourceMembers)
    {
        . $File.FullName
    }

Export-ModuleMember -Function $PublicMembers
