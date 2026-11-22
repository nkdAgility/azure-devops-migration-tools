# Writes the staged files only after the gate approved them, and only if they still match
# the fingerprint recorded at approval.
param([Parameter(Mandatory)][string]$Staged, [string]$Target = '.', [Parameter(Mandatory)][string]$Record)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'approval-check.ps1') -Path $Staged -Record $Record
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$recordPath = [IO.Path]::GetFullPath($Record)
$isDir = Test-Path $Staged -PathType Container
$items = if ($isDir) { Get-ChildItem $Staged -Recurse -File -Force | Where-Object FullName -ne $recordPath | Sort-Object FullName } else { Get-Item $Staged }
$snapshot = @($items | ForEach-Object {
    $bytes = [IO.File]::ReadAllBytes($_.FullName)
    $rel = if ($isDir) { [IO.Path]::GetRelativePath((Resolve-Path $Staged), $_.FullName) } else { $_.Name }
    # The fingerprint is over the text with line endings normalised, the same as approval-bind and approval-check.
    @{ rel = $rel; bytes = $bytes; hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(([Text.Encoding]::UTF8.GetString($bytes) -replace "`r`n", "`n")))) }
})
$fp = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes((($snapshot | ForEach-Object { "$($_.rel) $($_.hash)" }) -join "`n"))))
if ($fp -ne (Get-Content $Record -Raw | ConvertFrom-Json).fingerprint) { Write-Error "Changed since approval: $Staged. Approve it again."; exit 1 }
foreach ($f in $snapshot) {
    $dest = Join-Path $Target $f.rel
    New-Item -ItemType Directory -Force (Split-Path $dest) | Out-Null
    [IO.File]::WriteAllBytes([IO.Path]::GetFullPath($dest), $f.bytes)
    "WROTE $($f.rel)"
}
exit 0