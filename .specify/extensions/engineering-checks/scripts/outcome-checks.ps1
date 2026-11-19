# Outcome Done: runs every outcome check the spec defined against the real system.
# Each check is a line in the item's 'Outcome checks' section:  - <what is checked>: `<command>`
# Writes .specify/reviews/outcome-checks.md only when every check passes.
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
Remove-Item .specify/reviews/outcome-checks.md -ErrorAction SilentlyContinue
$cfg = Get-WorkflowConfig
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$item = & (Get-BoardAdapter $cfg).GetItem $id
$section = Get-Section (Get-ItemEvidence $item) 'Outcome checks'
if (-not $section) { Write-Error "#$id has no 'Outcome checks' section."; exit 1 }
$checks = [regex]::Matches($section, '(?m)^\s*-\s*(.+?):\s*`([^`]+)`')
if ($checks.Count -eq 0) { Write-Error "'Outcome checks' lists no runnable check (- <what>: ``command``)."; exit 1 }
$malformed = @($section -split "`n" | Where-Object { $_ -match '^\s*-\s' -and $_ -notmatch '^\s*-\s*(.+?):\s*`([^`]+)`' })
if ($malformed) { Write-Error "'Outcome checks' has entries that are not runnable checks (- <what>: ``command``): $($malformed.Trim() -join ' | ')"; exit 1 }
$results = foreach ($c in $checks) {
    $name = $c.Groups[1].Value; $cmd = $c.Groups[2].Value
    pwsh -NoProfile -Command $cmd | Out-Host
    $code = $LASTEXITCODE
    [pscustomobject]@{ name = $name; command = $cmd; exit = $code }
}
$failed = @($results | Where-Object exit -ne 0)
$body = ($results | ForEach-Object { "- $($_.name): ``$($_.command)`` exit $($_.exit)" }) -join "`n"
if ($failed) { Write-Error "Outcome checks failed: $($failed.name -join ', ')"; exit 1 }
New-Item -ItemType Directory -Force .specify/reviews | Out-Null
"Automated outcome checks (all passed):`n$body" | Set-Content .specify/reviews/outcome-checks.md -Encoding utf8
exit 0