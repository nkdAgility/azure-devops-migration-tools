<#
    Azure DevOps Migration Tools (Winget) Release

    Submission policy, so that microsoft/winget-pkgs holds at most one open PR per package:
    - Canary is never submitted.
    - Production is always submitted.
    - Preview is submitted only when no Preview PR of ours was opened in the last
      $PreviewMinDaysBetweenSubmissions days (open or merged; closed-unmerged ones do not count).
    - After a successful submission, our own older open PRs for the same package are closed as superseded.
    wingetcreate has no way to update or replace an open PR (--replace only replaces an existing
    manifest version in the repo), so superseded PRs are closed here with gh.
#>
param (
    [Parameter(Mandatory = $true)]
    [string]$version,

    # Production, Preview or Canary
    [Parameter(Mandatory = $true)]
    [string]$ring,

    [Parameter(Mandatory = $true)]
    [string]$GH_TOKEN,

    # Minimum days between Preview submissions
    [int]$PreviewMinDaysBetweenSubmissions = 7,

    # Submit a Preview even inside the throttle window
    [switch]$Force,

    # Decide and generate the manifest, but do not sync the fork, close PRs or submit
    [switch]$DryRun,

    # Testing only: decide what would happen and stop before downloading wingetcreate
    [switch]$PlanOnly
)

function Get-WingetSubmissionPlan {
    param (
        [string]$Ring,
        [string]$PackageId,
        [object[]]$OurPullRequests,
        [int]$PreviewMinDaysBetweenSubmissions,
        [bool]$Force,
        [datetime]$Now = (Get-Date).ToUniversalTime()
    )
    $titlePrefix = "New version: $PackageId version "
    $mine = @($OurPullRequests | Where-Object { $_.title.StartsWith($titlePrefix, [StringComparison]::OrdinalIgnoreCase) })
    $open = @($mine | Where-Object { $_.state -eq 'OPEN' })
    $plan = [ordered]@{ Submit = $true; Reason = ""; Close = $open }
    if ($Ring -eq 'Preview' -and -not $Force) {
        $since = $Now.AddDays(-$PreviewMinDaysBetweenSubmissions)
        $recent = @($mine | Where-Object { $_.state -ne 'CLOSED' -and ([datetime]$_.createdAt).ToUniversalTime() -gt $since })
        if ($recent.Count -gt 0) {
            $plan.Submit = $false
            $plan.Close = @()
            $plan.Reason = "Preview PR #$($recent[0].number) was opened within the last $PreviewMinDaysBetweenSubmissions days"
            return $plan
        }
    }
    $plan.Reason = "No recent submission blocks this one"
    return $plan
}

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

$env:GH_TOKEN = $GH_TOKEN
$ghUser = gh api user --jq .login
if ($LASTEXITCODE -ne 0) { throw "GH_TOKEN could not be used to read the GitHub user" }

$ourPrs = gh pr list -R microsoft/winget-pkgs --author $ghUser --state all --limit 50 --search "$wigetPackageId in:title" --json number,title,state,createdAt
if ($LASTEXITCODE -ne 0) { throw "Could not list $ghUser pull requests in microsoft/winget-pkgs" }
$plan = Get-WingetSubmissionPlan -Ring $ring -PackageId $wigetPackageId -OurPullRequests ($ourPrs | ConvertFrom-Json) -PreviewMinDaysBetweenSubmissions $PreviewMinDaysBetweenSubmissions -Force $Force.IsPresent
Write-Output "Plan for $wigetPackageId $version : Submit=$($plan.Submit) ($($plan.Reason)); close superseded: $(($plan.Close | ForEach-Object { "#$($_.number)" }) -join ', ')"
if (-not $plan.Submit -and -not $DryRun) {
    Write-Host "##[notice] Skipping WinGet submission of $wigetPackageId $version. $($plan.Reason)."
    return
}
if ($PlanOnly) { return }
if (-not $plan.Submit) { Write-Output "DryRun: generating the manifest even though a real run would skip" }

Write-Output "Download wingetcreate.exe"
Invoke-WebRequest https://aka.ms/wingetcreate/latest -OutFile wingetcreate.exe

# wingetcreate submits from the token owner's winget-pkgs fork and fails if it cannot sync it with upstream.
# Syncing needs the 'workflow' scope whenever upstream has changed .github/workflows, so sync here to get a clear error.
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
    ./wingetcreate.exe update --submit --no-open --token $GH_TOKEN --urls $installURL --version $version $wigetPackageId
}
if ($LASTEXITCODE -ne 0) { throw "wingetcreate failed for $wigetPackageId" }

# Close only after a successful submission, so a failed run never leaves zero open PRs.
foreach ($pr in $plan.Close) {
    if ($DryRun) { Write-Output "DryRun: would close #$($pr.number) $($pr.title)"; continue }
    gh pr close $pr.number -R microsoft/winget-pkgs --comment "Superseded by $wigetPackageId $version."
    if ($LASTEXITCODE -ne 0) { Write-Host "##[warning] Could not close superseded PR #$($pr.number)" }
}

Write-Host "Deployed : $wigetPackageId"

Write-Output "--------------"
