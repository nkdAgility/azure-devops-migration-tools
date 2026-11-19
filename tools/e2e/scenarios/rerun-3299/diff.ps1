#Requires -Version 7.0
# rerun-3299, after the first migration: seed three differences and snapshot every target item's revision count.
#   1. story 1.2: a new source revision (a missing revision on the target);
#   2. story 2.3: its Related link deleted on the target;
#   3. story 3.4: moved under feature 4 on the target (a child under the wrong parent).
# assert.ps1 then checks that a re-run fixes all three and adds no revision to any other item.
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$state = Read-E2EState $PSScriptRoot
$stories = @($state.stories); $features = @($state.features)
$perFeature = if ($state.storiesPerFeature) { [int]$state.storiesPerFeature } else { 9 }
function StoryId([int]$f, [int]$s) { $stories[($f - 1) * $perFeature + ($s - 1)] }

Set-E2EOrg nkdagility-adomt-source
$revisionItem = StoryId 1 2
$description = "changed after the first migration (#3299) at $(Get-Date -Format o)"
Set-E2EWorkItem -Id $revisionItem -Fields ([ordered]@{ "System.Description" = $description }) | Out-Null

Set-E2EOrg nkdagility-adomt-target
$linkItem = StoryId 2 3
$linkTarget = Find-E2ETargetItem $Tgt $linkItem
# Remove the link to the previous story, the one assert.ps1 checks; the order of relations is not guaranteed.
$previousTarget = Find-E2ETargetItem $Tgt (StoryId 2 2)
$relations = @($linkTarget.relations)
$related = @(0..($relations.Count - 1) | Where-Object { $relations[$_].rel -eq "System.LinkTypes.Related" -and [int](($relations[$_].url -split "/")[-1]) -eq $previousTarget.id })[0]
if ($null -eq $related) { throw "target $($linkTarget.id) has no Related link to $($previousTarget.id)" }
Set-E2EWorkItem -Id $linkTarget.id -Ops @(@{ op = "remove"; path = "/relations/$related" }) | Out-Null

$parentItem = StoryId 3 4
$parentTarget = Find-E2ETargetItem $Tgt $parentItem
$wrongParent = Find-E2ETargetItem $Tgt $features[3]
$parentIndex = [array]::IndexOf(@($parentTarget.relations | ForEach-Object rel), "System.LinkTypes.Hierarchy-Reverse")
Set-E2EWorkItem -Id $parentTarget.id -Ops @(
    @{ op = "remove"; path = "/relations/$parentIndex" },
    @{ op = "add"; path = "/relations/-"; value = @{ rel = "System.LinkTypes.Hierarchy-Reverse"; url = "https://dev.azure.com/nkdagility-adomt-target/_apis/wit/workItems/$($wrongParent.id)" } }) | Out-Null

$revs = @{}
foreach ($wi in Get-E2EAllWorkItems $Tgt) { $revs["$($wi.id)"] = $wi.rev }
$state | Add-Member -Force NoteProperty diff @{ revisionItem = $revisionItem; description = $description; linkItem = $linkItem; parentItem = $parentItem; revs = $revs }
Save-E2EState $PSScriptRoot ($state | ConvertTo-Json -Depth 10 | ConvertFrom-Json -AsHashtable)
"seeded differences: new source revision on $revisionItem, Related link deleted on target $($linkTarget.id), target $($parentTarget.id) moved under $($wrongParent.id)"
