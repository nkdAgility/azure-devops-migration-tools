cls
$StartTimeBuild = Get-Date;
.\build\logging.ps1

Write-InfoLog "BUILD Azure DevOps Migration Tools"
Write-InfoLog "======================"
Write-InfoLog "Running from $($MyInvocation.MyCommand.Path)"
 #==============================================================================

$SkipPreRequisits = $false
if ($args -contains "-SkipPreRequisits") {
    $SkipPreRequisits = $true
}

if ($SkipPreRequisits) {
    Write-InfoLog "Skipping PreRequisits"
} else {
    Write-InfoLog "Installing PreRequisits"
    .\build\install-prerequsits.ps1
}
 #==============================================================================

 # Create output folder
if (Get-Item -Path ".\output" -ErrorAction SilentlyContinue) {
    Write-InfoLog "Cleanning up output folder"
    Remove-Item -Path ".\output\" -Recurse -Force
}
New-Item -Name "output" -ItemType Directory

#==============================================================================
.\build\versioning.ps1
 #==============================================================================
 Write-InfoLog "Complile and Test"
 Write-InfoLog "--------------"
$SkipCompile = $false
if ($args -contains "-SkipCompile") {
    $SkipCompile = $true
}
Write-InfoLog "-SkipCompile $SkipCompile"
if ($SkipCompile) {
    Write-InfoLog "Skipping Compile & Test"
} else {
    Write-InfoLog "Running Compile"
    .\build\compile-and-test.ps1
}
Write-InfoLog "--------------"
#==============================================================================

Write-InfoLog "Azure DevOps Migration Tools (PACKAGING)"
Write-InfoLog "----------------------------------------"
# Create output sub folders
New-Item -Name "output/MigrationTools/" -ItemType Directory
New-Item -Name "output/MigrationTools/preview/" -ItemType Directory
New-Item -Name "output/MigrationTools/ConfigSamples/" -ItemType Directory
# Copy Files
Copy-Item  -Path ".\src\MigrationTools.ConsoleFull\bin\Debug\net472\*" -Destination "./output/MigrationTools/" -Recurse
Copy-Item  -Path ".\src\MigrationTools.ConsoleCore\bin\Debug\net7.0\*" -Destination "./output/MigrationTools/preview/" -Recurse
Copy-Item  -Path ".\src\MigrationTools.Samples\*" -Destination "./output/MigrationTools/ConfigSamples/" -Recurse

#==============================================================================

$versionText = "v$($versionInfo.SemVer)";
Write-InfoLog "Version: $versionText"
$ZipName = "MigrationTools-$($versionInfo.SemVer).zip"
Write-InfoLog "ZipName: $ZipName"
$ZipFilePath = ".\output\$ZipName"
Write-InfoLog "ZipFilePath: $ZipFilePath"
#-------------------------------------------
# Create Zip
7z a -tzip  $ZipFilePath .\output\MigrationTools\**
#-------------------------------------------
# create hash
Write-InfoLog "Creating Hash for $ZipFilePath"
$ZipHash = Get-FileHash $ZipFilePath -Algorithm SHA256
$obj = @{
    "Hash" = $($ZipHash.Hash)
    "FullHash" = $ZipHash
    }
$hashSaveFile = ".\output\MigrationTools-$($versionInfo.SemVer).txt"
$obj | ConvertTo-Json |  Set-Content -Path ".\output\MigrationTools-$($versionInfo.SemVer).txt"
Write-InfoLog "Hash saved to $hashSaveFile"
#-------------------------------------------
# Replace tokens
Write-InfoLog "Find and replace tokens"
$tokens = @("**\chocolatey*.ps1", "**\vss-extension.json")
$files = Get-ChildItem -Path $tokens -Recurse -Exclude "output"
Write-InfoLog "Found $($files.Count) files that might have tokens"
$hash = @{ "#{GITVERSION.SEMVER}#" = $versionInfo.SemVer; "#{Chocolatey.FileHash}#" = $obj.Hash;}
foreach ($file in $files) {
    Write-InfoLog "Processing $($file.Name)"
    $contents = Get-Content $file.FullName
    if ($contents -eq $null) {
        Write-InfoLog "$($file.Name) is empty"
       continue;
    }
    $updated = $false;
    foreach ($key in $hash.Keys) {
        if ($contents | %{$_ -match $key}) {
            Write-InfoLog "Found token $key in $($file.Name)"
            $contents = $contents | ForEach-Object { $_ -replace $key, $hash[$key] }
            $updated = $true
        }        
    }
    if ($updated) {
        Set-Content $file.FullName $contents 
        Write-InfoLog "Replaced tokens in $($file.Name)"
    }
}
Write-InfoLog "--------------"
#-------------------------------------------
# Build TFS Extension
tfx extension create --root src\MigrationTools.Extension --output-path output/ --manifest-globs vss-extension.json
#-------------------------------------------

# Build Chocolatey Package
choco pack src/MigrationTools.Chocolatey/nkdAgility.AzureDevOpsMigrationTools.nuspec --version $versionInfo.NuGetVersion configuration=release --outputdirectory output
#-------------------------------------------
# Copy MigrationTools.nupkg
Copy-Item  -Path ".\src\MigrationTools\bin\**\*.nupkg" -Destination "./output/" -Recurse
#-------------------------------------------
# Copy MigrationTools.nupkg
New-Item -Name "output/WinGet/" -ItemType Directory
Copy-Item  -Path ".\src\MigrationTools.WinGet\**" -Destination "./output/WinGet" -Recurse
#-------------------------------------------
 #==============================================================================
 # Cleanup
 Remove-Item -Path ".\output\MigrationTools" -Recurse -Force
 #==============================================================================
# Publish
#Write-InfoLog "PUBLISH ABBWorkItemClone"
#Write-InfoLog "--------------"
#$files = Get-ChildItem -Path ".\output\*" -Recurse
#if ($versionInfo.PreReleaseTag -eq "") {
#    Write-InfoLog "Publishing Release"
#   #gh release create $versionText $files --generate-notes --generate-notes --discussion-category "General"
#} else {
#    Write-InfoLog "Publishing PreRelease"
#   #gh release create $versionText $files --generate-notes --generate-notes --prerelease --discussion-category "General"
#}

 #==============================================================================
 # Final
 Write-InfoLog "Build ran in $((Get-Date) - $StartTimeBuild)"
 #==============================================================================
 Close-Logger