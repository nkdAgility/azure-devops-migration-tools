#Requires -Version 7.0
# longrun-3305: #3305. A long first migration saved later work items with an empty State Change Date. Seeds
# $Features Features, each the parent of $StoriesPerFeature User Stories. Stories go New -> Active -> Resolved -> Closed
# and Features New -> Active, every change stamped by the server, so every revision has a State Change Date and the
# state-stamped dates that go with its state.
param([string]$Src = "amt3305-long-src", [string]$Tgt = "amt3305-long-tgt", [string]$Process = "amte2e-agile",
    [int]$Features = 100, [int]$StoriesPerFeature = 2)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

function Set-PlainState([int]$Id, [string]$State) { Set-E2EWorkItem -Id $Id -Fields ([ordered]@{ "System.State" = $State }) | Out-Null }
$ids = @()
for ($f = 1; $f -le $Features; $f++) {
    $feature = Set-E2EWorkItem $Src "Feature" -Fields ([ordered]@{ "System.Title" = "3305 feature $f" })
    Set-PlainState $feature.id "Active"
    $ids += $feature.id
    for ($s = 1; $s -le $StoriesPerFeature; $s++) {
        $story = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3305 feature $f story $s" }) -Ops @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Reverse" $feature.id)
        foreach ($state in "Active", "Resolved", "Closed") { Set-PlainState $story.id $state }
        $ids += $story.id
    }
    if ($f % 10 -eq 0) { "seeded $f of $Features features ($($ids.Count) items)" }
}
Save-E2EState $PSScriptRoot @{ ids = $ids }
"seeded longrun-3305: $($ids.Count) items in $Src"
