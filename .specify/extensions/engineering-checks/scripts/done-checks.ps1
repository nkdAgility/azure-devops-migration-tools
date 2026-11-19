# Fresh full build, full test suite, then the repository's own real-system proof.
# The repository names its commands in .specify/memory/done-checks.json:
#   { "build": "dotnet build", "test": "dotnet test", "proof": "pwsh ./proof.ps1" }
$ErrorActionPreference = 'Stop'
$cfgPath = '.specify/memory/done-checks.json'
if (-not (Test-Path $cfgPath)) { Write-Error "Missing $cfgPath: the repository must name its build, test and proof commands."; exit 1 }
$cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json
foreach ($k in 'build','test','proof') {
    if (-not $cfg.$k) { Write-Error "done-checks.json has no '$k' command."; exit 1 }
    "== $k: $($cfg.$k)"
    pwsh -NoProfile -Command $cfg.$k
    if ($LASTEXITCODE -ne 0) { Write-Error "$k failed."; exit 1 }
}
exit 0