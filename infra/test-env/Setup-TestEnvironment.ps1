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

.EXAMPLE
    ./Setup-TestEnvironment.ps1 -SourceOrg adomt-test-source -TargetOrg adomt-test-target -Reviewers MrHinsh
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $SourceOrg,
    [Parameter(Mandatory)] [string] $TargetOrg,
    [Parameter(Mandatory)] [string[]] $Reviewers,
    [string] $Repository = 'nkdAgility/azure-devops-migration-tools',
    [string] $ResourceGroup = 'rg-adomt-integration-tests',
    [string] $Location = 'westeurope',
    [string] $SourceProject = 'migrationSource1',
    [string] $TargetProject = 'migrationTarget1',
    [string] $ProcessName = 'Agile',
    [switch] $SkipGitHub
)

$ErrorActionPreference = 'Stop'
$AzureDevOpsResource = '499b84ac-1321-427f-aa17-267ca6975798'

function Invoke-Ado {
    param([string] $Method = 'GET', [string] $Uri, $Body)
    $token = az account get-access-token --resource $AzureDevOpsResource --query accessToken -o tsv
    $headers = @{ Authorization = "Bearer $token" }
    $params = @{ Method = $Method; Uri = $Uri; Headers = $headers; ContentType = 'application/json' }
    if ($null -ne $Body) { $params.Body = ($Body | ConvertTo-Json -Depth 10) }
    Invoke-RestMethod @params
}

# --- 1. Entra identity (Bicep) -------------------------------------------------------------------
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

# --- 2. Azure DevOps orgs ------------------------------------------------------------------------
function Set-OrgAccess {
    param([string] $Org, [string] $Project)
    Write-Host "== Binding identity to https://dev.azure.com/$Org/"

    # Add the service principal to the org (idempotent: returns the existing entitlement).
    $entitlement = Invoke-Ado -Method POST -Uri "https://vsaex.dev.azure.com/$Org/_apis/serviceprincipalentitlements?api-version=7.1-preview.1" -Body @{
        accessLevel      = @{ accountLicenseType = 'express' }
        servicePrincipal = @{ origin = 'aad'; originId = $spObjectId; subjectKind = 'servicePrincipal' }
    }
    $spDescriptor = $entitlement.operationResult.result.servicePrincipal.descriptor
    if (-not $spDescriptor) { $spDescriptor = $entitlement.servicePrincipal.descriptor }
    if (-not $spDescriptor) { throw "Could not add the service principal to $Org." }

    # Project Collection Administrators membership (PUT is idempotent).
    $groups = Invoke-Ado -Uri "https://vssps.dev.azure.com/$Org/_apis/graph/groups?scopeDescriptor=&api-version=7.1-preview.1"
    $pca = $groups.value | Where-Object { $_.principalName -eq "[$Org]\Project Collection Administrators" } | Select-Object -First 1
    if (-not $pca) { throw "Project Collection Administrators group not found in $Org." }
    Invoke-Ado -Method PUT -Uri "https://vssps.dev.azure.com/$Org/_apis/graph/memberships/$spDescriptor/$($pca.descriptor)?api-version=7.1-preview.1" | Out-Null
    Write-Host "   member of Project Collection Administrators"

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

Set-OrgAccess -Org $SourceOrg -Project $SourceProject
Set-OrgAccess -Org $TargetOrg -Project $TargetProject

# --- 3. GitHub environments and variables --------------------------------------------------------
if (-not $SkipGitHub) {
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

Write-Host "== Done. Run the 'Integration tests (Azure DevOps)' workflow to validate."
