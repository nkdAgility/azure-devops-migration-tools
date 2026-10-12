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
    [string]$GH_TOKEN,

    # Generate and validate the manifest without syncing the fork or submitting a PR
    [switch]$DryRun

)
#Publish
Write-Output "Azure DevOps Migration Tools (Winget) Release"
Write-Output "--------------"

$wigetPackageId = "nkdAgility.AzureDevOpsMigrationTools"
switch ($ring) {
    "Preview" { $wigetPackageId = "$wigetPackageId.Preview" }
    "Production" { }
    default {
        Write-Host "We dont ship canary builds!"
        return
    }
}

Write-Output "Download wingetcreate.exe"
Invoke-WebRequest https://aka.ms/wingetcreate/latest -OutFile wingetcreate.exe

# wingetcreate submits from the token owner's winget-pkgs fork and fails if it cannot sync it with upstream.
# Syncing needs the 'workflow' scope whenever upstream has changed .github/workflows, so sync here to get a clear error.
$env:GH_TOKEN = $GH_TOKEN
$ghUser = gh api user --jq .login
if ($LASTEXITCODE -ne 0) { throw "GH_TOKEN could not be used to read the GitHub user" }
$fork = "$ghUser/winget-pkgs"
$forkState = gh api "repos/microsoft/winget-pkgs/compare/master...$($ghUser):master"
if ($LASTEXITCODE -ne 0) { throw "Could not compare $fork with microsoft/winget-pkgs; does the fork exist?" }
$forkState = $forkState | ConvertFrom-Json
$ahead = $forkState.ahead_by
$behind = $forkState.behind_by
Write-Output "$fork is $behind commits behind and $ahead ahead of microsoft/winget-pkgs"
if ([int]$ahead -gt 0) { throw "$fork master has $ahead commits not in microsoft/winget-pkgs; reset it manually" }
if ([int]$behind -gt 0 -and -not $DryRun) {
    Write-Output "Syncing $fork with microsoft/winget-pkgs"
    gh repo sync $fork --source microsoft/winget-pkgs
    if ($LASTEXITCODE -ne 0) { throw "Could not sync $fork. The token needs the 'workflow' scope (classic PAT) or Workflows: write (fine-grained) to sync upstream workflow changes." }
}

#https://github.com/microsoft/winget-create/blob/main/doc/update.md

$installURL = "https://github.com/nkdAgility/azure-devops-migration-tools/releases/download/v$version/MigrationTools-$version.zip|x64"

Write-Host $"##[warning] $installURL"

Write-Host "Winget Create with $wigetPackageId"
if ($DryRun) {
    ./wingetcreate.exe update --urls $installURL --version $version --out (Join-Path $PWD "winget-manifests") $wigetPackageId
} else {
    ./wingetcreate.exe update --submit --token $GH_TOKEN --urls $installURL --version $version $wigetPackageId
}
if ($LASTEXITCODE -ne 0) { throw "wingetcreate failed for $wigetPackageId" }

Write-Host "Deployed : $wigetPackageId"

Write-Output "--------------"
