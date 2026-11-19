# Prints this repo's order setting as JSON, for the workflow's 'if' step.
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
@{ beforeMerge = [bool](Get-WorkflowConfig).validatingBeforeMerge } | ConvertTo-Json -Compress
exit 0