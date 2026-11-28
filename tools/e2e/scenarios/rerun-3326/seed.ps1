#Requires -Version 7.0
# rerun-3326: #3326. A re-run's repair save on an already migrated Closed item let Azure DevOps stamp ClosedDate with
# the save time. Seen when the repair re-adds a Git commit link the target cannot resolve (the commit is not in the
# target repo, so the link still points at the source). Seeds a commit in both repos and one in the source repo only,
# and items that cover: the unresolved link (Closed Task, Resolved User Story), a resolvable link and a Hierarchy link
# as controls, an item the assert stamps as an earlier re-run would have, and an item the assert makes genuinely wrong.
param([string]$Src = "amt3326-rsrc", [string]$Tgt = "amt3326-rtgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$so = "nkdagility-adomt-source"; $to = "nkdagility-adomt-target"
Set-E2EOrg $to; New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg $so; $sp = New-E2EProject -Name $Src -Process $Process

function Get-Repo([string]$Org, [string]$Project) {
    $r = (Invoke-E2ERest "$Project/_apis/git/repositories" -Org $Org).value | Where-Object name -eq $Project
    if (-not $r) { $pid_ = (Invoke-E2ERest "_apis/projects/$Project" -Org $Org).id; $r = Invoke-E2ERest "$Project/_apis/git/repositories" -Org $Org -Method POST -Body @{ name = $Project; project = @{ id = $pid_ } } }
    $r
}
$srcRepo = Get-Repo $so $Src
$tgtRepo = Get-Repo $to $Tgt

# git auth through environment config, so the token is never on a command line.
if (-not $env:AMT_TEST_PAT) { $tok = az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv 2>$null; $hdr = "Authorization: Bearer $tok" }
else { $hdr = "Authorization: Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($env:AMT_TEST_PAT)")) }
$env:GIT_CONFIG_COUNT = "1"; $env:GIT_CONFIG_KEY_0 = "http.extraHeader"; $env:GIT_CONFIG_VALUE_0 = $hdr
$env:GIT_AUTHOR_NAME = "amte2e"; $env:GIT_AUTHOR_EMAIL = "amte2e@example.invalid"; $env:GIT_COMMITTER_NAME = "amte2e"; $env:GIT_COMMITTER_EMAIL = "amte2e@example.invalid"
$work = Join-Path ([IO.Path]::GetTempPath()) "amte2e-3326-git-$Src"
try {
    function Push-E2ERepo([string]$Url) { $o = git push -q $Url main 2>&1; if ($LASTEXITCODE -ne 0) { throw "git push to $Url failed ($LASTEXITCODE): $o" } }
    if (Test-Path $work) { Remove-Item $work -Recurse -Force }
    New-Item -ItemType Directory $work | Out-Null
    Push-Location $work
    git init -q -b main main; Set-Location main
    Set-Content a.txt "in both"; git add a.txt; git commit -q -m "C1 in both"; $c1 = (git rev-parse HEAD).Trim()
    Push-E2ERepo $tgtRepo.remoteUrl
    Set-Content a.txt "source only"; git commit -q -am "C2 source only"; $c2 = (git rev-parse HEAD).Trim()
    Push-E2ERepo $srcRepo.remoteUrl
}
finally {
    Pop-Location
    foreach ($v in "GIT_CONFIG_COUNT", "GIT_CONFIG_KEY_0", "GIT_CONFIG_VALUE_0", "GIT_AUTHOR_NAME", "GIT_AUTHOR_EMAIL", "GIT_COMMITTER_NAME", "GIT_COMMITTER_EMAIL") { Remove-Item "env:$v" -ErrorAction SilentlyContinue }
    $tok = $null; $hdr = $null
    if (Test-Path $work) { Remove-Item $work -Recurse -Force }
}
$resolvable = "vstfs:///Git/Commit/$($sp.id)%2F$($srcRepo.id)%2F$c1"
$unresolved = "vstfs:///Git/Commit/$($sp.id)%2F$($srcRepo.id)%2F$c2"
function CommitOp([string]$Uri) { @{ op = "add"; path = "/relations/-"; value = @{ rel = "ArtifactLink"; url = $Uri; attributes = @{ name = "Fixed in Commit" } } } }
function New-Item3326([string]$Type, [string]$Title, [string[]]$States, [object[]]$Ops = @()) {
    $wi = Set-E2EWorkItem $Src $Type -Fields ([ordered]@{ "System.Title" = "3326 $Title" }) -Ops $Ops
    foreach ($state in $States) { Start-Sleep -Seconds 1; Set-E2EWorkItem -Id $wi.id -Fields ([ordered]@{ "System.State" = $state }) | Out-Null }
    $wi.id
}
$s = [ordered]@{}
$s.F = New-Item3326 "Feature" "feature" @("Active")
$s.T1 = New-Item3326 "Task" "T1 unresolved commit link" @("Active", "Closed") @(CommitOp $unresolved)
$s.T2 = New-Item3326 "Task" "T2 resolvable commit link" @("Active", "Closed") @(CommitOp $resolvable)
$s.S1 = New-Item3326 "User Story" "S1 hierarchy link" @("Active", "Resolved", "Closed") @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Reverse" $s.F)
$s.S2 = New-Item3326 "User Story" "S2 resolved, unresolved commit link" @("Active", "Resolved") @(CommitOp $unresolved)
$s.T4 = New-Item3326 "Task" "T4 stamped by an earlier re-run" @("Active", "Closed") @(CommitOp $unresolved)
$s.T5 = New-Item3326 "Task" "T5 genuinely wrong target" @("Active", "Closed")
Save-E2EState $PSScriptRoot $s
"seeded rerun-3326 in ${Src}: " + (($s.Keys | ForEach-Object { "$_=$($s[$_])" }) -join ", ") + "; commits in both $c1, source only $c2"
