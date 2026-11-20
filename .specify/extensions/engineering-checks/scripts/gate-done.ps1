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
    if (Test-Path .specify/reviews/outcome-attempt-pending) { Write-Error "an outcome check attempt did not record its start; run the outcome checks again."; exit 1 }
    $r = (Get-Content $report -Raw) -replace "`r`n", "`n"
    $id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
    $wkr = ".specify/wkr/$id.md"
    $history = if (Test-Path $wkr) { (Get-Content $wkr -Raw) -replace "`r`n", "`n" } else { '' }
    $head = git rev-parse HEAD
    # The report itself must be a pass for this item at this commit (an earlier pass on the WKR cannot stand in
    # for a report about another item), and it must close the latest attempt recorded on the WKR.
    $own = [regex]::Match($r, '(?m)^Automated outcome checks \(all passed\) for #(\d+) at ([0-9a-f]{40}) attempt [0-9a-f]+:')
    $problem = if (-not $own.Success) { "$report is not a pass report" }
        elseif ($own.Groups[1].Value -ne "$id") { "$report is for #$($own.Groups[1].Value), not #$id" }
        elseif ($own.Groups[2].Value -ne $head) { "$report is for $($own.Groups[2].Value), not the current commit $head" }
        else { Get-OutcomeProblem ($history + "`n" + $r) $id $head }
    if ($problem) { Write-Error "$problem; run the outcome checks again."; exit 1 }
    $body = $r + "`n" + $body
}
& (Join-Path $PSScriptRoot 'record.ps1') -Section $section -Body $body
$reason = if ($Gate -eq 'output') { 'Output Done: automated checks passed and manual validation scored' } else { 'Outcome Done: outcome checks passed and real-system validation scored' }
& (Join-Path $PSScriptRoot 'advance.ps1') -Reason $reason
exit $LASTEXITCODE