<#
    Azure DevOps Migration Tools (GitHub Release) Release
#>
param (
    # height of largest column without top bar
    [Parameter(Mandatory=$true)]
    [string]$version,

    # name of the releaseTag
    [Parameter(Mandatory=$true)]
    [string]$releaseTag,

    # GH_TOKEN
    [Parameter(Mandatory=$true)]
    [string]$GH_TOKEN

)
#Publish
Write-Output "Azure DevOps Migration Tools (Winget) Release"
Write-Output "--------------"

Write-Output "Download wingetcreate.exe"
Invoke-WebRequest https://aka.ms/wingetcreate/latest -OutFile wingetcreate.exe

# echo "Structure of work folder of this pipeline:"
# tree /f
# Get-ChildItem -Recurse
#https://github.com/microsoft/winget-create/blob/main/doc/update.md

$installURL = "https://github.com/nkdAgility/azure-devops-migration-tools/releases/download/v$version/MigrationTools-$version.zip|x64"

Write-Host $"##[warning] $installURL"

$wigetPackageId = "nkdAgility.AzureDevOpsMigrationTools"

if ($releaseTag -eq "Preview")
{
  $wigetPackageId = "$wigetPackageId.Preview"
}
Write-Host "Winget Create with $wigetPackageId"

./wingetcreate.exe update --submit --token $GH_TOKEN --urls $installURL --version $version $wigetPackageId

Write-Host "Deployed : $wigetPackageId"

Write-Output "--------------"