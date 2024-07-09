<#
    Script description.

    Some notes.
#>
param (
    # height of largest column without top bar
    [Parameter(Mandatory=$true)]
    [string]$version,
    
    # name of the output folder
    [Parameter(Mandatory=$true)]
    [string]$outfolder
)
Write-Output "Azure DevOps Migration Tools (Executable) Packaging"
Write-Output "----------------------------------------"
Write-Output "Version: $version"
Write-Output "Output Folder: $outfolder"
$OutputFilename = "MigrationTools-$version.zip"
Write-Output "Filename: $OutputFilename"
$OutputFullName = "$outfolder\$OutputFilename"
Write-Output "Full Name: $OutputFullName"
#==============================================================================
# Install Prerequisits
Write-Output "Install Prerequisits for $env:RUNNER_OS"

sudo apt install p7zip-full p7zip-rar
$installedStuff = choco list -i 
if (($installedStuff -like "*7zip*").Count -eq 0) {
   Write-Output "Installing 7zip"
   choco install 7zip --confirm --accept-license -y
} else { Write-Output "Detected 7zip"}
#==============================================================================
# Create output sub folders
Write-Output "Create folders in $outfolder"
New-Item -Path $outfolder -Name "\MigrationTools\" -ItemType Directory
New-Item -Path $outfolder -Name "\MigrationTools\preview\" -ItemType Directory
New-Item  -Path $outfolder -Name "\MigrationTools\ConfigSamples\" -ItemType Directory
#==============================================================================
# Copy Files
Write-Output "Copy files to $outfolder\MigrationTools\"
Copy-Item  -Path ".\src\MigrationTools.ConsoleFull\bin\Debug\net472\*" -Destination "$outfolder\MigrationTools\" -Recurse
Copy-Item  -Path ".\src\MigrationTools.ConsoleCore\bin\Debug\net8.0\*" -Destination "$outfolder\MigrationTools\preview\" -Recurse
Copy-Item  -Path ".\src\MigrationTools.Samples\*" -Destination "$outfolder\MigrationTools\ConfigSamples\" -Recurse 
#==============================================================================
# Create Zip
7z a -tzip  $OutputFullName $outfolder\MigrationTools\**
#==============================================================================
# Cleanup
Remove-Item -Path "$outfolder\MigrationTools" -Recurse -Force