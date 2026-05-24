# Copy only the files you want to a temp folder
$TempPath = "C:\temp\ConsoleFX"

New-Item -Path $TempPath -ItemType Directory -Force | Out-Null

Copy-Item -Path "C:\Users\Oliver Galbraith\git\ConsoleFX\ConsoleFX.psd1" -Destination $TempPath
Copy-Item -Path "C:\Users\Oliver Galbraith\git\ConsoleFX\ConsoleFX.psm1" -Destination $TempPath
Copy-Item -Path "C:\Users\Oliver Galbraith\git\ConsoleFX\lib" -Destination $TempPath -Recurse
Copy-Item -Path "C:\Users\Oliver Galbraith\git\ConsoleFX\Public" -Destination $TempPath -Recurse
Copy-Item -Path "C:\Users\Oliver Galbraith\git\ConsoleFX\Private" -Destination $TempPath -Recurse

# Then publish from the temp folder
Publish-Module -Path $TempPath -NuGetApiKey $env:NugetApiKey -Repository PSGallery -Verbose

Remove-Item -Path $TempPath -Recurse
