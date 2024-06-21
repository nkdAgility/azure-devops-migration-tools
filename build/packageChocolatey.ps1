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
Write-Output "Azure DevOps Migration Tools (Chocolatey) Packaging"
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

#-------------------------------------------
# Replace tokens
Write-Output "Find and replace tokens"
$tokens = @("**\chocolatey*.ps1")
$files = Get-ChildItem -Path $tokens -Recurse -Exclude "output"
Write-Output "Found $($files.Count) files that might have tokens"

$hash = @{ 
    "#{GITVERSION.SEMVER}#" = $version; 
    "#{Chocolatey.FileHash}#" = $obj.Hash;
}
$hash

foreach ($file in $files) {
    Write-Output "Processing $($file.Name)"
    $contents = Get-Content $file.FullName
    if ($contents -eq $null) {
        Write-Output "$($file.Name) is empty"
       continue;
    }
    $updated = $false;
    foreach ($key in $hash.Keys) {
        if ($contents | %{$_ -match $key}) {
            Write-Output "Found token $key in $($file.Name)"
            $contents = $contents | ForEach-Object { $_ -replace $key, $hash[$key] }
            $updated = $true
        }        
    }
    if ($updated) {
        Set-Content $file.FullName $contents 
        Write-Output "Replaced tokens in $($file.Name)"
    }
}
Write-Output "--------------"

#-------------------------------------------
# Build Chocolatey Package
Write-Output "Build Chocolatey Package with version [$version]"
choco pack src/MigrationTools.Chocolatey/nkdAgility.AzureDevOpsMigrationTools.nuspec --version $version configuration=release --outputdirectory $outfolder
#-------------------------------------------