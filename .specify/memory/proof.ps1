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
    # Every named resource the seed takes (projects, and processes such as Process or NoBugProcess) gets a scratch
    # name of this run, so nothing shared is seeded or migrated into. The config is re-pointed at the same names,
    # longest value first so that "x" is not replaced inside "x-src". Each script gets only the parameters it declares.
    $defaults = [ordered]@{}
    foreach ($m in [regex]::Matches((Get-Content $seed -Raw), '\[string\]\$(\w+)\s*=\s*"([^"]+)"')) { $defaults[$m.Groups[1].Value] = $m.Groups[2].Value }
    if (-not ($defaults.Contains('Src') -and $defaults.Contains('Tgt'))) { $notCovered += $d.Name; continue }
    $names = [ordered]@{}
    foreach ($k in $defaults.Keys) { $names[$k] = "amte2e-p$RunId-$($d.Name)-$($k.ToLower())" }
    $scratch += $names.Values
    "== $($d.Name): $(($names.Keys | ForEach-Object { "$_=$($names[$_])" }) -join ', ')"
    $cfgText = Get-Content $config -Raw
    foreach ($k in ($defaults.Keys | Sort-Object { $defaults[$_].Length } -Descending)) { $cfgText = $cfgText.Replace($defaults[$k], $names[$k]) }
    $cfgFile = Join-Path $work "$($d.Name).json"
    $cfgText | Set-Content $cfgFile -Encoding utf8
    $argsFor = { param($script, [hashtable]$extra = @{})
        $declared = @([regex]::Matches((Get-Content $script -Raw), '(?m)\[\w+\]\$(\w+)') | ForEach-Object { $_.Groups[1].Value })
        $a = @(); foreach ($k in $names.Keys) { if ($declared -contains $k) { $a += "-$k", $names[$k] } }
        foreach ($k in $extra.Keys) { if ($declared -contains $k) { $a += "-$k", $extra[$k] } }
        , $a }
    $seedArgs = & $argsFor $seed
    pwsh -NoProfile -File $seed @seedArgs
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (seed)"; continue }
    pwsh -NoProfile -File tools/e2e/Invoke-MigrationE2E.ps1 -Config $cfgFile
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (migration)"; continue }
    $assertArgs = & $argsFor $assert @{ OutFile = (Join-Path $work "$($d.Name).assert.md") }
    pwsh -NoProfile -File $assert @assertArgs
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (assertions)" }
}
"not covered (no seed, config and assert with scratch-project parameters): $($notCovered -join ', ')"
"scratch projects and processes left for clean-up: $($scratch -join ', ')"
if (-not $scratch) { Write-Error "no scenario was run: the proof covers nothing"; exit 1 }
if ($failed) { Write-Error "end-to-end proof failed: $($failed -join ', ')"; exit 1 }
"all covered scenarios passed; results in $work"
exit 0
