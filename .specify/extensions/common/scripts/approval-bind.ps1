# Records a fingerprint (SHA-256) of exactly what was approved, next to the approval.
param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][string]$Record, [string]$ApprovedBy = 'Martin')
$ErrorActionPreference = 'Stop'
function Get-Fingerprint([string]$p) {
    $files = if (Test-Path $p -PathType Container) { Get-ChildItem $p -Recurse -File -Force | Where-Object FullName -ne ([IO.Path]::GetFullPath($Record)) | Sort-Object FullName } else { Get-Item $p }
    $lines = foreach ($f in $files) {
        $rel = if (Test-Path $p -PathType Container) { [IO.Path]::GetRelativePath((Resolve-Path $p), $f.FullName) } else { $f.Name }
        "$rel $([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(([IO.File]::ReadAllText($f.FullName) -replace "`r`n", "`n")))))"
    }
    $bytes = [Text.Encoding]::UTF8.GetBytes(($lines -join "`n"))
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
}
$fp = Get-Fingerprint $Path
New-Item -ItemType Directory -Force (Split-Path $Record) -ErrorAction SilentlyContinue | Out-Null
@{ path = $Path; fingerprint = $fp; approvedBy = $ApprovedBy; approvedAt = (Get-Date -Format o) } |
    ConvertTo-Json | Set-Content $Record -Encoding utf8
"BOUND $Path $fp"
exit 0