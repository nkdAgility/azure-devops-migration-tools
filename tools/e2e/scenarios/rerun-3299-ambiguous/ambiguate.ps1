#Requires -Version 7.0
# rerun-3299-ambiguous, after a migration: add a second target item holding D's reflected work item ID and delete L's link.
$ErrorActionPreference = "Stop"
$cfg = (Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json).MigrationTools.Endpoints
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$s = Read-E2EState $PSScriptRoot
Set-E2EOrg nkdagility-adomt-target
$tD = Find-E2ETargetItem $cfg.Target.Project $s.d; $tL = Find-E2ETargetItem $cfg.Target.Project $s.l
$copy = if ($s.copy) { [int]$s.copy } else {
    (Set-E2EWorkItem $cfg.Target.Project "User Story" -Fields ([ordered]@{ "System.Title" = "amb3299 D (second target copy)"; "Custom.ReflectedWorkItemId" = $tD.fields.'Custom.ReflectedWorkItemId' })).id }
$relations = @($tL.relations)
$i = @(0..([Math]::Max(0, $relations.Count - 1)) | Where-Object { $relations.Count -gt 0 -and $relations[$_].rel -eq "System.LinkTypes.Related" })
if ($i.Count -eq 0) { throw "target L $($tL.id) has no Related link to delete" }
Set-E2EWorkItem -Id $tL.id -Ops @(@{ op = "remove"; path = "/relations/$($i[0])" }) | Out-Null
Save-E2EState $PSScriptRoot @{ d = $s.d; l = $s.l; copy = $copy; targetD = [int]$tD.id; targetL = [int]$tL.id }
"ambiguated: target D $($tD.id) and copy $copy both reflect source $($s.d); L's link deleted on target $($tL.id)"
