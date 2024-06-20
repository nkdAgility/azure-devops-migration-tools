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
$MigrationToolsFilename = "MigrationTools-$version.zip"
Write-Output "MigrationTools Filename: $MigrationToolsFilename"

# create hash
Write-Output "Creating Hash for $MigrationToolsFilename"
$ZipHash = Get-FileHash $outfolder\$MigrationToolsFilename -Algorithm SHA256
$obj = @{
    "Hash" = $($ZipHash.Hash)
    "FullHash" = $ZipHash
    }
$hashSaveFile = "$outfolder\MigrationTools-$version.txt"
$obj | ConvertTo-Json |  Set-Content -Path $hashSaveFile
Write-Output "Hash saved to $hashSaveFile"

$fileToUpdate = Get-Item -Path "src\MigrationTools.Extension\vss-extension.json"
Write-Output "Processing $($fileToUpdate.Name)"
$contents = Get-Content -Path $fileToUpdate.FullName
$contents = $contents | ForEach-Object { $_ -replace "#{GITVERSION.SEMVER}#", $version }
$contents = $contents | ForEach-Object { $_ -replace "#{Chocolatey.FileHash}#", $obj.Hash }
Set-Content -Path $fileToUpdate.FullName -Value $contents 
Write-Output "Replaced tokens in $($fileToUpdate.Name)"

#-------------------------------------------
# Build TFS Extension
Write-Output "Build TFS Extension"
tfx extension create --root src\MigrationTools.Extension --output-path $outfolder --manifest-globs vss-extension.json
#-------------------------------------------