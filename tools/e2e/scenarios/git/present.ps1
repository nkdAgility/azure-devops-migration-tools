#Requires -Version 7.0
# git, after a migration: push C2 to the target repository, so the commit link that was kept as the source link because
# C2 was missing in the target (VerifyTargetExists) can now be rewritten. Used by the #3299 missing-to-present check.
param([string]$WorkDir = (Join-Path ([IO.Path]::GetTempPath()) "amte2e-git"))
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
$tgtProject = $cfg.MigrationTools.Endpoints.Target.Project
$repo = (Invoke-E2ERest "$tgtProject/_apis/git/repositories" -Org "nkdagility-adomt-target").value | Where-Object name -eq $tgtProject
if (-not $repo) { throw "target repository $tgtProject not found" }
if (-not (Test-Path (Join-Path $WorkDir "main"))) { throw "work directory $WorkDir from seed.ps1 not found" }
if (-not $env:AMT_TEST_PAT) { $tok = az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv 2>$null; $hdr = "Authorization: Bearer $tok" }
else { $hdr = "Authorization: Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($env:AMT_TEST_PAT)")) }
$env:GIT_CONFIG_COUNT = "1"; $env:GIT_CONFIG_KEY_0 = "http.extraHeader"; $env:GIT_CONFIG_VALUE_0 = $hdr
Push-Location (Join-Path $WorkDir "main")
try {
    $c2 = (git rev-parse HEAD).Trim()
    $o = git push -q $repo.remoteUrl main 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git push to the target repository failed ($LASTEXITCODE): $o" }
}
finally { Pop-Location; Remove-Item Env:GIT_CONFIG_VALUE_0 -ErrorAction SilentlyContinue }
"pushed C2 $c2 to $($repo.name) in $tgtProject"
