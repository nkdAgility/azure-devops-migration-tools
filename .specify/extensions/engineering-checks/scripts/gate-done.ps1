# Records one gate's evidence (its confidence file) on the item and moves it on through the policy.
param([Parameter(Mandatory)][ValidateSet('output','outcome')][string]$Gate)
$ErrorActionPreference = 'Stop'
$conf = ".specify/reviews/$Gate-confidence.md"
if (-not (Test-Path $conf)) { Write-Error "$conf was not written."; exit 1 }
$section = if ($Gate -eq 'output') { 'Output done evidence' } else { 'Outcome done' }
$body = Get-Content $conf -Raw
if ($Gate -eq 'outcome') {
    # The outcome checks must have passed for this item, on this commit, in the latest attempt.
    Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
    $report = '.specify/reviews/outcome-checks.md'
    if (-not (Test-Path $report)) { Write-Error "$report was not written: the outcome checks have not passed."; exit 1 }
    $r = (Get-Content $report -Raw) -replace "`r`n", "`n"
    $id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
    $wkr = ".specify/wkr/$id.md"
    $history = if (Test-Path $wkr) { (Get-Content $wkr -Raw) -replace "`r`n", "`n" } else { '' }
    $problem = Get-OutcomeProblem ($history + "`n" + $r) $id (git rev-parse HEAD)
    if ($problem) { Write-Error "$problem; run the outcome checks again."; exit 1 }
    $body = $r + "`n" + $body
}
& (Join-Path $PSScriptRoot 'record.ps1') -Section $section -Body $body
$reason = if ($Gate -eq 'output') { 'Output Done: automated checks passed and manual validation scored' } else { 'Outcome Done: outcome checks passed and real-system validation scored' }
& (Join-Path $PSScriptRoot 'advance.ps1') -Reason $reason
exit $LASTEXITCODE