# pipelines-3312: YAML build definitions whose repository has no properties. "amte2e-3312-yaml" uses repository
# amte2e-3312-app, which exists in both projects, so it must migrate and point at the target repository.
# "amte2e-3312-missing" uses amte2e-3312-gone, which exists only in the source, so it must fail with a clear message.
param([string]$Src = "amte2e-3312-src", [string]$Tgt = "amte2e-3312-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

function Initialize-Repo([string]$Org, [string]$Project, [string]$Name) {
    $repo = (Invoke-E2ERest "$Project/_apis/git/repositories" -Org $Org).value | Where-Object name -eq $Name
    if (-not $repo) {
        $projectId = (Invoke-E2ERest "_apis/projects/$Project" -Org $Org).id
        $repo = Invoke-E2ERest "$Project/_apis/git/repositories" -Org $Org -Method POST -Body @{ name = $Name; project = @{ id = $projectId } }
    }
    if (-not $repo.defaultBranch) {
        Invoke-E2ERest "$Project/_apis/git/repositories/$($repo.id)/pushes" -Org $Org -Method POST -Body @{
            refUpdates = @(@{ name = "refs/heads/main"; oldObjectId = "0000000000000000000000000000000000000000" })
            commits = @(@{ comment = "init"; changes = @(@{ changeType = "add"; item = @{ path = "/azure-pipelines.yml" }; newContent = @{ content = "steps:`n- script: echo amte2e`n"; contentType = "rawtext" } }) })
        } | Out-Null
    }
    Invoke-E2ERest "$Project/_apis/git/repositories/$($repo.id)" -Org $Org
}

function New-YamlDefinition([string]$Name, $Repo, $QueueId) {
    $existing = (Invoke-E2ERest "$Src/_apis/build/definitions?name=$Name").value | Select-Object -First 1
    if ($existing) { return $existing }
    # No properties on the repository: this is the shape that #3312 is about.
    Invoke-E2ERest "$Src/_apis/build/definitions" -Method POST -Body @{
        name = $Name; type = "build"; quality = "definition"; queue = @{ id = $QueueId }
        repository = @{ id = $Repo.id; name = $Repo.name; type = "TfsGit"; url = $Repo.remoteUrl; defaultBranch = "refs/heads/main" }
        process = @{ type = 2; yamlFilename = "/azure-pipelines.yml" }
    }
}

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
$tgtRepo = Initialize-Repo nkdagility-adomt-target $Tgt "amte2e-3312-app"
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null
$appRepo = Initialize-Repo nkdagility-adomt-source $Src "amte2e-3312-app"
$goneRepo = Initialize-Repo nkdagility-adomt-source $Src "amte2e-3312-gone"
$queue = (Invoke-E2ERest "$Src/_apis/distributedtask/queues?queueName=Azure%20Pipelines" -ApiVersion "7.1-preview.1").value | Select-Object -First 1

$yaml = New-YamlDefinition "amte2e-3312-yaml" $appRepo $queue.id
$missing = New-YamlDefinition "amte2e-3312-missing" $goneRepo $queue.id
$stored = Invoke-E2ERest "$Src/_apis/build/definitions/$($yaml.id)"
Save-E2EState $PSScriptRoot @{ yaml = $yaml.id; missing = $missing.id; targetRepo = $tgtRepo.id }
"seeded: yaml $($yaml.id) (repository properties: $([bool]$stored.repository.properties)) missing $($missing.id) target repo $($tgtRepo.id)"
