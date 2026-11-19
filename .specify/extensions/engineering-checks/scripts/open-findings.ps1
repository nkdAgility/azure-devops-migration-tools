# Fails the step if a review file reports open findings.
param([Parameter(Mandatory)][string]$File)
if (-not (Test-Path $File)) { Write-Error "$File was not written."; exit 1 }
$last = @(Get-Content $File | Where-Object { $_.Trim() }) | Select-Object -Last 1
$m = [regex]::Match("$last", '^\s*Open findings:\s*(\d+)\s*$')
if (-not $m.Success) { Write-Error "$File does not end with 'Open findings: N'."; exit 1 }
$n = [int]$m.Groups[1].Value
if ($n -gt 0) { Write-Error "$File has $n open findings."; exit 1 }
"no open findings in $File"
exit 0