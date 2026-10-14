# testplans (regression, #3209 guide): requirement, Shared Steps, Test Case using them, Test Plan with a static suite holding the
# Test Case and a requirement-based suite. Migrated with the four steps of docs/content/docs/how-to/migrating-plans-and-suites.
param([string]$Src = "amte2e-tp-src", [string]$Tgt = "amte2e-tp-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null
New-E2ENode $Src Iterations "Release 1" -Start (Get-Date "2026-10-01") -Finish (Get-Date "2026-10-31")

$us = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "tp requirement" })
$ssSteps = '<steps id="0" last="2"><step id="1" type="ActionStep"><parameterizedString isformatted="true">open app</parameterizedString><parameterizedString isformatted="true">app opens</parameterizedString><description/></step></steps>'
$ss = Set-E2EWorkItem $Src "Shared Steps" -Fields ([ordered]@{ "System.Title" = "tp shared steps"; "Microsoft.VSTS.TCM.Steps" = $ssSteps })
$tcSteps = "<steps id=""0"" last=""3""><compref id=""2"" ref=""$($ss.id)""/><step id=""3"" type=""ValidateStep""><parameterizedString isformatted=""true"">click save</parameterizedString><parameterizedString isformatted=""true"">saved</parameterizedString><description/></step></steps>"
# The UI adds a "Shared Steps" link next to the compref; the migration finds shared steps through that link.
$tc = Set-E2EWorkItem $Src "Test Case" -Fields ([ordered]@{ "System.Title" = "tp test case"; "Microsoft.VSTS.TCM.Steps" = $tcSteps }) -Ops @(@{ op = "add"; path = "/relations/-"; value = @{ rel = "Microsoft.VSTS.TestCase.SharedStepReferencedBy-Reverse"; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/wit/workItems/$($ss.id)" } }, @{ op = "add"; path = "/relations/-"; value = @{ rel = "Microsoft.VSTS.Common.TestedBy-Reverse"; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/wit/workItems/$($us.id)" } })

$plan = Invoke-E2ERest "$Src/_apis/testplan/plans" -Method POST -Body @{ name = "amte2e plan"; areaPath = $Src; iteration = "$Src\Release 1" }
$static = Invoke-E2ERest "$Src/_apis/testplan/Plans/$($plan.id)/suites" -Method POST -Body @{ suiteType = "staticTestSuite"; name = "Suite A"; parentSuite = @{ id = $plan.rootSuite.id } }
$req = Invoke-E2ERest "$Src/_apis/testplan/Plans/$($plan.id)/suites" -Method POST -Body @{ suiteType = "requirementTestSuite"; requirementId = $us.id; parentSuite = @{ id = $plan.rootSuite.id } }
Invoke-E2ERest "$Src/_apis/testplan/Plans/$($plan.id)/Suites/$($static.id)/TestCase" -Method POST -Body (ConvertTo-Json @(@{ workItem = @{ id = $tc.id } }) -Depth 5) | Out-Null

Save-E2EState $PSScriptRoot @{ us = $us.id; ss = $ss.id; tc = $tc.id; plan = $plan.id; static = $static.id; req = $req.id }
"seeded testplans: US $($us.id) SS $($ss.id) TC $($tc.id) plan $($plan.id) static $($static.id) req-suite $($req.id) ($($req.name))"
