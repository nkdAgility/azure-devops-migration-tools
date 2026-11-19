# Records evidence on the item's WKR file: a line (-Line "Spec agreed: ...") or a section (-Section "Output done evidence" -Body "...").
param([string]$Line, [string]$Section, [string]$Body = '')
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$wkr = ".specify/wkr/$id.md"
if ($Line) { Add-Content $wkr $Line }
if ($Section) { Add-Content $wkr "`n## $Section`n`n$Body`n" }
exit 0