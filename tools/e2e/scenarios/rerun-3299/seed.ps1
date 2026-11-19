#Requires -Version 7.0
# rerun-3299: a re-run over items that are already migrated and in sync must be fast and touch nothing (#3299).
# 30 Features, each with 9 child User Stories; each story has a Related link to the previous story and every third story
# a hyperlink, so a re-run has links to compare. 300 items in all.
param([string]$Process = "amte2e-agile", [int]$FeatureCount = 30, [int]$StoriesPerFeature = 9)
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
$Src = $cfg.MigrationTools.Endpoints.Source.Project
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

$featureIds = @(); $storyIds = @()
for ($f = 1; $f -le $FeatureCount; $f++) {
    $feature = Set-E2EWorkItem $Src "Feature" -Fields ([ordered]@{ "System.Title" = "rerun3299 feature $f" })
    $featureIds += $feature.id
    $previous = $null
    for ($s = 1; $s -le $StoriesPerFeature; $s++) {
        $ops = @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Reverse" $feature.id "parent")
        if ($previous) { $ops += Add-E2ELinkOp "System.LinkTypes.Related" $previous "previous story" }
        if ($s % 3 -eq 0) { $ops += @{ op = "add"; path = "/relations/-"; value = @{ rel = "Hyperlink"; url = "https://example.com/rerun3299/$f/$s" } } }
        $story = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "rerun3299 story $f.$s"; "System.Description" = "story $s of feature $f" }) -Ops $ops
        $storyIds += $story.id
        $previous = $story.id
    }
}
Save-E2EState $PSScriptRoot @{ features = $featureIds; stories = $storyIds; storiesPerFeature = $StoriesPerFeature }
"seeded rerun-3299: $($featureIds.Count) features, $($storyIds.Count) stories in $Src"

