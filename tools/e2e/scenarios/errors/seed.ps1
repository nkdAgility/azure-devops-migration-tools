# errors (#3181) and typechange (#3179). Both targets use a process where Bug is disabled.
#  errors:     3 Tasks and 1 Bug around them; the Bug cannot be created in the target, the Tasks must still migrate.
#  typechange: a Bug whose type is then changed to User Story; the target shell must be created as User Story.
param([string]$Process = "amte2e-agile", [string]$NoBugProcess = "amte2e-nobug")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EProcess.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
$pid_ = New-E2EBaseProcess -Name $NoBugProcess -Org nkdagility-adomt-target
Set-E2EWitDisabled $pid_ "Bug" $true
foreach ($p in "amte2e-errors-tgt", "amte2e-typechange-tgt") { New-E2EProject -Name $p -Process $NoBugProcess | Out-Null }

Set-E2EOrg nkdagility-adomt-source
foreach ($p in "amte2e-errors-src", "amte2e-typechange-src") { New-E2EProject -Name $p -Process $Process | Out-Null }

$t1 = Set-E2EWorkItem "amte2e-errors-src" "Task" -Fields ([ordered]@{ "System.Title" = "errors task 1" })
$bug = Set-E2EWorkItem "amte2e-errors-src" "Bug" -Fields ([ordered]@{ "System.Title" = "errors bug (fails in target)" })
$t2 = Set-E2EWorkItem "amte2e-errors-src" "Task" -Fields ([ordered]@{ "System.Title" = "errors task 2" })
$t3 = Set-E2EWorkItem "amte2e-errors-src" "Task" -Fields ([ordered]@{ "System.Title" = "errors task 3" })

$tc = Set-E2EWorkItem "amte2e-typechange-src" "Bug" -Fields ([ordered]@{ "System.Title" = "typechange item"; "Microsoft.VSTS.TCM.ReproSteps" = "repro" })
Set-E2EWorkItem -Id $tc.id -Fields ([ordered]@{ "System.WorkItemType" = "User Story"; "System.State" = "New" }) | Out-Null
Set-E2EWorkItem -Id $tc.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null
$ctl = Set-E2EWorkItem "amte2e-typechange-src" "Task" -Fields ([ordered]@{ "System.Title" = "typechange control task" })

Save-E2EState $PSScriptRoot @{ tasks = @($t1.id, $t2.id, $t3.id); bug = $bug.id; typechange = $tc.id; control = $ctl.id }
"seeded errors: tasks $($t1.id),$($t2.id),$($t3.id) bug $($bug.id); typechange: $($tc.id) control $($ctl.id)"
