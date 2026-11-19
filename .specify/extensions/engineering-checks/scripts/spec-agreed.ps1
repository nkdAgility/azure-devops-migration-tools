# After Martin approves at the review gate: the item must carry his own words, "Spec agreed: <his words>",
# written by the WKR from what he said. A fixed or empty line does not count. Fails the step otherwise.
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$wkr = ".specify/wkr/$id.md"
$text = if (Test-Path $wkr) { Get-Content $wkr -Raw } else { '' }
$m = [regex]::Matches($text, '(?m)^Spec agreed:[ \t]*(.+)$') | Select-Object -Last 1
if (-not $m -or $m.Groups[1].Value.Trim().Length -lt 3 -or $m.Groups[1].Value -match '^approved by Martin at the review gate') {
    Write-Error "No 'Spec agreed:' line with Martin's own words on #$id. Record what he said, then resume the run."
    exit 1
}
"spec agreed: $($m.Groups[1].Value.Trim())"
exit 0