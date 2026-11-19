# Brings the spec's results onto the item: Spec (where it is), Red team (the challenge) and Architecture check.
$ErrorActionPreference = 'Stop'
$rec = Join-Path $PSScriptRoot 'record.ps1'
$root = 'docs/specs'
if (Test-Path .specify/memory/workflow.json) { $c = Get-Content .specify/memory/workflow.json -Raw | ConvertFrom-Json; if ($c.PSObject.Properties['specRoot'] -and $c.specRoot) { $root = $c.specRoot.TrimEnd('/') } }
$spec = Get-ChildItem specs -Recurse -Filter spec.md -ErrorAction SilentlyContinue | Sort-Object LastWriteTime | Select-Object -Last 1
if (-not $spec) { Write-Error 'No spec.md under specs/.'; exit 1 }
& $rec -Section 'Spec' -Body "Working spec: ``$([IO.Path]::GetRelativePath((Get-Location), $spec.FullName))`` (deleted once reconciled into $root/)."
foreach ($p in @(@('Red team', '.specify/reviews/challenge.md'), @('Architecture check', '.specify/reviews/arch-check.md'))) {
    if (-not (Test-Path $p[1])) { Write-Error "$($p[1]) was not written."; exit 1 }
    & $rec -Section $p[0] -Body (Get-Content $p[1] -Raw)
}
exit 0