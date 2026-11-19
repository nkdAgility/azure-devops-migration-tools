# Fails the step if a review file reports open findings.
param([Parameter(Mandatory)][string]$File)
if (-not (Test-Path $File)) { Write-Error "$File was not written."; exit 1 }
$m = Select-String -Path $File -Pattern 'Open findings:\s*(\d+)' | Select-Object -Last 1
if (-not $m) { Write-Error "$File does not end with 'Open findings: N'."; exit 1 }
$n = [int]$m.Matches[0].Groups[1].Value
if ($n -gt 0) { Write-Error "$File has $n open findings."; exit 1 }
"no open findings in $File"
exit 0