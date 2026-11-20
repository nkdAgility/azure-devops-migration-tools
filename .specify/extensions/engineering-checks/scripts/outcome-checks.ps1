# Outcome Done: runs every outcome check the spec defined against the real system.
# Each check is a line in the item's 'Outcome checks' section:  - <what is checked>: `<command>`
# Every attempt is recorded on the WKR before anything that can fail ("Outcome checks started for #<id> at <sha>
# attempt <a>:"), and ends with a failed marker or a report. A started attempt with no pass is incomplete and
# outranks any earlier pass. Writes .specify/reviews/outcome-checks.md only when every check passes, naming the
# item, the commit and the attempt.
$ErrorActionPreference = 'Stop'
Remove-Item .specify/reviews/outcome-checks.md -ErrorAction SilentlyContinue
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$sha = git rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or -not $sha) { Write-Error 'git rev-parse HEAD failed; the commit is not known.'; exit 1 }
$attempt = [guid]::NewGuid().ToString('N').Substring(0, 12)
$wkr = ".specify/wkr/$id.md"
New-Item -ItemType Directory -Force .specify/wkr | Out-Null
Add-Content $wkr "Outcome checks started for #$id at $sha attempt ${attempt}:"
$fail = { param($why) Add-Content $wkr "Outcome checks failed for #$id at $sha attempt ${attempt}: $why"; Write-Error $why -ErrorAction Continue; exit 1 }
try {
    Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
    $cfg = Get-WorkflowConfig
    $item = & (Get-BoardAdapter $cfg).GetItem $id
    $section = Get-Section (Get-ItemEvidence $item) 'Outcome checks'
} catch { & $fail "the item could not be read: $_" }
if (-not $section) { & $fail "#$id has no 'Outcome checks' section." }
$checks = [regex]::Matches($section, '(?m)^\s*-\s*(.+?):\s*`([^`]+)`')
if ($checks.Count -eq 0) { & $fail "'Outcome checks' lists no runnable check (- <what>: ``command``)." }
$malformed = @($section -split "`n" | Where-Object { $_ -match '^\s*-\s' -and $_ -notmatch '^\s*-\s*(.+?):\s*`([^`]+)`' })
if ($malformed) { & $fail "'Outcome checks' has entries that are not runnable checks (- <what>: ``command``): $($malformed.Trim() -join ' | ')" }
$results = foreach ($c in $checks) {
    $name = $c.Groups[1].Value; $cmd = $c.Groups[2].Value
    pwsh -NoProfile -Command $cmd | Out-Host
    $code = $LASTEXITCODE
    [pscustomobject]@{ name = $name; command = $cmd; exit = $code }
}
$failed = @($results | Where-Object exit -ne 0)
$body = ($results | ForEach-Object { "- $($_.name): ``$($_.command)`` exit $($_.exit)" }) -join "`n"
if ($failed) { & $fail "Outcome checks failed: $($failed.name -join ', ')" }
New-Item -ItemType Directory -Force .specify/reviews | Out-Null
"Automated outcome checks (all passed) for #$id at $sha attempt ${attempt}:`n$body" | Set-Content .specify/reviews/outcome-checks.md -Encoding utf8
exit 0
