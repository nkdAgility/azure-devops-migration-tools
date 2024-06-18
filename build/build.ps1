
Write-Output "BUILD Azure DevOps Migration Tools"
Write-Output "======================"
Write-Output "Running from $($MyInvocation.MyCommand.Path)"
 #==============================================================================

$SkipPreRequisits = $false
if ($args -contains "-SkipPreRequisits") {
    $SkipPreRequisits = $true
}

if ($SkipPreRequisits) {
    Write-Output "Skipping PreRequisits"
} else {
    Write-Output "Installing PreRequisits"
    .\build\install-prerequsits.ps1
}
 #==============================================================================

 # Create output folder
if (Get-Item -Path ".\output" -ErrorAction SilentlyContinue) {
    Write-Output "Cleanning up output folder"
    Remove-Item -Path ".\output\" -Recurse -Force
}
New-Item -Name "output" -ItemType Directory

#==============================================================================

Write-Output "Detect Version"
Write-Output "--------------"

$IsLocal = $False
if ($Build.BuildId -eq $Null -or $github.run_id -eq $Null)
{
    $IsLocal = $True
}
Write-Output "IsLocal: $IsLocal"

# Get Version Numbers
$versionInfoJson = dotnet-gitversion
If ($IsLocal) {
    $versionInfoJson = $versionInfoJson | foreach {$_.replace("Preview","Local")}
    $versionInfoJson = $versionInfoJson | foreach {$_.replace("preview","Local")}
    $versionInfoJson
}
$versionInfo = $versionInfoJson | ConvertFrom-Json

Write-Output "FullSemVer: $($versionInfo.FullSemVer)"
Write-Output "SemVer: $($versionInfo.SemVer)"
Write-Output "PreReleaseTag: $($versionInfo.PreReleaseTag)"
Write-Output "InformationalVersion: $($versionInfo.InformationalVersion)"
Write-Output "--------------"

 #==============================================================================

 Write-Output "Complile and Test"
 Write-Output "--------------"

$SkipCompile = $false
if ($args -contains "-SkipCompile") {
    $SkipCompile = $true
}
Write-Output "-SkipCompile $SkipCompile"

if ($SkipCompile) {
    Write-Output "Skipping Compile & Test"
} else {
    Write-Output "Running Compile"
    .\build\compile-and-test.ps1
}
Write-Output "--------------"

#==============================================================================

Write-Output "Azure DevOps Migration Tools (PACKAGING)"
Write-Output "----------------------------------------"
# Create output sub folders
New-Item -Name "output/MigrationTools/" -ItemType Directory
New-Item -Name "output/MigrationTools/preview/" -ItemType Directory
New-Item -Name "output/MigrationTools/ConfigSamples/" -ItemType Directory
# Copy Files
Copy-Item  -Path ".\src\MigrationTools.ConsoleFull\bin\Release\net472\*" -Destination "./output/MigrationTools/" -Recurse
Copy-Item  -Path ".\src\MigrationTools.ConsoleCore\bin\Release\net7.0\*" -Destination "./output/MigrationTools/preview/" -Recurse
Copy-Item  -Path ".\src\MigrationTools.Samples\*" -Destination "./output/MigrationTools/ConfigSamples/" -Recurse

#==============================================================================

$versionText = "v$($versionInfo.SemVer)";
Write-Output "Version: $versionText"
$ZipName = "MigrationTools-$($versionInfo.SemVer).zip"
Write-Output "ZipName: $ZipName"
$ZipFilePath = ".\output\$ZipName"
Write-Output "ZipFilePath: $ZipFilePath"
#-------------------------------------------
# Create Zip
7z a -tzip  $ZipFilePath .\output\MigrationTools\**
#-------------------------------------------
# create hash
Write-Output "Creating Hash for $ZipFilePath"
$ZipHash = Get-FileHash $ZipFilePath -Algorithm SHA256
$obj = @{
    "Hash" = $($ZipHash.Hash)
    "FullHash" = $ZipHash
    }
$hashSaveFile = ".\output\MigrationTools-$($versionInfo.SemVer).txt"
$obj | ConvertTo-Json |  Set-Content -Path ".\output\MigrationTools-$($versionInfo.SemVer).txt"
Write-Output "Hash saved to $hashSaveFile"
#-------------------------------------------
# Replace tokens
Write-Output "Find and replace tokens"
$tokens = @("**\chocolatey*.ps1", "**\vss-extension.json")
$files = Get-ChildItem -Path $tokens -Recurse -Exclude "output"
Write-Output "Found $($files.Count) files that might have tokens"
$hash = @{ "#{GITVERSION.SEMVER}#" = $versionInfo.SemVer; "#{Chocolatey.FileHash}#" = $obj.Hash;}
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
# Build TFS Extension
tfx extension create --root src\MigrationTools.Extension --output-path output/ --manifest-globs vss-extension.json
#-------------------------------------------

# Build Chocolatey Package
choco pack src/MigrationTools.Chocolatey/nkdAgility.AzureDevOpsMigrationTools.nuspec --version $versionInfo.NuGetVersion configuration=release --outputdirectory output

 #==============================================================================
 # Cleanup
 Remove-Item -Path ".\output\MigrationTools" -Recurse -Force
 #==============================================================================
# Publish
Write-Output "PUBLISH ABBWorkItemClone"
Write-Output "--------------"
$files = Get-ChildItem -Path ".\output\*" -Recurse
if ($versionInfo.PreReleaseTag -eq "") {
    Write-Output "Publishing Release"
   gh release create $versionText $files --generate-notes --generate-notes --discussion-category "General"
} else {
    Write-Output "Publishing PreRelease"
   gh release create $versionText $files --generate-notes --generate-notes --prerelease --discussion-category "General"
}
