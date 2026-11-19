#Requires -Version 7.0
# rerun-3299-crossproject: links between work items in two projects, with AllowCrossProjectLinking (#3299).
# Source project B holds feature BF and story B1; source project A holds story A1 (Related to B1) and story A2 (child of BF).
# config-b.json migrates B to its own target project first; config.json then migrates A, whose links must reach B's target items.
param([string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
$a = (Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json).MigrationTools.Endpoints
$b = (Get-Content (Join-Path $PSScriptRoot "config-b.json") -Raw | ConvertFrom-Json).MigrationTools.Endpoints
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $a.Target.Project -Process $Process | Out-Null
New-E2EProject -Name $b.Target.Project -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $a.Source.Project -Process $Process | Out-Null
New-E2EProject -Name $b.Source.Project -Process $Process | Out-Null
$bf = Set-E2EWorkItem $b.Source.Project "Feature" -Fields ([ordered]@{ "System.Title" = "xp3299 BF" })
$b1 = Set-E2EWorkItem $b.Source.Project "User Story" -Fields ([ordered]@{ "System.Title" = "xp3299 B1" })
$a1 = Set-E2EWorkItem $a.Source.Project "User Story" -Fields ([ordered]@{ "System.Title" = "xp3299 A1" }) -Ops @(Add-E2ELinkOp "System.LinkTypes.Related" $b1.id "to B1")
$a2 = Set-E2EWorkItem $a.Source.Project "User Story" -Fields ([ordered]@{ "System.Title" = "xp3299 A2" }) -Ops @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Reverse" $bf.id "parent BF")
Save-E2EState $PSScriptRoot @{ bf = $bf.id; b1 = $b1.id; a1 = $a1.id; a2 = $a2.id }
"seeded crossproject: B $($b.Source.Project) BF $($bf.id) B1 $($b1.id); A $($a.Source.Project) A1 $($a1.id) -> B1, A2 child of BF"
