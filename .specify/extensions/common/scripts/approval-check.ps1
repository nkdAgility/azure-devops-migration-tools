# Before a later step: checks the text still matches the approved fingerprint.
# Exit 1 (the step fails) if it changed, so it must be approved again.
param([Parameter(Mandatory)][string]$Path, [Parameter(Mandatory)][string]$Record)
$ErrorActionPreference = 'Stop'
if (-not (Test-Path $Record)) { Write-Error "No approval recorded for $Path"; exit 1 }
$rec = Get-Content $Record -Raw | ConvertFrom-Json
$files = if (Test-Path $Path -PathType Container) { Get-ChildItem $Path -Recurse -File -Force | Where-Object FullName -ne ([IO.Path]::GetFullPath($Record)) | Sort-Object FullName } else { Get-Item $Path }
$lines = foreach ($f in $files) {
    $rel = if (Test-Path $Path -PathType Container) { [IO.Path]::GetRelativePath((Resolve-Path $Path), $f.FullName) } else { $f.Name }
    "$rel $((Get-FileHash $f.FullName -Algorithm SHA256).Hash)"
}
$fp = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes(($lines -join "`n"))))
if ($fp -ne $rec.fingerprint) { Write-Error "Changed since approval: $Path. Approve it again."; exit 1 }
"MATCHES $Path"
exit 0