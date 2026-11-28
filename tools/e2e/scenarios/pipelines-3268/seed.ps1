# pipelines-3268: classic releases with a Build artifact that their deploy job downloads.
# "amte2e-3268-release" uses "amte2e-3268-build", which is migrated with it, so it must migrate with its artifact
# pointing at the target project and build definition. "amte2e-3268-orphan-release" uses "amte2e-3268-orphan",
# which is never migrated, so it must be skipped with a clear error instead of a 400 from the target.
param([string]$Src = "amte2e-3268-src", [string]$Tgt = "amte2e-3268-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$cmd = "d9bafed4-0b18-4f58-968d-86655b4d2ce9"

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

function New-ClassicBuild([string]$Name, $Repo, $QueueId) {
    $existing = (Invoke-E2ERest "$Src/_apis/build/definitions?name=$Name").value | Select-Object -First 1
    if ($existing) { return $existing }
    Invoke-E2ERest "$Src/_apis/build/definitions" -Method POST -Body @{
        name = $Name; type = "build"; quality = "definition"; queue = @{ id = $QueueId }
        repository = @{ id = $Repo.id; name = $Repo.name; type = "TfsGit"; url = $Repo.remoteUrl; defaultBranch = "refs/heads/main"; properties = @{ cleanOptions = "0"; labelSources = "0"; reportBuildStatus = "true"; fetchDepth = "1" } }
        process = @{ type = 1; phases = @(@{ name = "Job 1"; refName = "Job_1"; condition = "succeeded()"; target = @{ type = 1; executionOptions = @{ type = 0 }; allowScriptsAuthAccessOption = $false }; jobAuthorizationScope = 1
            steps = @(@{ enabled = $true; displayName = "echo"; task = @{ id = $cmd; versionSpec = "2.*"; definitionType = "task" }; inputs = @{ script = "echo amte2e" } }) }) }
    }
}

function New-ReleaseWithArtifact([string]$Name, $Build, $Project, $QueueId) {
    $existing = (Invoke-E2ERest "$Src/_apis/release/definitions?searchText=$Name&isExactNameMatch=true" -Host_ "vsrm.dev.azure.com").value | Select-Object -First 1
    if ($existing) { return $existing }
    $alias = "_$($Build.name)"
    Invoke-E2ERest "$Src/_apis/release/definitions" -Host_ "vsrm.dev.azure.com" -Method POST -Body @{
        name = $Name; releaseNameFormat = "Release-`$(rev:r)"
        artifacts = @(@{ alias = $alias; type = "Build"; isPrimary = $true
            definitionReference = @{ project = @{ id = $Project.id; name = $Src }; definition = @{ id = "$($Build.id)"; name = $Build.name }; defaultVersionType = @{ id = "latestType"; name = "Latest" } } })
        environments = @(@{ name = "Stage 1"; rank = 1; retentionPolicy = @{ daysToKeep = 30; releasesToKeep = 3; retainBuild = $true }
            preDeployApprovals = @{ approvals = @(@{ rank = 1; isAutomated = $true; isNotificationOn = $false }) }
            postDeployApprovals = @{ approvals = @(@{ rank = 1; isAutomated = $true; isNotificationOn = $false }) }
            deployPhases = @(@{ name = "Agent job"; rank = 1; phaseType = 1
                deploymentInput = @{ queueId = $QueueId; agentSpecification = @{ identifier = "windows-latest" }; skipArtifactsDownload = $false
                    artifactsDownloadInput = @{ downloadInputs = @(@{ alias = $alias; artifactType = "Build"; artifactDownloadMode = "All"; artifactItems = @() }) } }
                workflowTasks = @(@{ name = "echo"; enabled = $true; taskId = $cmd; version = "2.*"; definitionType = "task"; inputs = @{ script = "echo amte2e" } }) }) })
    }
}

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Initialize-Repo nkdagility-adomt-target $Tgt | Out-Null
Set-E2EOrg nkdagility-adomt-source
$proj = New-E2EProject -Name $Src -Process $Process
$repo = Initialize-Repo nkdagility-adomt-source $Src
$queue = (Invoke-E2ERest "$Src/_apis/distributedtask/queues?queueName=Azure%20Pipelines" -ApiVersion "7.1-preview.1").value | Select-Object -First 1

$build = New-ClassicBuild "amte2e-3268-build" $repo $queue.id
$orphan = New-ClassicBuild "amte2e-3268-orphan" $repo $queue.id
$release = New-ReleaseWithArtifact "amte2e-3268-release" $build $proj $queue.id
$orphanRelease = New-ReleaseWithArtifact "amte2e-3268-orphan-release" $orphan $proj $queue.id
Save-E2EState $PSScriptRoot @{ build = $build.id; orphan = $orphan.id; release = $release.id; orphanRelease = $orphanRelease.id }
"seeded: build $($build.id) orphan $($orphan.id) release $($release.id) orphan release $($orphanRelease.id)"
