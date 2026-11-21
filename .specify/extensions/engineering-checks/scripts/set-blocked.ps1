# Items never move back. A stuck item keeps its state and is tagged `blocked` plus one tag saying why, so the
# manager and the worker know what needs doing:
#   spec-reconcile-required  the agreed spec cannot be met as written; the worker revises it for Martin's approval
#   outcome-check-failed     an outcome check failed in Validating; the worker fixes the code there
#   more-information-needed  waiting for an answer the worker cannot find itself
# -Clear removes `blocked` and the reason tag once the cause is settled.
#   set-blocked.ps1 -Why spec-reconcile-required [-Id 123]
#   set-blocked.ps1 -Why spec-reconcile-required -Clear [-Id 123]
param([Parameter(Mandatory)][ValidateSet('spec-reconcile-required', 'outcome-check-failed', 'more-information-needed')][string]$Why, [switch]$Clear, [int]$Id)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig
$ad = Get-BoardAdapter $cfg
if (-not $Id) { $Id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number }
$item = & $ad.GetItem $Id
if (-not $item) { Write-Error "Item $Id not found."; exit 1 }
& $ad.SetTag $item $Why (-not $Clear)
$item = & $ad.GetItem $Id
$reasons = @($cfg.blockedReasons | Where-Object { $item.tags -contains $_ })
# `blocked` stays while any reason remains.
& $ad.SetTag $item $cfg.tags.blocked ([bool]$reasons.Count)
if ($Clear) { "#${Id}: $Why cleared$(if ($reasons.Count) { "; still blocked: $($reasons -join ', ')" } else { '; no longer blocked' })" }
else { "#${Id}: blocked ($Why); it keeps its state, $($item.state)" }
exit 0
