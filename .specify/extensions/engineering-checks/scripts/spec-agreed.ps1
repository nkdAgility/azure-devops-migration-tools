# After Martin approves at the review gate: the item must carry his own words, "Spec agreed: <his words>",
# written by the WKR from what he said. A fixed or empty line does not count. Fails the step otherwise.
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$wkr = ".specify/wkr/$id.md"
$text = if (Test-Path $wkr) { (Get-Content $wkr -Raw) -replace "`r`n", "`n" } else { '' }
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
# The same rule as the Specified>Building check (specAgreed): the last line, Martin's own words.
$words = Get-SpecAgreed $text
if (-not $words) {
    Write-Error "No 'Spec agreed:' line with Martin's own words on #$id. Record what he said, then resume the run."
    exit 1
}
"spec agreed: $words"
exit 0