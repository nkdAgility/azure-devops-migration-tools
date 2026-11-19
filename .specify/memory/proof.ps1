# The real-system proof for Output Done: runs every end-to-end scenario on the test organisations
# (tools/e2e, see "Validating Changes End to End" in .github/copilot-instructions.md).
# Fails if any scenario fails. Needs `az login` (or AMT_TEST_PAT) for the test orgs.
$ErrorActionPreference = 'Stop'
$failed = @()
foreach ($cfg in Get-ChildItem tools/e2e/scenarios -Recurse -Filter config.json) {
    "== $($cfg.Directory.Name)"
    pwsh -NoProfile -File tools/e2e/Invoke-MigrationE2E.ps1 -Config $cfg.FullName
    if ($LASTEXITCODE -ne 0) { $failed += $cfg.Directory.Name }
}
if ($failed) { Write-Error "end-to-end scenarios failed: $($failed -join ', ')"; exit 1 }
"all end-to-end scenarios passed"
exit 0
