#Requires -Version 7.0
# rerun-3299-fieldmaps: a field map added after the first migration must still be applied by a re-run (#3299).
# 10 User Stories with the default Priority (2). config.json migrates them with no field maps; config-fieldmap.json adds a
# FieldLiteralMap that sets Priority to 1, which a re-run must apply even though nothing changed in the source.
param([string]$Process = "amte2e-agile", [int]$Count = 10)
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
$Src = $cfg.MigrationTools.Endpoints.Source.Project
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null
$ids = @(for ($i = 1; $i -le $Count; $i++) { (Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "fm3299 story $i" })).id })
Save-E2EState $PSScriptRoot @{ stories = $ids }
"seeded rerun-3299-fieldmaps: $($ids.Count) stories in $Src"
