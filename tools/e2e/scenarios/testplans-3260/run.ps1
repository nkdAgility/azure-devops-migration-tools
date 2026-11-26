#Requires -Version 7.0
# testplans-3260 end to end: seeds scratch projects amte2e-<Tag>-src/-tgt, runs the Plans and Suites guide's steps in the
# order below, checks each run's exit code and the target suites, and deletes the two projects at the end.
# Exits 0 only when every phase gives the expected result.
param(
    [string]$Tag = "3260r$(Get-Date -Format 'MMddHHmm')",
    [string]$BuildOutput,
    [string]$WorkDir = (Join-Path ([IO.Path]::GetTempPath()) "amte2e-testplans-$Tag"),
    [switch]$Keep
)
$ErrorActionPreference = "Stop"
$repo = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..\..")).Path
if (-not $BuildOutput) { $BuildOutput = Join-Path $repo "src\MigrationTools.ConsoleFull\bin\Debug\net472" }
$src = "amte2e-$Tag-src"; $tgt = "amte2e-$Tag-tgt"
New-Item -ItemType Directory -Force $WorkDir | Out-Null
$cfg = @{}
foreach ($f in 'step2-testcases', 'step3-variables-configurations', 'step4-plans-suites') {
    $cfg[$f] = Join-Path $WorkDir "$f.json"
    (Get-Content (Join-Path $PSScriptRoot "$f.json") -Raw).Replace('amte2e-3260-src', $src).Replace('amte2e-3260-tgt', $tgt) | Set-Content $cfg[$f]
}
$failures = [Collections.Generic.List[string]]::new()
$n = 0
function Invoke-Step([string]$Config, [int]$Expect, [string]$MustLog) {
    $script:n++
    $out = & (Join-Path $repo "tools/e2e/Invoke-MigrationE2E.ps1") -Config $cfg[$Config] -BuildOutput $BuildOutput -LogDirectory (Join-Path $WorkDir "$($script:n)-$Config") 2>&1
    $code = $LASTEXITCODE
    "run $($script:n) $Config exit $code (expected $Expect)"
    if ($code -ne $Expect) { $failures.Add("run $($script:n) $Config exited $code, expected $Expect") }
    if ($MustLog) {
        $logged = Get-ChildItem (Join-Path $WorkDir "$($script:n)-$Config") -Recurse -File -Filter *.log | Select-String -SimpleMatch $MustLog -Quiet
        if (-not $logged -and -not ($out -match [regex]::Escape($MustLog))) { $failures.Add("run $($script:n) $Config did not log '$MustLog'") }
    }
}
function Test-Suites([string]$Phase, [string[]]$Root, [string[]]$SuiteA) {
    & (Join-Path $PSScriptRoot "assert.ps1") -Root $Root -SuiteA $SuiteA -Tgt $tgt
    if ($LASTEXITCODE -ne 0) { $failures.Add("$Phase`: target suites do not hold the expected Test Cases") }
}
$tc1 = 'tc1 root and suite'; $tc2 = 'tc2 root and suite'; $tc3 = 'tc3 root only'
$seed = Join-Path $PSScriptRoot "seed.ps1"
try {
    "== 1. first migration"
    & $seed -Step 1 -Src $src -Tgt $tgt -StateDir $WorkDir
    Invoke-Step step2-testcases 0; Invoke-Step step3-variables-configurations 0; Invoke-Step step4-plans-suites 0
    Test-Suites "1. first migration" @($tc1) @($tc1)
    "== 2. TC2 added to the root suite and Suite A, re-run"
    & $seed -Step 2 -Src $src -Tgt $tgt -StateDir $WorkDir
    Invoke-Step step2-testcases 0; Invoke-Step step4-plans-suites 0
    Test-Suites "2. re-run" @($tc1, $tc2) @($tc1, $tc2)
    "== 3. re-run with nothing new"
    Invoke-Step step4-plans-suites 0
    Test-Suites "3. no duplicates" @($tc1, $tc2) @($tc1, $tc2)
    "== 4. TC3 in the root suite, not migrated yet"
    & $seed -Step 3 -Src $src -Tgt $tgt -StateDir $WorkDir
    Invoke-Step step4-plans-suites 1 "'tc3 root only' was not added to suite 'amte2e 3260 plan'"
    "== 5. TC3 migrated, re-run"
    Invoke-Step step2-testcases 0; Invoke-Step step4-plans-suites 0
    Test-Suites "5. after migrating TC3" @($tc1, $tc2, $tc3) @($tc1, $tc2)
}
catch { $failures.Add("the scenario stopped: $_") }
finally {
    if (-not $Keep) {
        Import-Module (Join-Path $repo "tools/e2e/E2EAzDo.psm1") -Force
        foreach ($p in @(@{ Org = 'nkdagility-adomt-source'; Name = $src }, @{ Org = 'nkdagility-adomt-target'; Name = $tgt })) {
            Set-E2EOrg $p.Org
            if (@(Get-E2EProjects -Org $p.Org -Prefix $p.Name) | Where-Object name -eq $p.Name) { Remove-E2EProject $p.Name -Org $p.Org; "gave back pool project $($p.Org)/$($p.Name)" }
        }
    }
}
if ($failures.Count) { $failures | ForEach-Object { "FAIL: $_" }; exit 1 }
"PASS: testplans-3260"
exit 0
