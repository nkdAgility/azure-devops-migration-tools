# Outcome Done: runs every outcome check the spec defined against the real system.
# Each check is a line in the item's 'Outcome checks' section:  - <what is checked>: `<command>`
# Writes .specify/reviews/outcome-checks.md only when every check passes, naming the item, the commit and the attempt.
# Every attempt, passed or failed, leaves a marker on the WKR, so a later failed attempt outranks an earlier pass.
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
Remove-Item .specify/reviews/outcome-checks.md -ErrorAction SilentlyContinue
$cfg = Get-WorkflowConfig
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$item = & (Get-BoardAdapter $cfg).GetItem $id
$sha = git rev-parse HEAD; $attempt = [guid]::NewGuid().ToString('N').Substring(0, 12)
$fail = { param($why) & (Join-Path $PSScriptRoot 'record.ps1') -Line "Outcome checks failed for #$id at $sha attempt ${attempt}: $why"; Write-Error $why; exit 1 }
$section = Get-Section (Get-ItemEvidence $item) 'Outcome checks'
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