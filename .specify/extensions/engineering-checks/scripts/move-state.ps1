# The only way an item's state changes. Checks the policy to leave its current state, then moves it and records why.
# Usage: move-state.ps1 -Id 123 -To Specified -Reason "spec written and challenged"
# Exits 0 when moved (or already there), 1 when the policy refuses the move, and prints what failed.
param([Parameter(Mandatory)]$Id, [Parameter(Mandatory)][string]$To, [Parameter(Mandatory)][string]$Reason, [string]$Root = '.', [switch]$WhatIf)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig -Root $Root
$r = Move-WorkItem -Config $cfg -Id $Id -To $To -Reason $Reason -Root $Root -WhatIf:$WhatIf
if ($r.ok) { if ($r.moved) { "moved #$Id $($r.from) -> $($r.to) ($($r.shift))" } else { "#$Id $($r.message ?? "can move $($r.from) -> $($r.to)")" }; exit 0 }
"refused: #$Id cannot move $($r.from) -> $($r.to):"; $r.failures | ForEach-Object { "  - $_" }
exit 1