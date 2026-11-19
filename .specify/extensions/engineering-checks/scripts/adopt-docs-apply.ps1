# Adoption step 2, after Martin approves the plan: runs each 'git mv' line in .specify/adopt/docs-plan.md
# (so history is kept) and records the systems list. Commits nothing.
$ErrorActionPreference = 'Stop'
$plan = '.specify/adopt/docs-plan.md'
if (-not (Test-Path $plan)) { Write-Error "$plan was not written."; exit 1 }
$t = (Get-Content $plan -Raw) -replace "`r`n", "`n"
$moves = [regex]::Matches($t, '(?m)^\s*git mv\s+(\S+)\s+(\S+)\s*$')
foreach ($mv in $moves) {
    $from = $mv.Groups[1].Value; $to = $mv.Groups[2].Value
    if (-not (Test-Path $from)) { if (Test-Path $to) { "already moved: $to"; continue }; Write-Error "not found: $from"; exit 1 }
    New-Item -ItemType Directory -Force (Split-Path $to) | Out-Null
    git mv $from $to; if ($LASTEXITCODE) { exit 1 }
    "moved $from -> $to"
}
$sys = [regex]::Match($t, '(?ms)^##\s*Systems\s*\n(.*?)(?=^##\s|\z)').Groups[1].Value.Trim()
New-Item -ItemType Directory -Force docs/specs | Out-Null
"# Systems`n`nEach system and the source paths that belong to it. The docs check uses this to find which ``docs/specs/{system}/`` files a change must update.`n`n$sys`n" | Set-Content docs/specs/systems.md -Encoding utf8
"$($moves.Count) moves; systems written to docs/specs/systems.md"
exit 0