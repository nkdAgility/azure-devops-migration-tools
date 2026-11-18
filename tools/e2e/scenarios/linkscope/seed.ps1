# linkscope: #3271. Related links whose target the run cannot migrate.
# A (in the query) links to B (in the query, the control: that link must arrive), to O (exists, outside the query) and
# to X, an item in a project that is then deleted. The link to X stays on A, but loading X fails with TF26198
# (DeniedOrNotExistException), as it does for a target the migration identity cannot read.
# Deleting a work item is not enough: Azure DevOps removes its links from the other items.
param([string]$Src = "amte2e-3271-lnk-src", [string]$Tgt = "amte2e-3271-lnk-tgt", [string]$FailTgt = "amte2e-3271-lnkfail-tgt", [string]$Gone = "amte2e-3271-lnk-gone", [string]$Process = "amte2e-agile", [string]$NoBugProcess = "amte2e-nobug")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
# config-fail.json: Bug is disabled in this target, so the Bug fails and the run must exit 1 although the link skips are not failures.
New-E2EProject -Name $FailTgt -Process $NoBugProcess | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null
New-E2EProject -Name $Gone -Process $Process | Out-Null

$b = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "in-scope B" })
$o = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "out-of-scope O" })
$x = Set-E2EWorkItem $Gone "User Story" -Fields ([ordered]@{ "System.Title" = "X, in a project that is then deleted" })
$a = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "in-scope A" })
Start-Sleep -Seconds 2
Set-E2EWorkItem -Id $a.id -Ops @(
    (Add-E2ELinkOp "System.LinkTypes.Related" $b.id "to B"),
    (Add-E2ELinkOp "System.LinkTypes.Related" $o.id "to O"),
    (Add-E2ELinkOp "System.LinkTypes.Related" $x.id "to X")) | Out-Null
Remove-E2EProject $Gone
$bug = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "fail-scope Bug" })

Save-E2EState $PSScriptRoot @{ a = $a.id; b = $b.id; o = $o.id; x = $x.id; bug = $bug.id }
"seeded linkscope: A $($a.id) B $($b.id) O $($o.id) (outside the query) X $($x.id) (project deleted)"
