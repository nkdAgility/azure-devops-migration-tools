# Runs sharedsteps-3256 end to end and cleans up: seed, migrate the Test Case only (references must be reported),
# re-run with all three types (references must be remapped), then delete the scratch projects.
# Exits 0 only when both assertions pass. Build MigrationTools.ConsoleFull first.
param([string]$Src = "amte2e-3256-src", [string]$Tgt = "amte2e-3256-tgt", [string]$BuildOutput)
$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..\..\..")
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$invoke = Join-Path $root "tools\e2e\Invoke-MigrationE2E.ps1"
$buildArgs = if ($BuildOutput) { @{ BuildOutput = $BuildOutput } } else { @{} }
$work = Join-Path ([IO.Path]::GetTempPath()) ("amt-3256-" + [guid]::NewGuid().ToString("N").Substring(0, 8))
New-Item -ItemType Directory -Force $work | Out-Null
$failed = @()
$seeded = $false
$passed = $false
try {
    & "$PSScriptRoot\seed.ps1" -Src $Src -Tgt $Tgt
    $seeded = $true
    foreach ($step in @(@{ Config = "config.json"; Expect = "Unresolved" }, @{ Config = "config-all.json"; Expect = "Resolved" })) {
        $cfg = Join-Path $work $step.Config
        (Get-Content (Join-Path $PSScriptRoot $step.Config) -Raw).Replace("amte2e-3256-src", $Src).Replace("amte2e-3256-tgt", $Tgt) | Set-Content $cfg -Encoding utf8
        $logs = Join-Path $work $step.Expect
        & $invoke -Config $cfg -LogDirectory $logs @buildArgs
        if ($LASTEXITCODE -ne 0) { $failed += "$($step.Config) migration exit $LASTEXITCODE"; continue }
        & "$PSScriptRoot\assert.ps1" -Src $Src -Tgt $Tgt -LogDirectory $logs -Expect $step.Expect
        if ($LASTEXITCODE -ne 0) { $failed += "$($step.Expect) assertions exit $LASTEXITCODE" }
    }
    $passed = -not $failed
}
finally {
    # Keep the migration logs when anything failed, so the failure can be read without another run. This runs
    # first, so the path is printed even if a project deletion below throws.
    if ($passed) { Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue }
    else { Write-Warning "sharedsteps-3256 logs kept in $work" }
    Remove-Item (Join-Path $PSScriptRoot "state.local.json") -ErrorAction SilentlyContinue
    # The seed creates the target project first; the source is ours to delete only once the seed has finished.
    if ($seeded) { Remove-E2EProject $Src -Org nkdagility-adomt-source }
    Remove-E2EProject $Tgt -Org nkdagility-adomt-target
}
if ($failed) { Write-Error "sharedsteps-3256 failed: $($failed -join '; ')"; exit 1 }
"sharedsteps-3256 passed"
exit 0
