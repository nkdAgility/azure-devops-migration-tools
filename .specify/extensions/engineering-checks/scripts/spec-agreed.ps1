# After Martin approves at the review gate: the item must carry his own words, "Spec agreed: <his words>",
# written by the WKR from what he said. A fixed or empty line does not count. Fails the step otherwise.
# It then binds the approval to the spec he saw: "Spec version: <path> sha256 <hash>" of the working spec named in
# the item's Spec section. The Specified>Building check refuses if that file has changed since, so a revised spec
# needs his approval again.
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
$spec = [regex]::Matches($text, '(?m)^Working spec: `([^`]+)`') | Select-Object -Last 1
if (-not $spec) { Write-Error "#$id names no working spec ('Working spec: ``<path>``'), so the approval cannot be bound to what Martin saw."; exit 1 }
$path = $spec.Groups[1].Value
if (-not (Test-Path $path)) { Write-Error "The working spec $path is missing."; exit 1 }
$hash = (Get-FileHash $path -Algorithm SHA256).Hash.ToLower()
Add-Content $wkr "Spec version: $path sha256 $hash"
"spec agreed: $words (bound to $path at $hash)"
exit 0
