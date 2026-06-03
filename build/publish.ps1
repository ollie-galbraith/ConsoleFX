# Copy only the files you want to a temp folder
$TempPath = "$PSScriptRoot\..\temp\ConsoleFX"

New-Item -Path $TempPath -ItemType Directory -Force | Out-Null

Copy-Item -Path "$PSScriptRoot\..\ConsoleFX.psd1" -Destination $TempPath
Copy-Item -Path "$PSScriptRoot\..\ConsoleFX.psm1" -Destination $TempPath
Copy-Item -Path "$PSScriptRoot\..\lib" -Destination $TempPath -Recurse
Copy-Item -Path "$PSScriptRoot\..\Public" -Destination $TempPath -Recurse
Copy-Item -Path "$PSScriptRoot\..\Private" -Destination $TempPath -Recurse

# Then publish from the temp folder
Publish-Module -Path $TempPath -NuGetApiKey $env:NugetApiKey -Repository PSGallery -Verbose

Remove-Item -Path $TempPath -Recurse
