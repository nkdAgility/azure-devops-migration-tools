# Adoption step 5, after Martin approves the dry run: applies .specify/adopt/reconcile.json exactly as approved.
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig
$plan = Get-Content .specify/adopt/reconcile.json -Raw | ConvertFrom-Json -AsHashtable
Invoke-AdoptionApply -Config $cfg -Adapter (Get-BoardAdapter $cfg) -Plan @($plan)
"applied $(@($plan).Count) items"
exit 0