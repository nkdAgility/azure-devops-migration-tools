#Requires -Version 7.0
# wicore-fresh (#3342): wicore's seed and assertions, but the target is a brand-new project made for this run and
# deleted after it (by the proof), so a migration into a new project is proven every run. The source comes from the pool.
param([string]$Src = "amte2e-wicorefresh-src", [string]$Tgt = "amte2e-wicorefresh-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Set-E2EOrg nkdagility-adomt-target
$fresh = New-E2EDisposableProject -Name "$Tgt-fresh" -Process $Process
"made the fresh target project nkdagility-adomt-target/$($fresh.name)"
& (Join-Path $PSScriptRoot "..\wicore\seed.ps1") -Src $Src -Tgt "$Tgt-fresh" -Process $Process -FreshTarget
exit $LASTEXITCODE
