# Records one gate's evidence (its confidence file) on the item and moves it on through the policy.
param([Parameter(Mandatory)][ValidateSet('output','outcome')][string]$Gate)
$ErrorActionPreference = 'Stop'
$conf = ".specify/reviews/$Gate-confidence.md"
if (-not (Test-Path $conf)) { Write-Error "$conf was not written."; exit 1 }
$section = if ($Gate -eq 'output') { 'Output done evidence' } else { 'Outcome done' }
$body = Get-Content $conf -Raw
if ($Gate -eq 'outcome' -and (Test-Path .specify/reviews/outcome-checks.md)) { $body = (Get-Content .specify/reviews/outcome-checks.md -Raw) + "`n" + $body }
& (Join-Path $PSScriptRoot 'record.ps1') -Section $section -Body $body
$reason = if ($Gate -eq 'output') { 'Output Done: automated checks passed and manual validation scored' } else { 'Outcome Done: outcome checks passed and real-system validation scored' }
& (Join-Path $PSScriptRoot 'advance.ps1') -Reason $reason
exit $LASTEXITCODE