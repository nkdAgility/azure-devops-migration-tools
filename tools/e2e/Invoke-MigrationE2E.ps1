<#
.SYNOPSIS
    Runs a devopsmigration.exe build end to end against the test organisation and exits with the tool's exit code.

.DESCRIPTION
    Authentication (see #3193):
      - Default: Microsoft Entra ID. Every endpoint is switched to AuthenticationMode EntraId through the tool's
        endpoint environment-variable overrides, so the run uses your 'az login' (or another ambient identity).
        This script never handles a token.
      - If AMT_TEST_PAT is set, that PAT is used instead. It is injected through the same overrides, never written
        to disk, and masked in the output.
    Any existing values of the override variables are restored afterwards.

    Every endpoint in the scenario config must point at one of the test organisations; the script refuses to run
    otherwise, so credentials are never sent anywhere else. Pass -AllowedOrganisation to use your own orgs.

    Works with Windows PowerShell 5.1 and PowerShell 7+.

    Build MigrationTools.ConsoleFull first. Long worktree paths can stop the exe launching, so the build output
    is copied to a short temp folder before running. The tool is run from -LogDirectory, so its logs are written
    there and kept after the copied build is removed; the path is printed as LOGS.

.EXAMPLE
    ./tools/e2e/Invoke-MigrationE2E.ps1 -Config ./my-scenario.json
#>
param(
    [Parameter(Mandatory)][string]$Config,
    [string]$RepoRoot,
    [string]$BuildOutput,
    # Working directory for the run; the tool writes logs\<timestamp> here. Defaults to a new temp folder that is kept.
    [string]$LogDirectory,
    [string]$Filter ="Saved as|Could not save|could not be migrated|Continuing|Too many|stopped|failed|Exiting",
    [string[]]$AllowedOrganisation = @(
        "https://dev.azure.com/nkdagility-adomt-source",
        "https://dev.azure.com/nkdagility-adomt-target",
        "https://dev.azure.com/nkdagility-preview")
)
$ErrorActionPreference = "Stop"
# Resolved here rather than as parameter defaults: Windows PowerShell 5.1 leaves $PSScriptRoot empty in the param block.
if (-not $RepoRoot) { $RepoRoot = (Resolve-Path (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..")).Path }
if (-not $BuildOutput) { $BuildOutput = Join-Path $RepoRoot "src\MigrationTools.ConsoleFull\bin\Debug\net472" }
$AllowedOrganisation = @($AllowedOrganisation | ForEach-Object { $_.TrimEnd('/') })

# Validate everything before touching the environment or the disk.
$configPath = (Resolve-Path $Config).Path
if (-not (Test-Path (Join-Path $BuildOutput "devopsmigration.exe"))) { throw "Build MigrationTools.ConsoleFull first: $BuildOutput" }

$endpoints = (Get-Content $configPath -Raw | ConvertFrom-Json).MigrationTools.Endpoints
if (-not $endpoints) { throw "No MigrationTools.Endpoints found in $configPath" }
$endpointNames = @($endpoints.PSObject.Properties.Name)
foreach ($name in $endpointNames) {
    $endpoint = $endpoints.$name
    $url = if ($endpoint.Collection) { $endpoint.Collection } else { $endpoint.Organisation }
    if (-not $url -or $AllowedOrganisation -notcontains $url.TrimEnd('/')) {
        throw "Endpoint '$name' points at '$url'. The harness only runs against: $($AllowedOrganisation -join ', ')."
    }
}

$pat = $env:AMT_TEST_PAT
# TFS endpoints read Authentication:*; REST endpoints read the same keys at the endpoint root.
$overrides = [ordered]@{}
foreach ($name in $endpointNames) {
    $mode = if ($pat) { "AccessToken" } else { "EntraId" }
    $overrides["MigrationTools__Endpoints__${name}__Authentication__AuthenticationMode"] = $mode
    $overrides["MigrationTools__Endpoints__${name}__AuthenticationMode"] = $mode
    if ($pat) {
        $overrides["MigrationTools__Endpoints__${name}__Authentication__AccessToken"] = $pat
        $overrides["MigrationTools__Endpoints__${name}__AccessToken"] = $pat
    }
}
Write-Host ("Authentication: " + $(if ($pat) { "PAT from AMT_TEST_PAT" } else { "Microsoft Entra ID (ambient login)" }))
$varNames = @($overrides.Keys)
$saved = @{}
foreach ($v in $varNames) { $saved[$v] = [Environment]::GetEnvironmentVariable($v) }

$runId = [Guid]::NewGuid().ToString("N").Substring(0, 8)
$runDir = Join-Path ([IO.Path]::GetTempPath()) ("amt-e2e-" + $runId)
# The tool writes logs to the current working directory (#3216), so run it from a separate folder that is kept.
if (-not $LogDirectory) { $LogDirectory = Join-Path ([IO.Path]::GetTempPath()) ("amt-e2e-logs-" + $runId) }
$pushed = $false
$exitCode = 1
try {
    Copy-Item $BuildOutput $runDir -Recurse
    New-Item -ItemType Directory -Force $LogDirectory | Out-Null
    foreach ($v in $varNames) { [Environment]::SetEnvironmentVariable($v, $overrides[$v]) }
    Push-Location $LogDirectory
    $pushed = $true
    $logPath = $null
    & (Join-Path $runDir "devopsmigration.exe") execute --config $configPath --skipVersionCheck --disableTelemetry 2>&1 |
        ForEach-Object {
            $line = "$_"
            # Report the path the tool resolved, which may be the LocalAppData fallback.
            if ($line -match 'Logpath: (.+?)\s*$') { $logPath = $Matches[1] }
            if ($line -match $Filter) { if ($pat) { $line.Replace($pat, "***") } else { $line } }
        }
    $exitCode = $LASTEXITCODE
    if ($logPath) { "LOGS: $logPath" } else { Write-Warning "The tool did not report a Logpath line." }
    "EXIT CODE: $exitCode"
}
finally {
    if ($pushed) { Pop-Location }
    foreach ($v in $varNames) { [Environment]::SetEnvironmentVariable($v, $saved[$v]) }
    Remove-Item $runDir -Recurse -Force -ErrorAction SilentlyContinue
}
exit $exitCode
