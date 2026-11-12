# statedates assertions: every item migrated once, revision history and final Activated/Resolved/Closed dates match the source.
param([string]$Src = "amte2e-3240-src", [string]$Tgt = "amte2e-3240-tgt", [string]$OutFile)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()
foreach ($k in "d1", "d2", "d3", "d4", "d5") { $r += Test-E2EItemFidelity -SourceId $s.$k -SrcProject $Src -TgtProject $Tgt }
$tgtItems = @(Get-E2EAllWorkItems $Tgt -Org nkdagility-adomt-target)
$r += New-E2EResult "target item count (no duplicates)" ($tgtItems.Count -eq 5) "$($tgtItems.Count) items"
exit (Write-E2EResults $r $OutFile)
