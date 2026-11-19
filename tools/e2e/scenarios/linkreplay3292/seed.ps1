#Requires -Version 7.0
# linkreplay3292: #3292 and #3293. Children whose parent changed in the source history, migrated before their earlier
# parents, so replaying an earlier parent meets a child that already has its final parent in the target.
# S1 child re-parented A -> B. S2 child re-parented A -> B -> C. S3 parent created with the child and the link removed
# straight after; the child's final parent is another item. S4 a parent gaining several children across revisions, some
# already linked from the child side, and a child whose parent changes during replay (N4 -> M4).
# Migration order is ChangedDate desc, so the last items touched below migrate first.
param([string]$Src = "amte2e-3292-src", [string]$Tgt = "amte2e-3292-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

function New-Item3292([string]$Type, [string]$Title, [object[]]$Ops = @()) { Set-E2EWorkItem $Src $Type -Fields ([ordered]@{ "System.Title" = $Title }) -Ops $Ops }
# A link-only change makes no revision, so each change also sets a field unless -LinkOnly.
function Step([switch]$LinkOnly) { if ($LinkOnly) { @{} } else { [ordered]@{ "System.Description" = "step $(Get-Date -Format o)" } } }
function Add-Child([int]$Parent, [int]$Child, [switch]$LinkOnly) { Start-Sleep -Seconds 2; Set-E2EWorkItem -Id $Parent -Fields (Step -LinkOnly:$LinkOnly) -Ops @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Forward" $Child) | Out-Null }
function Remove-Child([int]$Parent, [int]$Child, [switch]$LinkOnly) {
    Start-Sleep -Seconds 2
    $wi = Invoke-E2ERest "_apis/wit/workitems/$Parent`?`$expand=relations"
    $i = 0; $idx = -1
    foreach ($r in $wi.relations) { if ($r.rel -eq "System.LinkTypes.Hierarchy-Forward" -and $r.url -match "/$Child$") { $idx = $i }; $i++ }
    if ($idx -lt 0) { throw "No child link $Parent -> $Child" }
    Set-E2EWorkItem -Id $Parent -Fields (Step -LinkOnly:$LinkOnly) -Ops @(@{ op = "remove"; path = "/relations/$idx" }) | Out-Null
}
function Move-Child([int]$From, [int]$To, [int]$Child) { Remove-Child $From $Child; Add-Child $To $Child }
function Touch([int]$Id) { Start-Sleep -Seconds 2; Set-E2EWorkItem -Id $Id -Fields ([ordered]@{ "System.Description" = "touched $(Get-Date -Format o)" }) | Out-Null }

$s = [ordered]@{}
# S1
$s.A1 = (New-Item3292 "Feature" "3292 S1 A").id; $s.B1 = (New-Item3292 "Feature" "3292 S1 B").id; $s.K1 = (New-Item3292 "User Story" "3292 S1 child").id
Add-Child $s.A1 $s.K1; Move-Child $s.A1 $s.B1 $s.K1
# S2
$s.A2 = (New-Item3292 "Feature" "3292 S2 A").id; $s.B2 = (New-Item3292 "Feature" "3292 S2 B").id; $s.C2 = (New-Item3292 "Feature" "3292 S2 C").id
$s.K2 = (New-Item3292 "User Story" "3292 S2 child").id
Add-Child $s.A2 $s.K2; Move-Child $s.A2 $s.B2 $s.K2; Move-Child $s.B2 $s.C2 $s.K2
# S3: the parent is created with the child link, which is removed straight away.
$s.K3 = (New-Item3292 "User Story" "3292 S3 child").id; $s.Q3 = (New-Item3292 "Feature" "3292 S3 final parent").id
$s.P3 = (New-Item3292 "Feature" "3292 S3 transient parent" @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Forward" $s.K3)).id
Remove-Child $s.P3 $s.K3 -LinkOnly; Add-Child $s.Q3 $s.K3
# S4
$s.M4 = (New-Item3292 "Feature" "3292 S4 parent").id; $s.N4 = (New-Item3292 "Feature" "3292 S4 earlier parent").id
foreach ($k in "E1", "E2", "E3", "E4") { $s[$k] = (New-Item3292 "User Story" "3292 S4 child $k").id }
Add-Child $s.M4 $s.E1
Add-Child $s.N4 $s.E4
Add-Child $s.M4 $s.E2; Add-Child $s.M4 $s.E3
Move-Child $s.N4 $s.M4 $s.E4

# Earlier parents last-touched first so they migrate after their children.
foreach ($k in "A1", "A2", "P3", "B2", "E3", "E2", "M4", "B1", "C2", "Q3", "N4", "E4", "E1", "K1", "K2", "K3") { Touch $s[$k] }
Save-E2EState $PSScriptRoot $s
$s | ConvertTo-Json
