# Moves the current item to the next state in this repo's order, through the policy check.
param([Parameter(Mandatory)][string]$Reason, [string]$To)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
if (-not $To) {
    $adapter = Get-BoardAdapter $cfg
    $state = (& $adapter.GetItem $id).state
    $To = $cfg.order[$cfg.order.IndexOf($state) + 1]
}
& (Join-Path $PSScriptRoot 'move-state.ps1') -Id $id -To $To -Reason $Reason
exit $LASTEXITCODE