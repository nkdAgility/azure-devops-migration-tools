#Requires -Version 7.0
# testplans-3260: the target plan exists once and its root suite and Suite A hold exactly the expected Test Cases.
param([Parameter(Mandatory)][string[]]$Root, [Parameter(Mandatory)][string[]]$SuiteA, [string]$Tgt = "amte2e-3260-tgt")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Set-E2EOrg nkdagility-adomt-target
$failures = @()
$plans = @((Invoke-E2ERest "$Tgt/_apis/testplan/plans").value | Where-Object name -eq "amte2e 3260 plan")
if ($plans.Count -ne 1) { throw "expected 1 target plan 'amte2e 3260 plan', found $($plans.Count)" }
$suites = (Invoke-E2ERest "$Tgt/_apis/testplan/Plans/$($plans[0].id)/suites").value
foreach ($expect in @(@{ Name = "amte2e 3260 plan"; Titles = $Root }, @{ Name = "Suite A"; Titles = $SuiteA })) {
    $suite = $suites | Where-Object name -eq $expect.Name
    $titles = @((Invoke-E2ERest "$Tgt/_apis/testplan/Plans/$($plans[0].id)/Suites/$($suite.id)/TestCase").value | ForEach-Object { $_.workItem.name } | Sort-Object)
    $want = @($expect.Titles | Sort-Object)
    "suite '$($expect.Name)': $($titles.Count) Test Cases: $($titles -join ', ')"
    if (($titles -join '|') -ne ($want -join '|')) { $failures += "suite '$($expect.Name)' holds [$($titles -join ', ')], expected [$($want -join ', ')]" }
}
if ($failures) { $failures | ForEach-Object { "FAIL: $_" }; exit 1 }
"PASS"
exit 0
