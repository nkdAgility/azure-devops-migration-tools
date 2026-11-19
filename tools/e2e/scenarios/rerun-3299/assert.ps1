#Requires -Version 7.0
# rerun-3299, after the re-run that follows diff.ps1: the three differences are fixed and no other item has a new revision.
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$state = Read-E2EState $PSScriptRoot
$stories = @($state.stories); $features = @($state.features)
$failures = @()
Set-E2EOrg nkdagility-adomt-target
function TargetOf([int]$sourceId) { Find-E2ETargetItem $Tgt $sourceId }
function RelatedIds($wi, [string]$rel) { @($wi.relations | Where-Object rel -eq $rel | ForEach-Object { [int]($_.url -split '/')[-1] }) }

$revisionTarget = TargetOf $state.diff.revisionItem
if ($revisionTarget.fields.'System.Description' -notmatch 'changed after the first migration') { $failures += "missing revision not replayed on $($revisionTarget.id)" }

$linkTarget = TargetOf $state.diff.linkItem
# State values come back as Int64, so find the index by value rather than with [array]::IndexOf.
$linkIndex = @(0..($stories.Count - 1) | Where-Object { [long]$stories[$_] -eq [long]$state.diff.linkItem })[0]
$previousTarget = TargetOf $stories[$linkIndex - 1]
if ((RelatedIds $linkTarget "System.LinkTypes.Related") -notcontains $previousTarget.id) { $failures += "deleted Related link not restored on $($linkTarget.id)" }

$parentTarget = TargetOf $state.diff.parentItem
$rightParent = TargetOf $features[2]
$parents = RelatedIds $parentTarget "System.LinkTypes.Hierarchy-Reverse"
if ($parents.Count -ne 1 -or $parents[0] -ne $rightParent.id) { $failures += "target $($parentTarget.id) has parent(s) $($parents -join ',') instead of $($rightParent.id)" }

$changed = @($revisionTarget.id, $linkTarget.id, $parentTarget.id, $rightParent.id, (TargetOf $features[3]).id, $previousTarget.id)
$touched = @()
foreach ($wi in Get-E2EAllWorkItems $Tgt) {
    $before = $state.diff.revs."$($wi.id)"
    if ($wi.rev -ne $before -and $changed -notcontains $wi.id) { $touched += "$($wi.id) rev $before -> $($wi.rev)" }
}
if ($touched) { $failures += "in-sync items got new revisions: $($touched -join '; ')" }

if ($failures) { $failures | ForEach-Object { "FAIL: $_" }; exit 1 }
"PASS: missing revision replayed, deleted link restored, child moved back under $($rightParent.id); no other item has a new revision ($(@($state.diff.revs.PSObject.Properties).Count) items checked)"
