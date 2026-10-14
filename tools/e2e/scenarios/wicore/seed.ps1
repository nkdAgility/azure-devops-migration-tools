# wi-core: hierarchy, multi-revision history, attachment and link added at known revisions, area/iteration nodes.
param([string]$Src = "amte2e-wicore-src", [string]$Tgt = "amte2e-wicore-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$u = Get-E2EUsers

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

New-E2ENode $Src Areas "Team A"
New-E2ENode $Src Areas "Sub" -Under "Team A"
New-E2ENode $Src Iterations "Sprint 1" -Start (Get-Date "2026-09-01") -Finish (Get-Date "2026-09-14")
New-E2ENode $Src Iterations "Sprint 2" -Start (Get-Date "2026-09-15") -Finish (Get-Date "2026-09-28")

$epic = Set-E2EWorkItem $Src "Epic" -Fields ([ordered]@{ "System.Title" = "E2E Epic"; "System.AreaPath" = "$Src\Team A" })
$feat = Set-E2EWorkItem $Src "Feature" -Fields ([ordered]@{ "System.Title" = "E2E Feature"; "System.AreaPath" = "$Src\Team A" }) -Ops @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Reverse" $epic.id)
$story = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "E2E Story"; "System.AreaPath" = "$Src\Team A\Sub"; "System.IterationPath" = "$Src\Sprint 1"; "System.AssignedTo" = $u.A }) -Ops @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Reverse" $feat.id)
$bug = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "E2E Bug"; "Microsoft.VSTS.TCM.ReproSteps" = "Steps v1" })
$task = Set-E2EWorkItem $Src "Task" -Fields ([ordered]@{ "System.Title" = "E2E Task"; "System.IterationPath" = "$Src\Sprint 2" }) -Ops @(Add-E2ELinkOp "System.LinkTypes.Hierarchy-Reverse" $story.id)

# Story revisions: 2 = activate + history, 3 = attachment, 4 = related link to bug, 5 = iteration move, 6 = resolve, 7 = close.
Set-E2EWorkItem -Id $story.id -Fields ([ordered]@{ "System.State" = "Active"; "System.History" = "Started work" }) | Out-Null
$att = New-E2EAttachment "e2e-note.txt" ([Text.Encoding]::UTF8.GetBytes("wicore attachment"))
Set-E2EWorkItem -Id $story.id -Ops @(Add-E2EAttachmentOp $att.url "added at rev 3") | Out-Null
Set-E2EWorkItem -Id $story.id -Ops @(Add-E2ELinkOp "System.LinkTypes.Related" $bug.id "related at rev 4") | Out-Null
Set-E2EWorkItem -Id $story.id -Fields ([ordered]@{ "System.IterationPath" = "$Src\Sprint 2" }) | Out-Null
Set-E2EWorkItem -Id $story.id -Fields ([ordered]@{ "System.State" = "Resolved"; "System.History" = "Done" }) | Out-Null
Set-E2EWorkItem -Id $story.id -Fields ([ordered]@{ "System.State" = "Closed" }) | Out-Null

Set-E2EWorkItem -Id $bug.id -Fields ([ordered]@{ "Microsoft.VSTS.TCM.ReproSteps" = "Steps v2"; "System.State" = "Active" }) | Out-Null

Save-E2EState $PSScriptRoot @{ epic = $epic.id; feature = $feat.id; story = $story.id; bug = $bug.id; task = $task.id }
"seeded: epic $($epic.id) feature $($feat.id) story $($story.id) bug $($bug.id) task $($task.id)"
