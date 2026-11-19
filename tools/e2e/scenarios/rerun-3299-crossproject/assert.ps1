#Requires -Version 7.0
# rerun-3299-crossproject: A1 is Related to B1's target and A2's parent is BF's target, both in B's target project.
$ErrorActionPreference = "Stop"
$a = (Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json).MigrationTools.Endpoints
$b = (Get-Content (Join-Path $PSScriptRoot "config-b.json") -Raw | ConvertFrom-Json).MigrationTools.Endpoints
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$s = Read-E2EState $PSScriptRoot
Set-E2EOrg nkdagility-adomt-target
function Ids($wi, $rel) { @($wi.relations | Where-Object rel -eq $rel | ForEach-Object { [int]($_.url -split '/')[-1] }) }
$tA1 = Find-E2ETargetItem $a.Target.Project $s.a1; $tA2 = Find-E2ETargetItem $a.Target.Project $s.a2
$tB1 = Find-E2ETargetItem $b.Target.Project $s.b1; $tBF = Find-E2ETargetItem $b.Target.Project $s.bf
$fail = @()
if (-not ($tA1 -and $tA2 -and $tB1 -and $tBF)) { "FAIL: target items missing"; exit 1 }
if ((Ids $tA1 "System.LinkTypes.Related") -notcontains $tB1.id) { $fail += "A1 $($tA1.id) is not Related to B1 $($tB1.id): $((Ids $tA1 'System.LinkTypes.Related') -join ',')" }
$p = Ids $tA2 "System.LinkTypes.Hierarchy-Reverse"
if ($p.Count -ne 1 -or $p[0] -ne $tBF.id) { $fail += "A2 $($tA2.id) parent is $($p -join ',') not BF $($tBF.id)" }
if ($fail) { $fail | ForEach-Object { "FAIL: $_" }; exit 1 }
"PASS: A1 $($tA1.id) Related to B1 $($tB1.id), A2 $($tA2.id) under BF $($tBF.id) -- revisions A1 r$($tA1.rev) A2 r$($tA2.rev) B1 r$($tB1.rev) BF r$($tBF.rev)"
