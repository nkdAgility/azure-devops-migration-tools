# Writes the staged files only after the gate approved them, and only if they still match
# the fingerprint recorded at approval.
param([Parameter(Mandatory)][string]$Staged, [string]$Target = '.', [Parameter(Mandatory)][string]$Record)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'approval-check.ps1') -Path $Staged -Record $Record
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Get-ChildItem $Staged -Recurse -File | ForEach-Object {
    $rel = [IO.Path]::GetRelativePath((Resolve-Path $Staged), $_.FullName)
    $dest = Join-Path $Target $rel
    New-Item -ItemType Directory -Force (Split-Path $dest) | Out-Null
    Copy-Item $_.FullName $dest -Force
    "WROTE $rel"
}
exit 0