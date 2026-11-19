#Requires -Version 7.0
# prlinks (#3286): a source work item linked to a real pull request, migrated to another organisation.
#   source  amte2e-3286-src  repo amte2e-3286-src : main (C1), feature (C2), pull request feature -> main
#   target  amte2e-3286-tgt  no repository: the target organisation cannot resolve the source pull request.
param([string]$WorkDir = (Join-Path ([IO.Path]::GetTempPath()) "amte2e-3286"))
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
$SrcProject = $cfg.MigrationTools.Endpoints.Source.Project
$TgtProject = $cfg.MigrationTools.Endpoints.Target.Project
$srcOrg = "nkdagility-adomt-source"; $tgtOrg = "nkdagility-adomt-target"

Set-E2EOrg $tgtOrg
New-E2EProject -Name $TgtProject -Process "amte2e-agile" | Out-Null
Set-E2EOrg $srcOrg
$sp = New-E2EProject -Name $SrcProject -Process "amte2e-agile"
$repo = (Invoke-E2ERest "$SrcProject/_apis/git/repositories" -Org $srcOrg).value | Where-Object name -eq $SrcProject

if (-not $env:AMT_TEST_PAT) { $tok = az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv 2>$null; $hdr = "Authorization: Bearer $tok" }
else { $hdr = "Authorization: Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($env:AMT_TEST_PAT)")) }
$env:GIT_CONFIG_COUNT = "1"; $env:GIT_CONFIG_KEY_0 = "http.extraHeader"; $env:GIT_CONFIG_VALUE_0 = $hdr
$env:GIT_AUTHOR_NAME = "amte2e"; $env:GIT_AUTHOR_EMAIL = "amte2e@example.invalid"; $env:GIT_COMMITTER_NAME = "amte2e"; $env:GIT_COMMITTER_EMAIL = "amte2e@example.invalid"
try {
    function Push-E2ERepo([string]$Url, [string]$Ref) { $o = git push -q $Url $Ref 2>&1; if ($LASTEXITCODE -ne 0) { throw "git push $Ref failed ($LASTEXITCODE): $o" } }
    if (Test-Path $WorkDir) { Remove-Item $WorkDir -Recurse -Force }
    New-Item -ItemType Directory $WorkDir | Out-Null
    Push-Location $WorkDir
    git init -q -b main repo; Set-Location repo
    Set-Content a.txt "one"; git add a.txt; git commit -q -m "C1"
    Push-E2ERepo $repo.remoteUrl main
    git checkout -q -b feature; Set-Content a.txt "two"; git commit -q -am "C2"
    Push-E2ERepo $repo.remoteUrl feature
}
finally {
    Pop-Location
    foreach ($v in "GIT_CONFIG_COUNT", "GIT_CONFIG_KEY_0", "GIT_CONFIG_VALUE_0", "GIT_AUTHOR_NAME", "GIT_AUTHOR_EMAIL", "GIT_COMMITTER_NAME", "GIT_COMMITTER_EMAIL") { Remove-Item "env:$v" -ErrorAction SilentlyContinue }
    $tok = $null; $hdr = $null
}

$prTitle = "amte2e 3286 feature change"
$pr = (Invoke-E2ERest "$SrcProject/_apis/git/repositories/$($repo.id)/pullrequests?searchCriteria.status=all" -Org $srcOrg).value | Where-Object title -eq $prTitle | Select-Object -First 1
if (-not $pr) {
    $pr = Invoke-E2ERest "$SrcProject/_apis/git/repositories/$($repo.id)/pullrequests" -Org $srcOrg -Method POST -Body @{
        sourceRefName = "refs/heads/feature"; targetRefName = "refs/heads/main"; title = $prTitle; description = "seeded for #3286"
    }
}
$prUri = "vstfs:///Git/PullRequestId/$($sp.id)%2F$($repo.id)%2F$($pr.pullRequestId)"
$item = Set-E2EWorkItem $SrcProject "User Story" -Fields ([ordered]@{ "System.Title" = "pull request link item" })
Set-E2EWorkItem -Id $item.id -Ops @(@{ op = "add"; path = "/relations/-"; value = @{ rel = "ArtifactLink"; url = $prUri; attributes = @{ name = "Pull Request" } } }) | Out-Null
Save-E2EState $PSScriptRoot @{ item = $item.id; projectId = $sp.id; repoId = $repo.id; prId = $pr.pullRequestId; prTitle = $prTitle; prUri = $prUri
    prWebUrl = "https://dev.azure.com/$srcOrg/$SrcProject/_git/$SrcProject/pullrequest/$($pr.pullRequestId)" }
"seeded prlinks: item $($item.id) linked to pull request $($pr.pullRequestId) ($prUri)"
