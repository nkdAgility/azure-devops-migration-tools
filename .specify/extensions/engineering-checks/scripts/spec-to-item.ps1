# Brings the spec's results onto the item: Spec (where it is), and Red team (the challenge); the architecture check is done in Refining.
$ErrorActionPreference = 'Stop'
$rec = Join-Path $PSScriptRoot 'record.ps1'
$root = 'docs/specs'
if (Test-Path .specify/memory/workflow.json) { $c = Get-Content .specify/memory/workflow.json -Raw | ConvertFrom-Json; if ($c.PSObject.Properties['specRoot'] -and $c.specRoot) { $root = $c.specRoot.TrimEnd('/') } }
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
# The spec in the item's own folder (specs/<issue>-<name>/spec.md); else the newest spec under specs/.
$own = Get-ItemFolder '.' $id
$spec = if ($own -and (Test-Path (Join-Path $own 'spec.md'))) { Get-Item (Join-Path $own 'spec.md') } else { Get-ChildItem specs -Recurse -Filter spec.md -ErrorAction SilentlyContinue | Sort-Object LastWriteTime | Select-Object -Last 1 }
if (-not $spec) { Write-Error 'No spec.md under specs/.'; exit 1 }
$rel = [IO.Path]::GetRelativePath((Get-Location), $spec.FullName).Replace('\', '/')
& $rec -Section 'Spec' -Body "Working spec: ``$rel`` (deleted once reconciled into $root/)."
# The version Martin is shown at the review gate; spec-agreed.ps1 binds his approval to exactly this.
# A spec already presented and unchanged is not presented again, so an approval Martin has given still stands.
$recPath = Get-ItemRecordPath '.' $id
$recText = if (Test-Path $recPath) { Get-Content $recPath -Raw } else { '' }
$lastShown = [regex]::Matches("$recText", '(?m)^Spec presented:[ \t]*\S+[ \t]+sha256[ \t]+(?<h>[0-9a-f]{64})') | Select-Object -Last 1
$stands = -not (Get-SpecApprovalProblem ("$recText" -replace "`r`n", "`n") '.')
if (-not $stands -and -not ($lastShown -and (Test-TextHash $spec.FullName $lastShown.Groups['h'].Value))) { & $rec -Line "Spec presented: $rel sha256 $(Get-TextHash $spec.FullName)" }
foreach ($p in @(, @('Red team', '.specify/reviews/challenge.md'))) {
    if (-not (Test-Path $p[1])) { Write-Error "$($p[1]) was not written."; exit 1 }
    & $rec -Section $p[0] -Body (Get-Content $p[1] -Raw)
}
exit 0