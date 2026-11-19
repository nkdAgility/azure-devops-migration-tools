# Shows exactly what will be written, and where, before a gate.
# Usage: approval-preview.ps1 -Staged <folder of proposed files> -Target <repo root>
param([Parameter(Mandatory)][string]$Staged, [string]$Target = '.')
$ErrorActionPreference = 'Stop'
if (-not (Test-Path $Staged)) { Write-Error "Nothing staged at $Staged"; exit 1 }
Get-ChildItem $Staged -Recurse -File | ForEach-Object {
    $rel = [IO.Path]::GetRelativePath((Resolve-Path $Staged), $_.FullName)
    $dest = Join-Path $Target $rel
    if (Test-Path $dest) { "CHANGE $rel"; git --no-pager diff --no-index -- $dest $_.FullName }
    else { "NEW    $rel" }
}
exit 0