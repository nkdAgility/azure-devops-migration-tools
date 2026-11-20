#Requires -Version 7.0
# rerun-3299-ambiguous: a link whose other end has two target items (#3299 review).
# Source: D, and L with a Related link to D. After the first migration, ambiguate.ps1 adds a second target item that holds
# D's reflected work item ID, and deletes L's link on the target. A re-run must not count L as in sync: the map cannot
# say which target item is D, so L goes through the link migration, which finds D by query and restores the link.
param([string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
$cfg = (Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json).MigrationTools.Endpoints
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $cfg.Target.Project -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $cfg.Source.Project -Process $Process | Out-Null
$dItem = Set-E2EWorkItem $cfg.Source.Project "User Story" -Fields ([ordered]@{ "System.Title" = "amb3299 D" })
$lItem = Set-E2EWorkItem $cfg.Source.Project "User Story" -Fields ([ordered]@{ "System.Title" = "amb3299 L" }) -Ops @(Add-E2ELinkOp "System.LinkTypes.Related" $dItem.id "to D")
Save-E2EState $PSScriptRoot @{ d = $dItem.id; l = $lItem.id }
"seeded ambiguous: D $($dItem.id), L $($lItem.id) Related to D"
