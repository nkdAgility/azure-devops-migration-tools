#Requires -Version 7.0
# rerun-3299-ambiguous: target L has a Related link to one of the two target items that reflect D.
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$s = Read-E2EState $PSScriptRoot
Set-E2EOrg nkdagility-adomt-target
$tL = Invoke-E2ERest "_apis/wit/workitems/$($s.targetL)?`$expand=relations"
$related = @($tL.relations | Where-Object rel -eq "System.LinkTypes.Related" | ForEach-Object { [int]($_.url -split '/')[-1] })
if (-not ($related | Where-Object { $_ -in @([int]$s.targetD, [int]$s.copy) })) { "FAIL: target L $($s.targetL) has no Related link to D ($($s.targetD) or $($s.copy)): $($related -join ',')"; exit 1 }
"PASS: target L $($s.targetL) is Related to $($related -join ',') (D is $($s.targetD), its copy $($s.copy))"
