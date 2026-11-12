# sharedsteps: Shared Steps and Shared Parameter cannot hold Custom.ReflectedWorkItemId on an inherited process.
# Seeds one of each and a Test Case that uses both, so a run and a re-run show the items are migrated once and wired up.
param([string]$Src = "amte2e-ss-src", [string]$Tgt = "amte2e-ss-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

$ssSteps = '<steps id="0" last="2"><step id="2" type="ActionStep"><parameterizedString isformatted="true">open @app</parameterizedString><parameterizedString isformatted="true">app opens</parameterizedString><description/></step></steps>'
$ss = Set-E2EWorkItem $Src "Shared Steps" -Fields ([ordered]@{ "System.Title" = "ss shared steps"; "Microsoft.VSTS.TCM.Steps" = $ssSteps })
$spParams = '<parameterSet><paramNames><param>app</param></paramNames><paramData lastId="1"><dataRow id="1"><kvp key="app" value="notepad"/></dataRow></paramData></parameterSet>'
$sp = Set-E2EWorkItem $Src "Shared Parameter" -Fields ([ordered]@{ "System.Title" = "ss shared parameter"; "Microsoft.VSTS.TCM.Parameters" = $spParams })

$tcSteps = "<steps id=""0"" last=""3""><compref id=""2"" ref=""$($ss.id)""/><step id=""3"" type=""ValidateStep""><parameterizedString isformatted=""true"">click save</parameterizedString><parameterizedString isformatted=""true"">saved</parameterizedString><description/></step></steps>"
$tcData = "{""parameterMap"":[{""paramName"":""app"",""bindedParamName"":""app"",""dataSetName"":""ss shared parameter"",""dataSetId"":$($sp.id)}],""sharedParameterDataSetIds"":[$($sp.id)],""rowMappingType"":0}"
$tc = Set-E2EWorkItem $Src "Test Case" -Fields ([ordered]@{ "System.Title" = "ss test case"; "Microsoft.VSTS.TCM.Steps" = $tcSteps; "Microsoft.VSTS.TCM.Parameters" = '<parameters><param name="app" bind="default"/></parameters>'; "Microsoft.VSTS.TCM.LocalDataSource" = $tcData }) -Ops @(
    @{ op = "add"; path = "/relations/-"; value = @{ rel = "Microsoft.VSTS.TestCase.SharedStepReferencedBy-Reverse"; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/wit/workItems/$($ss.id)" } },
    @{ op = "add"; path = "/relations/-"; value = @{ rel = "Microsoft.VSTS.TestCase.SharedParameterReferencedBy-Reverse"; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/wit/workItems/$($sp.id)" } })

Save-E2EState $PSScriptRoot @{ ss = $ss.id; sp = $sp.id; tc = $tc.id }
"seeded sharedsteps: SS $($ss.id) SP $($sp.id) TC $($tc.id)"
