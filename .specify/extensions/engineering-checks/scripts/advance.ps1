# Moves the current item to the next state in this repo's order, through the policy check.
param([Parameter(Mandatory)][string]$Reason, [string]$To, [int]$Id)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig
$id = if ($Id) { $Id } else { (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number }
if (-not $To) {
    $adapter = Get-BoardAdapter $cfg
    $item = & $adapter.GetItem $id
    $order = Get-ItemOrder $cfg $item
    # State names match whatever their case, so a board's "building" is the order's "Building".
    $i = Get-StateIndex $order $item.state
    if ($i -lt 0 -or $i -ge $order.Count - 1) { Write-Error "#$id is in $($item.state), which has no next state on its route."; exit 1 }
    $To = $order[$i + 1]
}
& (Join-Path $PSScriptRoot 'move-state.ps1') -Id $id -To $To -Reason $Reason
exit $LASTEXITCODE