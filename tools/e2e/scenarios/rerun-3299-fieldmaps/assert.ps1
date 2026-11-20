#Requires -Version 7.0
# rerun-3299-fieldmaps: every target story has the Priority the run's field maps give it.
# -ExpectPriority 2 after config.json (no field maps), 1 after config-fieldmap.json.
param([Parameter(Mandatory)][int]$ExpectPriority)
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Set-E2EOrg nkdagility-adomt-target
$items = @(Get-E2EAllWorkItems $Tgt)
$wrong = @($items | Where-Object { [int]$_.fields.'Microsoft.VSTS.Common.Priority' -ne $ExpectPriority } | ForEach-Object { "$($_.id)=$($_.fields.'Microsoft.VSTS.Common.Priority')" })
$revs = ($items | ForEach-Object { "$($_.id):r$($_.rev)" }) -join ' '
if ($items.Count -eq 0) { "FAIL: no target items"; exit 1 }
if ($wrong) { "FAIL: Priority is not $ExpectPriority on $($wrong -join ', ')"; exit 1 }
"PASS: all $($items.Count) target stories have Priority $ExpectPriority -- revisions $revs"
