# pipelines: #3207 (task group versions and counts), #3220 (run fails when items are skipped/rejected), regression of build and
# release pipelines and variable groups. Source: variable group, task group with v1 and v2 (created the way the tool creates
# versions: draft -> preview -> publish), a classic build using task group 2.* and the variable group, and a release.
param([string]$Src = "amte2e-pipe-src", [string]$Tgt = "amte2e-pipe-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$av = "7.1-preview.1"

function Initialize-Repo([string]$Org, [string]$Project) {
    $repo = (Invoke-E2ERest "$Project/_apis/git/repositories/$Project" -Org $Org)
    if (-not $repo.defaultBranch) {
        Invoke-E2ERest "$Project/_apis/git/repositories/$($repo.id)/pushes" -Org $Org -Method POST -Body @{
            refUpdates = @(@{ name = "refs/heads/main"; oldObjectId = "0000000000000000000000000000000000000000" })
            commits = @(@{ comment = "init"; changes = @(@{ changeType = "add"; item = @{ path = "/README.md" }; newContent = @{ content = "amte2e"; contentType = "rawtext" } }) })
        } | Out-Null
    }
    Invoke-E2ERest "$Project/_apis/git/repositories/$Project" -Org $Org
}

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Initialize-Repo nkdagility-adomt-target $Tgt | Out-Null
Set-E2EOrg nkdagility-adomt-source
$proj = New-E2EProject -Name $Src -Process $Process
$repo = Initialize-Repo nkdagility-adomt-source $Src

$vg = (Invoke-E2ERest "$Src/_apis/distributedtask/variablegroups?groupName=amte2e-vg" -ApiVersion "7.1-preview.2").value | Select-Object -First 1
if (-not $vg) {
    $vg = Invoke-E2ERest "_apis/distributedtask/variablegroups" -ApiVersion "7.1-preview.2" -Method POST -Body @{
        name = "amte2e-vg"; type = "Vsts"; variables = @{ greeting = @{ value = "hello" } }
        variableGroupProjectReferences = @(@{ name = "amte2e-vg"; projectReference = @{ id = $proj.id; name = $Src } })
    }
}

$cmd = (Invoke-E2ERest "_apis/distributedtask/tasks/d9bafed4-0b18-4f58-968d-86655b4d2ce9" -ApiVersion $av).value | Sort-Object { $_.version.major } -Descending | Select-Object -First 1
$tg = (Invoke-E2ERest "$Src/_apis/distributedtask/taskgroups" -ApiVersion $av).value | Where-Object name -eq "amte2e-tg" | Select-Object -First 1
if (-not $tg) {
    $tg = Invoke-E2ERest "$Src/_apis/distributedtask/taskgroups" -ApiVersion $av -Method POST -Body @{
        name = "amte2e-tg"; category = "Build"; instanceNameFormat = "amte2e-tg"; runsOn = @("Agent"); inputs = @()
        version = @{ major = 1; minor = 0; patch = 0; isTest = $false }
        tasks = @(@{ enabled = $true; displayName = "echo v1"; alwaysRun = $false; continueOnError = $false; timeoutInMinutes = 0; task = @{ id = $cmd.id; versionSpec = "$($cmd.version.major).*"; definitionType = "task" }; inputs = @{ script = "echo v1" } })
    }
}
$versions = (Invoke-E2ERest "$Src/_apis/distributedtask/taskgroups/$($tg.id)?expanded=true" -ApiVersion $av).value
if (-not ($versions | Where-Object { $_.version.major -eq 2 })) {
    # Deep copy without -AsHashtable, which Windows PowerShell 5.1 does not have.
    $b = $tg | ConvertTo-Json -Depth 20 | ConvertFrom-Json
    foreach ($k in "id", "revision", "createdBy", "createdOn", "modifiedBy", "modifiedOn", "owner") { $b.PSObject.Properties.Remove($k) }
    $b | Add-Member -NotePropertyName version -NotePropertyValue ([pscustomobject]@{ major = 2; minor = 0; patch = 0; isTest = $true }) -Force
    $b | Add-Member -NotePropertyName parentDefinitionId -NotePropertyValue $tg.id -Force
    $b.tasks[0].inputs.script = "echo v2"; $b.tasks[0].displayName = "echo v2"
    $draft = Invoke-E2ERest "$Src/_apis/distributedtask/taskgroups" -ApiVersion $av -Method POST -Body $b
    Invoke-E2ERest "$Src/_apis/distributedtask/taskgroups?parentTaskGroupId=$($tg.id)" -ApiVersion $av -Method PUT -Body @{ comment = "preview"; parentDefinitionRevision = $tg.revision; preview = $true; taskGroupId = $draft.id; taskGroupRevision = 1 } | Out-Null
    Invoke-E2ERest "$Src/_apis/distributedtask/taskgroups/$($tg.id)?disablePriorVersions=false" -ApiVersion $av -Method PATCH -Body @{ comment = "publish v2"; revision = $tg.revision + 1; preview = $false; version = @{ major = 2; minor = 0; patch = 0; isTest = $false } } | Out-Null
}

$queue = (Invoke-E2ERest "$Src/_apis/distributedtask/queues?queueName=Azure%20Pipelines" -ApiVersion "7.1-preview.1").value | Select-Object -First 1
$build = (Invoke-E2ERest "$Src/_apis/build/definitions?name=amte2e-build").value | Select-Object -First 1
if (-not $build) {
    $build = Invoke-E2ERest "$Src/_apis/build/definitions" -Method POST -Body @{
        name = "amte2e-build"; type = "build"; quality = "definition"; queue = @{ id = $queue.id }
        # properties as the designer writes them; the processor only maps the repository when they are present.
        repository = @{ id = $repo.id; name = $repo.name; type = "TfsGit"; url = $repo.remoteUrl; defaultBranch = "refs/heads/main"; properties = @{ cleanOptions = "0"; labelSources = "0"; labelSourcesFormat = "`$(build.buildNumber)"; reportBuildStatus = "true"; gitLfsSupport = "false"; skipSyncSource = "false"; checkoutNestedSubmodules = "false"; fetchDepth = "1" } }
        variableGroups = @(@{ id = $vg.id })
        process = @{ type = 1; phases = @(@{ name = "Job 1"; refName = "Job_1"; condition = "succeeded()"; target = @{ type = 1; executionOptions = @{ type = 0 }; allowScriptsAuthAccessOption = $false }; jobAuthorizationScope = 1
            steps = @(@{ enabled = $true; displayName = "amte2e-tg"; task = @{ id = $tg.id; versionSpec = "2.*"; definitionType = "metaTask" }; inputs = @{} }) }) }
    }
}
$release = (Invoke-E2ERest "$Src/_apis/release/definitions?searchText=amte2e-release" -Host_ "vsrm.dev.azure.com").value | Select-Object -First 1
if (-not $release) {
    $release = Invoke-E2ERest "$Src/_apis/release/definitions" -Host_ "vsrm.dev.azure.com" -Method POST -Body @{
        name = "amte2e-release"; releaseNameFormat = "Release-`$(rev:r)"; variableGroups = @($vg.id)
        environments = @(@{ name = "Stage 1"; rank = 1; retentionPolicy = @{ daysToKeep = 30; releasesToKeep = 3; retainBuild = $true }
            preDeployApprovals = @{ approvals = @(@{ rank = 1; isAutomated = $true; isNotificationOn = $false }) }
            postDeployApprovals = @{ approvals = @(@{ rank = 1; isAutomated = $true; isNotificationOn = $false }) }
            deployPhases = @(@{ name = "Agent job"; rank = 1; phaseType = 1; deploymentInput = @{ queueId = $queue.id; agentSpecification = @{ identifier = "windows-latest" } }
                workflowTasks = @(@{ name = "amte2e-tg"; enabled = $true; taskId = $tg.id; version = "2.*"; definitionType = "metaTask"; inputs = @{} }) }) })
    }
}
Save-E2EState $PSScriptRoot @{ vg = $vg.id; tg = $tg.id; build = $build.id; release = $release.id; queue = $queue.id }
"seeded pipelines: vg $($vg.id) tg $($tg.id) (versions: $(((Invoke-E2ERest "$Src/_apis/distributedtask/taskgroups/$($tg.id)?expanded=true" -ApiVersion $av).value.version.major | Sort-Object) -join ',')) build $($build.id) release $($release.id)"
