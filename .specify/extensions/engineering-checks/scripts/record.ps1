# Records evidence on the item's record, specs/<issue>-<name>/item.md: a line (-Line "Spec agreed: ...") or a section
# (-Section "Output done evidence" -Body "...").
param([string]$Line, [string]$Section, [string]$Body = '', [int]$Id)
$ErrorActionPreference = 'Stop'
$id = if ($Id) { $Id } else { (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number }
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$wkr = Get-ItemRecordPath '.' $id
New-Item -ItemType Directory -Force (Split-Path $wkr) | Out-Null
if ($Line) { Add-Content $wkr $Line }
if ($Section) { Add-Content $wkr "`n## $Section`n`n$Body`n" }
exit 0