# A WKR's watch on its item. Run it on a timer (or when woken). It records answers to open questions,
# sets or clears 'blocked', and prints what changed as JSON so the WKR can act on it.
# Usage: watch.ps1 -Id 123 [-Root .]
param([Parameter(Mandatory)]$Id, [string]$Root = '.')
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig -Root $Root
Sync-ItemWatch -Config $cfg -Id $Id -Root $Root | ConvertTo-Json -Depth 4
exit 0