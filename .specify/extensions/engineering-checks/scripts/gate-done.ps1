# Records one gate's evidence (its confidence file) on the item and moves it on through the policy.
param([Parameter(Mandatory)][ValidateSet('output','outcome')][string]$Gate)
$ErrorActionPreference = 'Stop'
$conf = ".specify/reviews/$Gate-confidence.md"
if (-not (Test-Path $conf)) { Write-Error "$conf was not written."; exit 1 }
$section = if ($Gate -eq 'output') { 'Output done evidence' } else { 'Outcome done' }
$body = Get-Content $conf -Raw
if ($Gate -eq 'outcome') {
    # The outcome checks must have passed on this commit: an older report cannot stand for this attempt.
    $report = '.specify/reviews/outcome-checks.md'
    if (-not (Test-Path $report)) { Write-Error "$report was not written: the outcome checks have not passed."; exit 1 }
    $r = Get-Content $report -Raw
    $m = [regex]::Match($r, '(?m)^Automated outcome checks \(all passed\) at ([0-9a-f]{40}):')
    $head = git rev-parse HEAD
    if (-not $m.Success -or $m.Groups[1].Value -ne $head) { Write-Error "$report is not for the current commit ($head); run the outcome checks again."; exit 1 }
    $body = $r + "`n" + $body
}
& (Join-Path $PSScriptRoot 'record.ps1') -Section $section -Body $body
$reason = if ($Gate -eq 'output') { 'Output Done: automated checks passed and manual validation scored' } else { 'Outcome Done: outcome checks passed and real-system validation scored' }
& (Join-Path $PSScriptRoot 'advance.ps1') -Reason $reason
exit $LASTEXITCODE