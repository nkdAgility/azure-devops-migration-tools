# git: #3185 (repo referenced from another project), #3204 (commit missing in target), #3198/#3070 (malformed and new-format links).
# The same local history is pushed to both organisations so commit ids match.
#   source  amte2e-git-src    repo amte2e-git-src : C1, C2       amte2e-git-other repo amte2e-shared : S1
#   target  amte2e-git-tgt    repo amte2e-git-tgt : C1 only      repo amte2e-shared (same name)       : S1
# Work item links: C1 (rewritten), C2 (missing in target, must stay on source), S1 (repo of another project, resolved by
# name), a malformed legacy link and a new-format ('/'-separated) link.
param([string]$WorkDir = (Join-Path ([IO.Path]::GetTempPath()) "amte2e-git"))
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$src = "nkdagility-adomt-source"; $tgt = "nkdagility-adomt-target"

Set-E2EOrg $tgt
$tp = New-E2EProject -Name "amte2e-git-tgt" -Process "amte2e-agile"
Set-E2EOrg $src
$sp = New-E2EProject -Name "amte2e-git-src" -Process "amte2e-agile"
$op = New-E2EProject -Name "amte2e-git-other" -Process "amte2e-agile"

function Get-Repo([string]$Org, [string]$Project, [string]$Name) {
    $r = (Invoke-E2ERest "$Project/_apis/git/repositories" -Org $Org).value | Where-Object name -eq $Name
    if (-not $r) { $pid_ = (Invoke-E2ERest "_apis/projects/$Project" -Org $Org).id; $r = Invoke-E2ERest "$Project/_apis/git/repositories" -Org $Org -Method POST -Body @{ name = $Name; project = @{ id = $pid_ } } }
    $r
}
$srcRepo = Get-Repo $src "amte2e-git-src" "amte2e-git-src"
$otherRepo = Get-Repo $src "amte2e-git-other" "amte2e-shared"
$tgtRepo = Get-Repo $tgt "amte2e-git-tgt" "amte2e-git-tgt"
$tgtShared = Get-Repo $tgt "amte2e-git-tgt" "amte2e-shared"

# git auth through environment config, so the token is never on a command line.
if (-not $env:AMT_TEST_PAT) { $tok = az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv 2>$null; $hdr = "Authorization: Bearer $tok" }
else { $hdr = "Authorization: Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($env:AMT_TEST_PAT)")) }
$env:GIT_CONFIG_COUNT = "1"; $env:GIT_CONFIG_KEY_0 = "http.extraHeader"; $env:GIT_CONFIG_VALUE_0 = $hdr
$env:GIT_AUTHOR_NAME = "amte2e"; $env:GIT_AUTHOR_EMAIL = "amte2e@example.invalid"; $env:GIT_COMMITTER_NAME = "amte2e"; $env:GIT_COMMITTER_EMAIL = "amte2e@example.invalid"
$env:GIT_AUTHOR_DATE = "2026-10-01T10:00:00Z"; $env:GIT_COMMITTER_DATE = "2026-10-01T10:00:00Z"
try {
    # Native commands ignore $ErrorActionPreference, so every push checks its own exit code.
    function Push-E2ERepo([string]$Url) { $o = git push -q $Url main 2>&1; if ($LASTEXITCODE -ne 0) { throw "git push to $Url failed ($LASTEXITCODE): $o" } }
    if (Test-Path $WorkDir) { Remove-Item $WorkDir -Recurse -Force }
    New-Item -ItemType Directory $WorkDir | Out-Null
    Push-Location $WorkDir
    git init -q -b main main; Set-Location main
    Set-Content a.txt "one"; git add a.txt; git commit -q -m "C1"; $c1 = (git rev-parse HEAD).Trim()
    Push-E2ERepo $tgtRepo.remoteUrl
    Set-Content a.txt "two"; git commit -q -am "C2"; $c2 = (git rev-parse HEAD).Trim()
    Push-E2ERepo $srcRepo.remoteUrl
    Set-Location ..; git init -q -b main shared; Set-Location shared
    Set-Content s.txt "shared"; git add s.txt; git commit -q -m "S1"; $s1 = (git rev-parse HEAD).Trim()
    Push-E2ERepo $otherRepo.remoteUrl
    Push-E2ERepo $tgtShared.remoteUrl
}
finally {
    Pop-Location
    foreach ($v in "GIT_CONFIG_COUNT", "GIT_CONFIG_KEY_0", "GIT_CONFIG_VALUE_0", "GIT_AUTHOR_NAME", "GIT_AUTHOR_EMAIL", "GIT_COMMITTER_NAME", "GIT_COMMITTER_EMAIL", "GIT_AUTHOR_DATE", "GIT_COMMITTER_DATE") { Remove-Item "env:$v" -ErrorAction SilentlyContinue }
    $tok = $null; $hdr = $null
}

$commitLink = { param($projId, $repoId, $sha) "vstfs:///Git/Commit/$projId%2F$repoId%2F$sha" }
$links = [ordered]@{
    c1        = & $commitLink $sp.id $srcRepo.id $c1
    c2missing = & $commitLink $sp.id $srcRepo.id $c2
    s1other   = & $commitLink $op.id $otherRepo.id $s1
    malformed = "vstfs:///Git/Commit/$($sp.id)%2F%2F$c1"
    newformat = "vstfs:///Git/Commit/$($sp.id)/$($srcRepo.id)/$c1"   # ids, not names: not the #3070 format
    newbyname = "vstfs:///Git/Commit/amte2e-git-src/amte2e-git-src/$c1" # #3070 format: projectName/repoName/commitId
}
$item = Set-E2EWorkItem "amte2e-git-src" "User Story" -Fields ([ordered]@{ "System.Title" = "git links item" })
$added = [ordered]@{}
foreach ($k in $links.Keys) {
    try {
        Set-E2EWorkItem -Id $item.id -Ops @(@{ op = "add"; path = "/relations/-"; value = @{ rel = "ArtifactLink"; url = $links[$k]; attributes = @{ name = "Fixed in Commit"; comment = $k } } }) | Out-Null
        $added[$k] = $true
    } catch { $added[$k] = "rejected by service: $($_.ErrorDetails.Message)" }
}
Save-E2EState $PSScriptRoot @{ item = $item.id; c1 = $c1; c2 = $c2; s1 = $s1; links = $links; added = $added; srcRepo = $srcRepo.id; otherRepo = $otherRepo.id; tgtRepo = $tgtRepo.id; tgtShared = $tgtShared.id; tgtProject = $tp.id }
"seeded git: item $($item.id); links added: " + (($added.Keys | ForEach-Object { "$_=$($added[$_])" }) -join "; ")
