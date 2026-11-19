# One manager pass: pulls work into free slots, moves work forward where its policy passes,
# finds hung work, and writes what is waiting on Martin to .specify/manager/attention.md.
# Prints the report as JSON. The harness starts or resumes the WKRs the report's actions name.
param([string]$Root = '.', [switch]$WhatIf)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig -Root $Root
$report = Invoke-WorkflowManager -Config $cfg -Root $Root -WhatIf:$WhatIf
$dir = Join-Path $Root '.specify/manager'; New-Item -ItemType Directory -Force $dir | Out-Null
Format-AttentionList $report | Set-Content (Join-Path $dir 'attention.md') -Encoding utf8
$report | ConvertTo-Json -Depth 6
exit 0