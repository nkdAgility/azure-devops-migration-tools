<#
    Azure DevOps Migration Tools (GitHub Release) Release
#>
param (
    # height of largest column without top bar
    [Parameter(Mandatory = $true)]
    [string]$version,

    # name of the releaseTag
    [Parameter(Mandatory = $true)]
    [string]$ring,

    # GH_TOKEN
    [Parameter(Mandatory = $true)]
    [string]$GH_TOKEN

)
#Publish
Write-Output "Azure DevOps Migration Tools (Winget) Release"
Write-Output "--------------"

Write-Output "Download wingetcreate.exe"
Invoke-WebRequest https://aka.ms/wingetcreate/latest -OutFile wingetcreate.exe

# wingetcreate submits from the token owner's winget-pkgs fork and fails if it cannot sync it, so sync it first
$env:GH_TOKEN = $GH_TOKEN
$ghUser = gh api user --jq .login
Write-Output "Syncing $ghUser/winget-pkgs with microsoft/winget-pkgs"
gh repo sync "$ghUser/winget-pkgs" --source microsoft/winget-pkgs --force
if ($LASTEXITCODE -ne 0) { Write-Host "##[warning] Failed to sync $ghUser/winget-pkgs; wingetcreate may fail" }

# echo "Structure of work folder of this pipeline:"
# tree /f
# Get-ChildItem -Recurse
#https://github.com/microsoft/winget-create/blob/main/doc/update.md

$installURL = "https://github.com/nkdAgility/azure-devops-migration-tools/releases/download/v$version/MigrationTools-$version.zip|x64"

Write-Host $"##[warning] $installURL"

$wigetPackageId = "nkdAgility.AzureDevOpsMigrationTools"

Write-Host "Winget Create with $wigetPackageId"
switch ($ring) {
    "Preview" {
        $wigetPackageId = "$wigetPackageId.Preview"
        ./wingetcreate.exe update --submit --token $GH_TOKEN --urls $installURL --version $version $wigetPackageId
        if ($LASTEXITCODE -ne 0) { throw "wingetcreate failed for $wigetPackageId" }
    }
    "Production" {
        $wigetPackageId = "nkdAgility.AzureDevOpsMigrationTools"
        ./wingetcreate.exe update --submit --token $GH_TOKEN --urls $installURL --version $version $wigetPackageId
        if ($LASTEXITCODE -ne 0) { throw "wingetcreate failed for $wigetPackageId" }
    }
    default {
        $wigetPackageId = "nkdAgility.AzureDevOpsMigrationTools.Canary"
        Write-Host "We dont ship canary builds!"
    }
}

Write-Host "Deployed : $wigetPackageId"

Write-Output "--------------"