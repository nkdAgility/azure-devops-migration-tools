#Requires -Version 7.0
# prlinks (#3286) assertions. -Expect Broken describes the result before the fix (the target keeps the source artifact
# link, which the target organisation cannot resolve); -Expect Fixed (default) the result after it.
param([string]$OutFile, [Parameter(Mandatory)][string]$LogDirectory, [ValidateSet("Fixed", "Broken")][string]$Expect = "Fixed")
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()

$t = Find-E2ETargetItem -Project $Tgt -SourceId $s.item
$r += New-E2EResult "item migrated" ($null -ne $t) "target $($t.id)"
$prArtifacts = @($t.relations | Where-Object { $_.rel -eq "ArtifactLink" -and $_.url -match "PullRequestId" })
$prHyperlinks = @($t.relations | Where-Object { $_.rel -eq "Hyperlink" -and $_.url -eq $s.prWebUrl })
# The target organisation resolves a vstfs pull request link only if it holds the repository.
$targetHasRepo = try { Invoke-E2ERest "_apis/git/repositories/$($s.repoId)" -Org nkdagility-adomt-target | Out-Null; $true } catch { $false }
$r += New-E2EResult "target organisation has no copy of the source repository" (-not $targetHasRepo) "GET target _apis/git/repositories/$($s.repoId)"

if ($Expect -eq "Broken") {
    $r += New-E2EResult "target keeps the source pull request artifact link" ($prArtifacts.Count -eq 1) ($prArtifacts.url -join ", ")
    $r += New-E2EResult "that link cannot be resolved on the target" ($prArtifacts.Count -eq 1 -and -not $targetHasRepo) "repository $($s.repoId) not in target"
    $r += New-E2EResult "no hyperlink to the pull request" ($prHyperlinks.Count -eq 0) "$($prHyperlinks.Count)"
}
else {
    $r += New-E2EResult "no pull request artifact link left on the target" ($prArtifacts.Count -eq 0) ($prArtifacts.url -join ", ")
    $r += New-E2EResult "exactly one hyperlink to the source pull request" ($prHyperlinks.Count -eq 1) "$($prHyperlinks.Count) x $($s.prWebUrl)"
    $comment = $prHyperlinks | Select-Object -First 1 | ForEach-Object { $_.attributes.comment }
    $r += New-E2EResult "hyperlink comment names the pull request" ($comment -eq "Pull request $($s.prId): $($s.prTitle)") "comment '$comment'"
    $web = try { (Invoke-WebRequest $s.prWebUrl -Headers @{ Authorization = "Bearer $(az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv 2>$null)" } -MaximumRedirection 0 -SkipHttpErrorCheck).StatusCode } catch { 0 }
    $r += New-E2EResult "hyperlink opens the source pull request" ($web -eq 200) "HTTP $web"
}
$log = Get-ChildItem $LogDirectory -Recurse -Filter "migration2*.log" | Sort-Object LastWriteTime | Select-Object -Last 1
$text = if ($log) { Get-Content $log.FullName -Raw } else { "" }
$r += New-E2EResult "run log found" ($null -ne $log) "LogDirectory $LogDirectory"
$r += New-E2EResult "no stack frames in the log" ($text -notmatch "(?m)^\s+at ") ""
$r += New-E2EResult "no errors in the log" ($text -notmatch "\d (ERR|FTL)\]") ""
exit (Write-E2EResults $r $OutFile)
