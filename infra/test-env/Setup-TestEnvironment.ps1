<#
.SYNOPSIS
    Creates or updates the integration test environment for #3193. Safe to re-run.

.DESCRIPTION
    1. Deploys main.bicep: the Entra app, its service principal and GitHub OIDC federated credentials.
    2. Binds that identity to the two Azure DevOps orgs: adds it as a user, makes it a
       Project Collection Administrator, and creates the test projects if they are missing.
    3. Configures GitHub: the integration-tests and integration-tests-forks environments
       (the latter with required reviewers) and the repository variables the workflow reads.

    Prerequisites (one-off, manual): the two Azure DevOps orgs exist and are connected to the
    Entra tenant; you are signed in with 'az login' (able to create app registrations, and an
    admin of both orgs) and 'gh auth login' (an admin of the repository).

    No secrets are created, printed or stored.

    -LocalOnly is for contributors running the live tests against their own two orgs: it skips the
    Entra app and GitHub steps, creates the test projects with your own login ('az login'), and
    prints (or with -SetUserEnvironment, saves) the environment variables the tests read.

.EXAMPLE
    # Maintainers: the project's CI environment
    ./Setup-TestEnvironment.ps1 -SourceOrg nkdagility-adomt-source -TargetOrg nkdagility-adomt-target -Reviewers MrHinsh

.EXAMPLE
    # Contributors: your own orgs, for local runs
    ./Setup-TestEnvironment.ps1 -LocalOnly -SourceOrg my-adomt-source -TargetOrg my-adomt-target -SetUserEnvironment
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $SourceOrg,
    [Parameter(Mandatory)] [string] $TargetOrg,
    [string[]] $Reviewers,
    [string] $Repository = 'nkdAgility/azure-devops-migration-tools',
    [string] $ResourceGroup = 'rg-adomt-integration-tests',
    [string] $Location = 'westeurope',
    [string] $SourceProject = 'migrationSource1',
    [string] $TargetProject = 'migrationTarget1',
    [string] $ProcessName = 'Agile',
    [switch] $SkipGitHub,
    [switch] $LocalOnly,
    [switch] $SetUserEnvironment
)

$ErrorActionPreference = 'Stop'
$AzureDevOpsResource = '499b84ac-1321-427f-aa17-267ca6975798'
if (-not $LocalOnly -and -not $Reviewers) { throw '-Reviewers is required (GitHub logins who approve fork runs), unless -LocalOnly.' }
$spObjectId = $null

function Invoke-Ado {
    param([string] $Method = 'GET', [string] $Uri, $Body, [string] $ContentType = 'application/json')
    $token = az account get-access-token --resource $AzureDevOpsResource --query accessToken -o tsv
    $headers = @{ Authorization = "Bearer $token" }
    $params = @{ Method = $Method; Uri = $Uri; Headers = $headers; ContentType = $ContentType }
    if ($null -ne $Body) { $params.Body = (ConvertTo-Json -InputObject $Body -Depth 20) }
    Invoke-RestMethod @params
}

# --- 1. Entra identity (Bicep) -------------------------------------------------------------------
if (-not $LocalOnly) {
    Write-Host "== Deploying Entra identity (main.bicep) into resource group $ResourceGroup"
    az group create --name $ResourceGroup --location $Location --output none
    $deployment = az deployment group create `
        --resource-group $ResourceGroup `
        --name adomt-integration-tests `
        --template-file (Join-Path $PSScriptRoot 'main.bicep') `
        --parameters repository=$Repository `
        --query properties.outputs -o json | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw 'Bicep deployment failed.' }
    $clientId = $deployment.clientId.value
    $spObjectId = $deployment.servicePrincipalObjectId.value
    $tenantId = $deployment.tenantId.value
    Write-Host "   clientId=$clientId tenant=$tenantId"
}

# --- 2. Azure DevOps orgs ------------------------------------------------------------------------
function Set-OrgAccess {
    param([string] $Org, [string] $Project)
    Write-Host "== Preparing https://dev.azure.com/$Org/"

    if ($spObjectId) {
        # Add the service principal to the org (idempotent: returns the existing entitlement).
        $entitlement = Invoke-Ado -Method POST -Uri "https://vsaex.dev.azure.com/$Org/_apis/serviceprincipalentitlements?api-version=7.1-preview.1" -Body @{
            accessLevel      = @{ accountLicenseType = 'express' }
            servicePrincipal = @{ origin = 'aad'; originId = $spObjectId; subjectKind = 'servicePrincipal' }
        }
        # The 7.1 response carries the entitlement under servicePrincipalEntitlement; older shapes used
        # operationResult.result. If neither has it, look the principal up by its Entra object id.
        $spDescriptor = @(
            $entitlement.servicePrincipalEntitlement.servicePrincipal.descriptor
            $entitlement.operationResult.result.servicePrincipal.descriptor
        ) | Where-Object { $_ } | Select-Object -First 1
        if (-not $spDescriptor) {
            $all = Invoke-Ado -Uri "https://vsaex.dev.azure.com/$Org/_apis/serviceprincipalentitlements?api-version=7.1-preview.1"
            $spDescriptor = ($all.items | Where-Object { $_.servicePrincipal.originId -eq $spObjectId } | Select-Object -First 1).servicePrincipal.descriptor
        }
        if (-not $spDescriptor) {
            $errors = ($entitlement.operationResult.errors | ForEach-Object { $_.value }) -join '; '
            throw "Could not add the service principal to $Org. $errors"
        }

        # Project Collection Administrators membership (PUT is idempotent).
        $groups = Invoke-Ado -Uri "https://vssps.dev.azure.com/$Org/_apis/graph/groups?scopeDescriptor=&api-version=7.1-preview.1"
        $pca = $groups.value | Where-Object { $_.principalName -eq "[$Org]\Project Collection Administrators" } | Select-Object -First 1
        if (-not $pca) { throw "Project Collection Administrators group not found in $Org." }
        Invoke-Ado -Method PUT -Uri "https://vssps.dev.azure.com/$Org/_apis/graph/memberships/$spDescriptor/$($pca.descriptor)?api-version=7.1-preview.1" | Out-Null
        Write-Host "   service principal is a Project Collection Administrator"
    }

    # Test project.
    $existing = (Invoke-Ado -Uri "https://dev.azure.com/$Org/_apis/projects?api-version=7.1").value | Where-Object name -eq $Project
    if ($existing) { Write-Host "   project $Project exists"; return }
    $process = (Invoke-Ado -Uri "https://dev.azure.com/$Org/_apis/process/processes?api-version=7.1").value | Where-Object name -eq $ProcessName
    if (-not $process) { throw "Process '$ProcessName' not found in $Org." }
    $op = Invoke-Ado -Method POST -Uri "https://dev.azure.com/$Org/_apis/projects?api-version=7.1" -Body @{
        name         = $Project
        visibility   = 'private'
        capabilities = @{
            versioncontrol  = @{ sourceControlType = 'Git' }
            processTemplate = @{ templateTypeId = $process.id }
        }
    }
    while ($op.status -notin 'succeeded', 'failed', 'cancelled') {
        Start-Sleep -Seconds 3
        $op = Invoke-Ado -Uri $op.url
    }
    if ($op.status -ne 'succeeded') { throw "Creating project $Project in $Org $($op.status)." }
    Write-Host "   created project $Project ($ProcessName)"
}

# --- Test data the live tests expect (idempotent) --------------------------------------------------
$ReleaseDefinitionName = 'New release pipeline'

function Add-TestWorkItems {
    param([string] $Org, [string] $Project, [int] $Count = 3)
    $wiql = @{ query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = '$Project' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')" }
    $found = (Invoke-Ado -Method POST -Uri "https://dev.azure.com/$Org/$Project/_apis/wit/wiql?`$top=1&api-version=7.1" -Body $wiql).workItems
    if ($found) { Write-Host "   work items exist in $Project"; return }
    $type = if ($ProcessName -eq 'Scrum') { 'Product Backlog Item' } elseif ($ProcessName -eq 'Basic') { 'Issue' } else { 'User Story' }
    for ($i = 1; $i -le $Count; $i++) {
        $patch = @(@{ op = 'add'; path = '/fields/System.Title'; value = "Integration test item $i" })
        Invoke-Ado -Method POST -Uri "https://dev.azure.com/$Org/$Project/_apis/wit/workitems/`$$([Uri]::EscapeDataString($type))?api-version=7.1" -Body $patch -ContentType 'application/json-patch+json' | Out-Null
    }
    Write-Host "   created $Count $type work items in $Project"
}

function Add-TestReleaseDefinition {
    param([string] $Org, [string] $Project)
    $base = "https://vsrm.dev.azure.com/$Org/$Project/_apis/release/definitions"
    $existing = (Invoke-Ado -Uri "$base`?searchText=$([Uri]::EscapeDataString($ReleaseDefinitionName))&isExactNameMatch=true&api-version=7.1").value
    if ($existing) { Write-Host "   release definition '$ReleaseDefinitionName' exists in $Project"; return }
    $queue = (Invoke-Ado -Uri "https://dev.azure.com/$Org/$Project/_apis/distributedtask/queues?queueName=Azure%20Pipelines&api-version=7.1-preview.1").value | Select-Object -First 1
    if (-not $queue) { throw "Agent queue 'Azure Pipelines' not found in $Org/$Project." }
    $approval = @{ approvals = @(@{ rank = 1; isAutomated = $true; isNotificationOn = $false }) }
    try {
    Invoke-Ado -Method POST -Uri "$base`?api-version=7.1" -Body @{
        name         = $ReleaseDefinitionName
        environments = @(@{
                name                = 'Stage 1'
                rank                = 1
                preDeployApprovals  = $approval
                postDeployApprovals = $approval
                retentionPolicy     = @{ daysToKeep = 30; releasesToKeep = 3; retainBuild = $true }
                deployPhases        = @(@{
                        name            = 'Agent job'
                        rank            = 1
                        phaseType       = 'agentBasedDeployment'
                        deploymentInput = @{ queueId = $queue.id }
                        workflowTasks   = @()
                    })
            })
    } | Out-Null
    }
    catch {
        if ("$_" -match 'classic pipelines are disabled') {
            Write-Warning ("Classic release pipelines are disabled in $Org, so '$ReleaseDefinitionName' could not be created and the release pipeline test will fail. " +
                "Fix: https://dev.azure.com/$Org/_settings/pipelinessettings (and Project settings > Pipelines > Settings in $Project): turn OFF " +
                "'Disable creation of classic release pipelines', then re-run this script.")
            return
        }
        throw
    }
    Write-Host "   created release definition '$ReleaseDefinitionName' in $Project"
}

Set-OrgAccess -Org $SourceOrg -Project $SourceProject
Set-OrgAccess -Org $TargetOrg -Project $TargetProject

Write-Host "== Seeding test data"
Add-TestWorkItems -Org $SourceOrg -Project $SourceProject
# The release pipeline test expects exactly one definition in the source that already exists in the target.
Add-TestReleaseDefinition -Org $SourceOrg -Project $SourceProject
Add-TestReleaseDefinition -Org $TargetOrg -Project $TargetProject

# --- 3. GitHub environments and variables --------------------------------------------------------
if (-not $SkipGitHub -and -not $LocalOnly) {
    Write-Host "== Configuring GitHub $Repository"
    '{}' | gh api -X PUT "repos/$Repository/environments/integration-tests" --input - | Out-Null

    $reviewerIds = foreach ($login in $Reviewers) { @{ type = 'User'; id = [int](gh api "users/$login" --jq .id) } }
    @{ prevent_self_review = $true; reviewers = @($reviewerIds) } | ConvertTo-Json -Depth 5 |
        gh api -X PUT "repos/$Repository/environments/integration-tests-forks" --input - | Out-Null
    Write-Host "   environments: integration-tests, integration-tests-forks (reviewers: $($Reviewers -join ', '))"

    $variables = [ordered]@{
        AZURE_TEST_CLIENT_ID       = $clientId
        AZURE_TEST_TENANT_ID       = $tenantId
        ADO_TEST_SOURCE_COLLECTION = "https://dev.azure.com/$SourceOrg/"
        ADO_TEST_TARGET_COLLECTION = "https://dev.azure.com/$TargetOrg/"
    }
    foreach ($name in $variables.Keys) { gh variable set $name --body $variables[$name] --repo $Repository }
    Write-Host "   variables: $($variables.Keys -join ', ')"
}

if ($LocalOnly) {
    $testVariables = [ordered]@{
        MigrationTools__Tests__UseEntraId       = 'true'
        MigrationTools__Tests__SourceCollection = "https://dev.azure.com/$SourceOrg/"
        MigrationTools__Tests__TargetCollection = "https://dev.azure.com/$TargetOrg/"
    }
    if ($SetUserEnvironment) {
        foreach ($name in $testVariables.Keys) { [Environment]::SetEnvironmentVariable($name, $testVariables[$name], 'User') }
        Write-Host "== Saved to your user environment. Open a new terminal, then run: dotnet test MigrationTools.sln"
    }
    else {
        Write-Host "== Done. Set these, then run: dotnet test MigrationTools.sln"
        foreach ($name in $testVariables.Keys) { Write-Host "   `$env:$name = '$($testVariables[$name])'" }
    }
    Write-Host "   (If 'az login' cannot reach your orgs, e.g. a personal Microsoft account, set MigrationTools__Tests__AccessToken to a PAT for them instead of UseEntraId.)"
    return
}
Write-Host "== Done. Run the 'Integration tests (Azure DevOps)' workflow to validate."
