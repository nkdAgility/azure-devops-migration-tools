<#
.SYNOPSIS
    Runs a devopsmigration.exe build end to end against the test organisation and exits with the tool's exit code.

.DESCRIPTION
    Reads the test PAT from the AMT_TEST_PAT environment variable. While the token still lives in
    src/MigrationTools/Tests/TestingConstants.cs (see #3193), it falls back to that file.

    Every endpoint in the scenario config must point at https://dev.azure.com/nkdagility-preview; the script refuses
    to run otherwise, so the test PAT is never sent anywhere else. The token is injected through the tool's
    endpoint environment-variable overrides for every endpoint in the config (any name, TFS or REST), is never
    written to disk, and is masked in the output. Any existing values of those variables are restored afterwards.
    REST (AzureDevOpsEndpoint) endpoints must still carry a placeholder AccessToken in the config, because the
    schema requires it; the override replaces it at run time.

    Works with Windows PowerShell 5.1 and PowerShell 7+.

    Build MigrationTools.ConsoleFull first. Long worktree paths can stop the exe launching, so the build output
    is copied to a short temp folder before running.

.EXAMPLE
    ./tools/e2e/Invoke-MigrationE2E.ps1 -Config ./my-scenario.json
#>
param(
    [Parameter(Mandatory)][string]$Config,
    [string]$RepoRoot,
    [string]$BuildOutput,
    [string]$Filter = "Saved as|Could not save|could not be migrated|Continuing|Too many|stopped|failed|Exiting"
)
$ErrorActionPreference = "Stop"
# Resolved here rather than as parameter defaults: Windows PowerShell 5.1 leaves $PSScriptRoot empty in the param block.
if (-not $RepoRoot) { $RepoRoot = (Resolve-Path (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..")).Path }
if (-not $BuildOutput) { $BuildOutput = Join-Path $RepoRoot "src\MigrationTools.ConsoleFull\bin\Debug\net472" }
$AllowedOrganisation = "https://dev.azure.com/nkdagility-preview"

# Validate everything before touching the environment or the disk.
$configPath = (Resolve-Path $Config).Path
if (-not (Test-Path (Join-Path $BuildOutput "devopsmigration.exe"))) { throw "Build MigrationTools.ConsoleFull first: $BuildOutput" }

$endpoints = (Get-Content $configPath -Raw | ConvertFrom-Json).MigrationTools.Endpoints
if (-not $endpoints) { throw "No MigrationTools.Endpoints found in $configPath" }
$endpointNames = @($endpoints.PSObject.Properties.Name)
foreach ($name in $endpointNames) {
    $endpoint = $endpoints.$name
    $url = if ($endpoint.Collection) { $endpoint.Collection } else { $endpoint.Organisation }
    if (-not $url -or $url.TrimEnd('/') -ne $AllowedOrganisation) {
        throw "Endpoint '$name' points at '$url'. The harness only runs against $AllowedOrganisation."
    }
}

$pat = $env:AMT_TEST_PAT
if (-not $pat) {
    $constants = Join-Path $RepoRoot "src\MigrationTools\Tests\TestingConstants.cs"
    if (Test-Path $constants) {
        $pat = ([regex]::Match((Get-Content $constants -Raw), 'AccessTokenRaw = "([^"]+)"')).Groups[1].Value.Replace("fake", "")
    }
}
if (-not $pat) { throw "Test PAT not found. Set AMT_TEST_PAT." }

# TFS endpoints read Authentication:AccessToken; REST endpoints read AccessToken.
$varNames = foreach ($name in $endpointNames) {
    "MigrationTools__Endpoints__${name}__Authentication__AccessToken"
    "MigrationTools__Endpoints__${name}__AccessToken"
}
$saved = @{}
foreach ($v in $varNames) { $saved[$v] = [Environment]::GetEnvironmentVariable($v) }

$runDir = Join-Path ([IO.Path]::GetTempPath()) ("amt-e2e-" + [Guid]::NewGuid().ToString("N").Substring(0, 8))
$pushed = $false
$exitCode = 1
try {
    Copy-Item $BuildOutput $runDir -Recurse
    foreach ($v in $varNames) { [Environment]::SetEnvironmentVariable($v, $pat) }
    Push-Location $runDir
    $pushed = $true
    & .\devopsmigration.exe execute --config $configPath --skipVersionCheck --disableTelemetry 2>&1 |
        Select-String -Pattern $Filter | ForEach-Object { $_.Line.Replace($pat, "***") }
    $exitCode = $LASTEXITCODE
    "EXIT CODE: $exitCode"
}
finally {
    if ($pushed) { Pop-Location }
    foreach ($v in $varNames) { [Environment]::SetEnvironmentVariable($v, $saved[$v]) }
    Remove-Item $runDir -Recurse -Force -ErrorAction SilentlyContinue
}
exit $exitCode
