#Requires -Version 7.0
# testplans-3260: Test Cases added to a plan's root suite after the plan was migrated are added on a re-run (#3260).
#   -Step 1: projects; TC1 in the root suite and in static Suite A of plan "amte2e 3260 plan".
#   -Step 2: TC2 added to the root suite and Suite A (migrate Test Cases, then Plans and Suites again).
#   -Step 3: TC3 added to the root suite only (run Plans and Suites before migrating it: the failure path).
param([ValidateSet(1, 2, 3)][int]$Step = 1, [string]$Src = "amte2e-3260-src", [string]$Tgt = "amte2e-3260-tgt", [string]$Process = "amte2e-agile", [string]$StateDir = $PSScriptRoot)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

function Add-ToSuite([int]$PlanId, [int]$SuiteId, [int]$TestCaseId) {
    Invoke-E2ERest "$Src/_apis/testplan/Plans/$PlanId/Suites/$SuiteId/TestCase" -Method POST -Body (ConvertTo-Json @(@{ workItem = @{ id = $TestCaseId } }) -Depth 5) | Out-Null
}

if ($Step -eq 1) {
    Set-E2EOrg nkdagility-adomt-target
    New-E2EProject -Name $Tgt -Process $Process | Out-Null
}
Set-E2EOrg nkdagility-adomt-source
switch ($Step) {
    1 {
        New-E2EProject -Name $Src -Process $Process | Out-Null
        $tc = Set-E2EWorkItem $Src "Test Case" -Fields ([ordered]@{ "System.Title" = "tc1 root and suite" })
        $plan = Invoke-E2ERest "$Src/_apis/testplan/plans" -Method POST -Body @{ name = "amte2e 3260 plan"; areaPath = $Src }
        $a = Invoke-E2ERest "$Src/_apis/testplan/Plans/$($plan.id)/suites" -Method POST -Body @{ suiteType = "staticTestSuite"; name = "Suite A"; parentSuite = @{ id = $plan.rootSuite.id } }
        Add-ToSuite $plan.id $plan.rootSuite.id $tc.id
        Add-ToSuite $plan.id $a.id $tc.id
        Save-E2EState $StateDir @{ tc1 = $tc.id; plan = $plan.id; root = $plan.rootSuite.id; suiteA = $a.id }
        "seeded step 1: TC1 $($tc.id), plan $($plan.id), root $($plan.rootSuite.id), Suite A $($a.id)"
    }
    2 {
        $st = Read-E2EState $StateDir
        $tc = Set-E2EWorkItem $Src "Test Case" -Fields ([ordered]@{ "System.Title" = "tc2 root and suite" })
        Add-ToSuite $st.plan $st.root $tc.id
        Add-ToSuite $st.plan $st.suiteA $tc.id
        "seeded step 2: TC2 $($tc.id) in the root suite and Suite A"
    }
    3 {
        $st = Read-E2EState $StateDir
        $tc = Set-E2EWorkItem $Src "Test Case" -Fields ([ordered]@{ "System.Title" = "tc3 root only" })
        Add-ToSuite $st.plan $st.root $tc.id
        "seeded step 3: TC3 $($tc.id) in the root suite only"
    }
}
