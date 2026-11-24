#Requires -Version 7.0
# linkscope: #3271. Related links whose target the run cannot migrate.
# A (in the query) links to B (in the query, the control: that link must arrive), to O (exists, outside the query) and
# to X, an item in a project that is then deleted. The link to X stays on A, but loading X fails with TF26198
# (DeniedOrNotExistException), as it does for a target the migration identity cannot read.
# Deleting a work item is not enough: Azure DevOps removes its links from the other items.
param([string]$Process = "amte2e-agile", [string]$NoBugProcess = "amte2e-nobug")
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
# Project names come from the scenario config, so the seed, the migration and the assertions always agree.
$Src = $cfg.MigrationTools.Endpoints.Source.Project
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
$FailTgt = (Get-Content (Join-Path $PSScriptRoot "config-fail.json") -Raw | ConvertFrom-Json).MigrationTools.Endpoints.Target.Project
$Gone = $Src -replace '-src$', '-gone'
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
# config-fail.json: Bug is disabled in this target, so the Bug fails and the run must exit 1 although the link skips are not failures.
New-E2EProject -Name $FailTgt -Process $NoBugProcess | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null
# Gone is the one project the pool cannot give (#3342): it is deleted below, so it is made in the source org only.
New-E2EDisposableProject -Name $Gone -Process $Process | Out-Null

$b = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "in-scope B" })
$o = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "out-of-scope O" })
$x = Set-E2EWorkItem $Gone "User Story" -Fields ([ordered]@{ "System.Title" = "X, in a project that is then deleted" })
$a = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "in-scope A" })
Start-Sleep -Seconds 2
Set-E2EWorkItem -Id $a.id -Ops @(
    (Add-E2ELinkOp "System.LinkTypes.Related" $b.id "to B"),
    (Add-E2ELinkOp "System.LinkTypes.Related" $o.id "to O"),
    (Add-E2ELinkOp "System.LinkTypes.Related" $x.id "to X")) | Out-Null
# R gets a Related link to X that is removed in a later revision, so replaying that revision removes a link to an item
# the source can't return (TfsWorkItemLinkTool.RemoveLinks).
$rItem = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "in-scope R (link removed)" })
Start-Sleep -Seconds 2
# Each link edit also changes a field: a link-only edit does not create a revision that the migration replays.
Set-E2EWorkItem -Id $rItem.id -Fields ([ordered]@{ "System.Description" = "link to X added" }) -Ops @((Add-E2ELinkOp "System.LinkTypes.Related" $x.id "to X, removed later")) | Out-Null
Start-Sleep -Seconds 2
Set-E2EWorkItem -Id $rItem.id -Fields ([ordered]@{ "System.Description" = "link to X removed" }) -Ops @(@{ op = "remove"; path = "/relations/0" }) | Out-Null
# T, a Test Case, uses Shared Steps S in the project that is deleted, so remapping its step reference reads an
# item the source can't return (TfsWorkItemLinkTool.MigrateSharedSteps).
$sharedSteps = Set-E2EWorkItem $Gone "Shared Steps" -Fields ([ordered]@{ "System.Title" = "S, shared steps in a project that is then deleted" })
$t = Set-E2EWorkItem $Src "Test Case" -Fields ([ordered]@{ "System.Title" = "in-scope T (uses unreadable shared steps)" }) -Ops @(
    @{ op = "add"; path = "/relations/-"; value = @{ rel = "Microsoft.VSTS.TestCase.SharedStepReferencedBy-Reverse"; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/wit/workItems/$($sharedSteps.id)" } })
Remove-E2EDisposableProject $Gone
$bug = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "fail-scope Bug" })

Save-E2EState $PSScriptRoot @{ a = $a.id; b = $b.id; o = $o.id; x = $x.id; r = $rItem.id; t = $t.id; shared = $sharedSteps.id; bug = $bug.id }
"seeded linkscope: A $($a.id) B $($b.id) O $($o.id) (outside the query) X $($x.id) (project deleted) R $($rItem.id) (link to X removed) T $($t.id) (uses shared steps $($sharedSteps.id))"
