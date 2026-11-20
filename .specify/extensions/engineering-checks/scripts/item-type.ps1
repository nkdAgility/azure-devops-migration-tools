# Prints the current item's type as JSON, for the workflow's 'if' steps.
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$t = (& (Get-BoardAdapter $cfg).GetItem $id).type
@{ type = "$t"; fullBar = ($t -eq 'Product Backlog Item') } | ConvertTo-Json -Compress
exit 0