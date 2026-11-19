# The real-system proof for Output Done, on the test organisations (tools/e2e; see "Validating Changes End to
# End" in platform/specs/architecture.md). Needs `az login` (or AMT_TEST_PAT) for the test orgs.
#
# Every scenario that has a seed, a config and an assert is run in scratch projects of its own, never in the
# projects the automated tests use: seed the scratch projects, run the migration with the config re-pointed at
# them, then run the scenario's assertions. Fails if any seed, migration or assertion fails.
# Scenarios without an assert are listed as not covered. Scratch projects are listed at the end and are not
# deleted here: Remove-E2EObjects.ps1 with a per-run prefix also deletes other scenarios' fields (#3278).
param([string]$RunId = (Get-Date -Format 'MMddHHmm'))
$ErrorActionPreference = 'Stop'
$failed = @(); $notCovered = @(); $scratch = @()
$work = Join-Path ([IO.Path]::GetTempPath()) "amtproof-$RunId"
New-Item -ItemType Directory -Force $work | Out-Null
foreach ($d in Get-ChildItem tools/e2e/scenarios -Directory) {
    $seed = Join-Path $d.FullName 'seed.ps1'; $assert = Join-Path $d.FullName 'assert.ps1'; $config = Join-Path $d.FullName 'config.json'
    if (-not ((Test-Path $seed) -and (Test-Path $assert) -and (Test-Path $config))) { $notCovered += $d.Name; continue }
    $seedText = Get-Content $seed -Raw
    $src = [regex]::Match($seedText, '\[string\]\$Src\s*=\s*"([^"]+)"').Groups[1].Value
    $tgt = [regex]::Match($seedText, '\[string\]\$Tgt\s*=\s*"([^"]+)"').Groups[1].Value
    if (-not $src -or -not $tgt) { $notCovered += $d.Name; continue }
    $sSrc = "amte2e-p$RunId-$($d.Name)-src"; $sTgt = "amte2e-p$RunId-$($d.Name)-tgt"
    $scratch += $sSrc, $sTgt
    "== $($d.Name): $sSrc -> $sTgt"
    $cfgFile = Join-Path $work "$($d.Name).json"
    (Get-Content $config -Raw).Replace($src, $sSrc).Replace($tgt, $sTgt) | Set-Content $cfgFile -Encoding utf8
    pwsh -NoProfile -File $seed -Src $sSrc -Tgt $sTgt
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (seed)"; continue }
    pwsh -NoProfile -File tools/e2e/Invoke-MigrationE2E.ps1 -Config $cfgFile
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (migration)"; continue }
    pwsh -NoProfile -File $assert -Src $sSrc -Tgt $sTgt -OutFile (Join-Path $work "$($d.Name).assert.md")
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (assertions)" }
}
"not covered (no seed, config and assert with scratch-project parameters): $($notCovered -join ', ')"
"scratch projects left for clean-up: $($scratch -join ', ')"
if ($failed) { Write-Error "end-to-end proof failed: $($failed -join ', ')"; exit 1 }
"all covered scenarios passed; results in $work"
exit 0
