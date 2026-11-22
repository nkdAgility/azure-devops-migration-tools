# Brings the stock bug assessment onto the item: Reproduction, Cause and Proposed fix sections,
# and each open clarification as an open question. Fails if the assessment says the bug is invalid.
$ErrorActionPreference = 'Stop'
$a = Get-ChildItem .specify/bugs -Recurse -Filter assessment.md -ErrorAction SilentlyContinue | Sort-Object LastWriteTime | Select-Object -Last 1
if (-not $a) { Write-Error 'No bug assessment found under .specify/bugs/.'; exit 1 }
$t = (Get-Content $a.FullName -Raw) -replace "`r`n", "`n"
function Sec($name) { $m = [regex]::Match($t, "(?ms)^##\s*$([regex]::Escape($name))\s*\n(.*?)(?=^##\s|\z)"); if ($m.Success) { $m.Groups[1].Value.Trim() } else { '' } }
if ($t -match '(?im)^\s*-\s*\*\*Verdict\*\*:\s*invalid') { Write-Error "The assessment says this is not a bug. Record 'Removed because:' and move it to Removed."; exit 1 }
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$rec = Join-Path $PSScriptRoot 'record.ps1'
$rel = [IO.Path]::GetRelativePath((Get-Location), $a.FullName).Replace('\', '/')
& $rec -Section 'Spec' -Body "Working spec: ``$rel`` (the bug assessment Martin approves)."
# The version Martin is shown at the review gate; spec-agreed.ps1 binds his approval to exactly this.
& $rec -Line "Spec presented: $rel sha256 $(Get-TextHash $a.FullName)"
& $rec -Section 'Reproduction' -Body (Sec 'Reproduction')
& $rec -Section 'Cause' -Body (Sec 'Root Cause Hypothesis')
& $rec -Section 'Proposed fix' -Body (Sec 'Proposed Remediation')
$open = [regex]::Matches((Sec 'Open Questions') + "`n" + (Sec 'Reproduction'), '\[NEEDS CLARIFICATION:\s*([^\]]+)\]')
if ($open.Count) {
    $n = 0; $lines = foreach ($o in $open) { $n++; "- Q-$n $($o.Groups[1].Value.Trim()) (for Martin) open" }
    & $rec -Section 'Questions' -Body ($lines -join "`n")
}
"assessment $($a.Directory.Name) brought onto the item"
exit 0