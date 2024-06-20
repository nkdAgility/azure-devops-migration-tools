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
$hashSaveFile = "$outfolder\MigrationTools-$($versionInfo.SemVer).txt"
$obj | ConvertTo-Json |  Set-Content -Path "$outfolder\MigrationTools-$version-hash.txt"
Write-Output "Hash saved to $hashSaveFile"

$fileToUpdate = Get-Item -Path "src\MigrationTools.Extension\vss-extension.json"
Write-InfoLog "Processing $($fileToUpdate.Name)"
$contents = Get-Content $fileToUpdate.FullName
$contents = $contents | ForEach-Object { $_ -replace "#{GITVERSION.SEMVER}#", $version }
$contents = $contents | ForEach-Object { $_ -replace "#{Chocolatey.FileHash}#", $obj.Hash }
Set-Content $file.FullName $contents 
Write-InfoLog "Replaced tokens in $($file.Name)"

#-------------------------------------------
# Build TFS Extension
Write-InfoLog "Build TFS Extension"
tfx extension create --root src\MigrationTools.Extension --output-path output\ --manifest-globs vss-extension.json
#-------------------------------------------