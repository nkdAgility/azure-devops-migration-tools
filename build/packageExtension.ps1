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
Write-Output "Azure DevOps Migration Tools (Extension) Packaging"
Write-Output "----------------------------------------"
Write-Output "Version: $version"
Write-Output "Output Folder: $outfolder"

# Copy out Extension Files
New-Item -Path $outfolder -Name "\MigrationTools.Extension\" -ItemType Directory
Copy-Item  -Path ".\src\MigrationTools.Extension\**" -Destination "$outfolder\MigrationTools.Extension\" -Recurse


$fileToUpdate = Get-Item -Path "$outfolder\MigrationTools.Extension\vss-extension.json"
Write-Output "Processing $($fileToUpdate.Name)"
$contents = Get-Content -Path $fileToUpdate.FullName
$contents = $contents | ForEach-Object { $_ -replace "#{GITVERSION.SEMVER}#", $version }
Set-Content -Path $fileToUpdate.FullName -Value $contents 
Write-Output "Replaced tokens in $($fileToUpdate.Name)"

#-------------------------------------------
if (((npm list -g tfx-cli) -join "," ).Contains("empty")) {
    Write-Output "Installing tfx-cli"
    npm i -g tfx-cli
} else { Write-Output "Detected tfx-cli"}
#-------------------------------------------
# Build TFS Extension
Write-Output "Build TFS Extension"
tfx extension create --root "$outfolder\MigrationTools.Extension\" --output-path $outfolder --manifest-globs vss-extension.json
#-------------------------------------------
# Cleanup
# Remove-Item -Path "$outfolder\MigrationTools.Extension" -Recurse -Force