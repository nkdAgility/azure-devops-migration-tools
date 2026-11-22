# After Martin approves at the review gate: the item must carry his own words, "Spec agreed: <his words>",
# written by the WKR from what he said. A fixed or empty line does not count. Fails the step otherwise.
# It then binds the approval to the spec he saw: "Spec version: <path> sha256 <hash>" of the working spec named in
# the item's Spec section. The Specified>Building check refuses if that file has changed since, so a revised spec
# needs his approval again.
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$wkr = Get-ItemRecordPath '.' $id
$text = if (Test-Path $wkr) { (Get-Content $wkr -Raw) -replace "`r`n", "`n" } else { '' }
# The same rule as the Specified>Building check (specAgreed): the last line, Martin's own words.
$words = Get-SpecAgreed $text
if (-not $words) {
    Write-Error "No 'Spec agreed:' line with Martin's own words on #$id. Record what he said, then resume the run."
    exit 1
}
# The approval binds only the version that was presented to Martin at the review gate ("Spec presented:", written
# when the spec is brought onto the item), and only when it was given after that presentation. An approval is bound
# once: a later change needs a new presentation and a new "Spec agreed:" line, never a rebind.
$agreed = [regex]::Matches($text, '(?m)^Spec agreed:') | Select-Object -Last 1
$shown = [regex]::Matches($text, '(?m)^Spec presented:[ \t]*(?<p>\S+)[ \t]+sha256[ \t]+(?<h>[0-9a-f]{64})[ \t]*$') | Select-Object -Last 1
if (-not $shown) { Write-Error "#$id records no 'Spec presented:' version, so the approval cannot be bound to what Martin saw."; exit 1 }
if ($agreed.Index -lt $shown.Index) { Write-Error "Martin's last 'Spec agreed:' line is older than the version presented to him; it needs his approval of the version presented."; exit 1 }
$path = $shown.Groups['p'].Value; $hash = $shown.Groups['h'].Value
if (-not (Test-Path $path)) { Write-Error "The working spec $path is missing."; exit 1 }
$now = Get-TextHash $path
if ($now -ne $hash) { Write-Error "The spec $path changed after it was presented to Martin; present it again and get his approval."; exit 1 }
$bound = [regex]::Matches($text, '(?m)^Spec version:[ \t]*(?<p>\S+)[ \t]+sha256[ \t]+(?<h>[0-9a-f]{64})[ \t]*$') | Where-Object { $_.Index -gt $agreed.Index } | Select-Object -Last 1
if ($bound) {
    if ($bound.Groups['h'].Value -ne $hash) { Write-Error "This approval is already bound to another version; it is never rebound. Get a new approval."; exit 1 }
    "spec agreed: $words (already bound to $path at $hash)"; exit 0
}
Add-Content $wkr "Spec version: $path sha256 $hash"
"spec agreed: $words (bound to $path at $hash)"
exit 0
