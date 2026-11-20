# Adoption step 3: the reconcile dry run. Writes .specify/adopt/reconcile.json and reconcile.md. Changes nothing.
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig
$plan = Get-AdoptionPlan -Config $cfg -Adapter (Get-BoardAdapter $cfg)
New-Item -ItemType Directory -Force .specify/adopt | Out-Null
$plan | ConvertTo-Json -Depth 5 | Set-Content .specify/adopt/reconcile.json -Encoding utf8
Format-AdoptionPlan $plan | Set-Content .specify/adopt/reconcile.md -Encoding utf8
Get-Content .specify/adopt/reconcile.md
exit 0