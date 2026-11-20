# Adoption step 7: every open item is in a valid state, no WIP limit is broken, and the attention list is shown.
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig
$ad = Get-BoardAdapter $cfg
$items = @(& $ad.GetItems)
$names = @($cfg.states | ForEach-Object { $_.name })
$bad = @($items | Where-Object { $_.state -notin $names -or ($_.ContainsKey('stateSet') -and -not $_.stateSet) })
$over = foreach ($s in $cfg.states) { $n = @($items | Where-Object state -eq $s.name).Count; if ($s.wipLimit -and $n -gt $s.wipLimit) { "$($s.name): $n of $($s.wipLimit)" } }
$report = Invoke-WorkflowManager -Config $cfg -Adapter $ad -WhatIf
New-Item -ItemType Directory -Force .specify/manager | Out-Null
Format-AttentionList $report | Set-Content .specify/manager/attention.md -Encoding utf8
Get-Content .specify/manager/attention.md
if ($bad) { "items in no valid state: $(($bad | ForEach-Object { "#$($_.id)" }) -join ', ')" }
if ($over) { "WIP limits already broken: $($over -join '; ')" }
if ($bad -or $over) { exit 1 }
"every open item is in a valid state and within its WIP limit"
exit 0