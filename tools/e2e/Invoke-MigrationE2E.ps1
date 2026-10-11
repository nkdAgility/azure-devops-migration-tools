<#
.SYNOPSIS
    Runs a devopsmigration.exe build end to end against the test organisation and reports the exit code.

.DESCRIPTION
    Uses the test PAT from src/MigrationTools/Tests/TestingConstants.cs (the same token the automated tests use)
    against https://dev.azure.com/nkdagility-preview. The token is passed through the tool's endpoint
    environment-variable overrides, is never written to disk, and is masked in the output.

    Build MigrationTools.ConsoleFull first. Long worktree paths can stop the exe launching, so the build output
    is copied to a short temp folder before running.

.EXAMPLE
    ./tools/e2e/Invoke-MigrationE2E.ps1 -Config ./my-scenario.json
#>
param(
    [Parameter(Mandatory)][string]$Config,
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$BuildOutput = (Join-Path $RepoRoot "src\MigrationTools.ConsoleFull\bin\Debug\net472"),
    [string]$Filter = "Saved as|Could not save|could not be migrated|Continuing|Too many|stopped|failed|Exiting"
)
$constants = Join-Path $RepoRoot "src\MigrationTools\Tests\TestingConstants.cs"
$pat = ([regex]::Match((Get-Content $constants -Raw), 'AccessTokenRaw = "([^"]+)"')).Groups[1].Value.Replace("fake", "")
if (-not $pat) { throw "Test PAT not found in $constants" }
if (-not (Test-Path (Join-Path $BuildOutput "devopsmigration.exe"))) { throw "Build MigrationTools.ConsoleFull first: $BuildOutput" }

$runDir = Join-Path ([IO.Path]::GetTempPath()) ("amt-e2e-" + [Guid]::NewGuid().ToString("N").Substring(0, 8))
Copy-Item $BuildOutput $runDir -Recurse
$configPath = (Resolve-Path $Config).Path

$env:MigrationTools__Endpoints__Source__Authentication__AccessToken = $pat
$env:MigrationTools__Endpoints__Target__Authentication__AccessToken = $pat
Push-Location $runDir
try {
    & .\devopsmigration.exe execute --config $configPath --skipVersionCheck --disableTelemetry 2>&1 |
        Select-String -Pattern $Filter | ForEach-Object { $_.Line.Replace($pat, "***") }
    "EXIT CODE: $LASTEXITCODE"
}
finally {
    Pop-Location
    Remove-Item Env:\MigrationTools__Endpoints__Source__Authentication__AccessToken, Env:\MigrationTools__Endpoints__Target__Authentication__AccessToken
    Remove-Item $runDir -Recurse -Force -ErrorAction SilentlyContinue
}
