<#
.SYNOPSIS
    REST helpers for seeding and cleaning up end-to-end scenarios in the test organisations.

.DESCRIPTION
    Signs in like Invoke-MigrationE2E.ps1: Microsoft Entra ID through 'az login' by default, or AMT_TEST_PAT if set.
    The token is held in memory only, never printed, and only sent to the allowed test organisations.
    Only projects and processes whose names start with "amte2e-" can be created or deleted through this module.
#>
$script:AllowedOrgs = @("nkdagility-adomt-source", "nkdagility-adomt-target", "nkdagility-preview")
$script:Prefix = "amte2e-"
$script:Headers = $null
$script:DefaultOrg = "nkdagility-adomt-source"

function Initialize-E2E {
    if ($env:AMT_TEST_PAT) {
        $b64 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($env:AMT_TEST_PAT)"))
        $script:Headers = @{ Authorization = "Basic $b64" }
        return
    }
    # 499b84ac-... is the Azure DevOps resource id.
    $tok = az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv 2>$null
    if (-not $tok) { throw "No Entra token. Run 'az login' as an identity with access to the test organisations, or set AMT_TEST_PAT." }
    $script:Headers = @{ Authorization = "Bearer $tok" }
}

function Set-E2EOrg([string]$Org) {
    if ($script:AllowedOrgs -notcontains $Org) { throw "Organisation '$Org' is not a test organisation." }
    $script:DefaultOrg = $Org
}

function Assert-E2EName([string]$Name) {
    if (-not $Name.StartsWith($script:Prefix)) { throw "Refusing to touch '$Name': only '$($script:Prefix)*' objects are allowed." }
}

function Invoke-E2ERest {
    param(
        [Parameter(Mandatory)][string]$Path,
        [string]$Method = "GET",
        $Body,
        [string]$ContentType = "application/json",
        [string]$Org = $script:DefaultOrg,
        [string]$Host_ = "dev.azure.com",
        [string]$ApiVersion = "7.1"
    )
    if ($script:AllowedOrgs -notcontains $Org) { throw "Organisation '$Org' is not a test organisation." }
    if (-not $script:Headers) { Initialize-E2E }
    $sep = if ($Path.Contains("?")) { "&" } else { "?" }
    $uri = "https://$Host_/$Org/$($Path.TrimStart('/'))$sep" + "api-version=$ApiVersion"
    $params = @{ Uri = $uri; Method = $Method; Headers = $script:Headers; ContentType = $ContentType }
    if ($null -ne $Body) {
        $params.Body = if ($Body -is [string] -or $Body -is [byte[]]) { $Body } else { $Body | ConvertTo-Json -Depth 20 }
    }
    Invoke-RestMethod @params
}

function Wait-E2EOperation($op) {
    if (-not $op.url) { return $op }
    do {
        Start-Sleep -Seconds 3
        $s = Invoke-RestMethod -Uri $op.url -Headers $script:Headers
    } while ($s.status -in "notSet", "queued", "inProgress")
    if ($s.status -ne "succeeded") { throw "Operation failed: $($s.status) $($s.resultMessage)" }
    $s
}

function Get-E2EProcessId([string]$Name, [string]$Org = $script:DefaultOrg) {
    (Invoke-E2ERest "_apis/work/processes" -Org $Org).value | Where-Object name -eq $Name | Select-Object -ExpandProperty typeId -First 1
}

function New-E2EProcess {
    param([Parameter(Mandatory)][string]$Name, [string]$Parent = "Agile", [string]$Org = $script:DefaultOrg)
    Assert-E2EName $Name
    $existing = Get-E2EProcessId $Name $Org
    if ($existing) { return $existing }
    $parentId = Get-E2EProcessId $Parent $Org
    (Invoke-E2ERest "_apis/work/processes" -Org $Org -Method POST -Body @{ name = $Name; parentProcessTypeId = $parentId; referenceName = ($Name -replace '[^A-Za-z0-9]', '') }).typeId
}

function Remove-E2EProcess([string]$Name, [string]$Org = $script:DefaultOrg) {
    Assert-E2EName $Name
    $id = Get-E2EProcessId $Name $Org
    if ($id) { Invoke-E2ERest "_apis/work/processes/$id" -Org $Org -Method DELETE | Out-Null }
}

function New-E2EProject {
    param([Parameter(Mandatory)][string]$Name, [string]$Process = "Agile", [string]$Org = $script:DefaultOrg)
    Assert-E2EName $Name
    $p = try { Invoke-E2ERest "_apis/projects/$Name" -Org $Org } catch { $null }
    if ($p) { return $p }
    $op = Invoke-E2ERest "_apis/projects" -Org $Org -Method POST -Body @{
        name = $Name; description = "AMT e2e regression scratch project"
        capabilities = @{ versioncontrol = @{ sourceControlType = "Git" }; processTemplate = @{ templateTypeId = (Get-E2EProcessId $Process $Org) } }
    }
    Wait-E2EOperation $op | Out-Null
    Invoke-E2ERest "_apis/projects/$Name" -Org $Org
}

function Remove-E2EProject([string]$Name, [string]$Org = $script:DefaultOrg) {
    Assert-E2EName $Name
    $p = try { Invoke-E2ERest "_apis/projects/$Name" -Org $Org } catch { $null }
    if ($p) { Wait-E2EOperation (Invoke-E2ERest "_apis/projects/$($p.id)" -Org $Org -Method DELETE) | Out-Null }
}

function Get-E2EProjects([string]$Org = $script:DefaultOrg) { (Invoke-E2ERest "_apis/projects?`$top=500" -Org $Org).value | Where-Object { $_.name.StartsWith($script:Prefix) } }
function Get-E2EProcesses([string]$Org = $script:DefaultOrg) { (Invoke-E2ERest "_apis/work/processes" -Org $Org).value | Where-Object { $_.name.StartsWith($script:Prefix) } }

# Creates or updates a work item. $Fields is an ordered hashtable of reference name -> value.
# Extra JSON-patch operations (relations, etc.) can be passed in $Ops.
function Set-E2EWorkItem {
    param([string]$Project, [string]$Type, [int]$Id, [System.Collections.IDictionary]$Fields = @{}, [object[]]$Ops = @(), [switch]$BypassRules, [string]$Org = $script:DefaultOrg)
    if (-not $Id) { Assert-E2EName $Project }
    $patch = @(foreach ($k in $Fields.Keys) { @{ op = "add"; path = "/fields/$k"; value = $Fields[$k] } }) + $Ops
    $q = if ($BypassRules) { "?bypassRules=true&suppressNotifications=true" } else { "?suppressNotifications=true" }
    $path = if ($Id) { "_apis/wit/workitems/$Id$q" } else { "$Project/_apis/wit/workitems/`$$([Uri]::EscapeDataString($Type))$q" }
    $method = if ($Id) { "PATCH" } else { "POST" }
    Invoke-E2ERest $path -Org $Org -Method $method -Body (ConvertTo-Json @($patch) -Depth 20) -ContentType "application/json-patch+json"
}

function Get-E2EWiql([string]$Project, [string]$Wiql, [string]$Org = $script:DefaultOrg) {
    Assert-E2EName $Project
    (Invoke-E2ERest "$Project/_apis/wit/wiql" -Org $Org -Method POST -Body @{ query = $Wiql }).workItems
}

function Get-E2EAllWorkItems([string]$Project, [string]$Org = $script:DefaultOrg) {
    $ids = Get-E2EWiql $Project "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = '$Project' ORDER BY [System.Id]" $Org | ForEach-Object id
    if (-not $ids) { return @() }
    (Invoke-E2ERest "_apis/wit/workitems?ids=$($ids -join ',')&`$expand=relations" -Org $Org).value
}

function Get-E2ERevisions([int]$Id, [string]$Org = $script:DefaultOrg) { (Invoke-E2ERest "_apis/wit/workitems/$Id/revisions?`$expand=relations" -Org $Org).value }

function New-E2EAttachment([string]$FileName, [byte[]]$Bytes, [string]$Org = $script:DefaultOrg) {
    Invoke-E2ERest "_apis/wit/attachments?fileName=$([Uri]::EscapeDataString($FileName))" -Org $Org -Method POST -Body $Bytes -ContentType "application/octet-stream"
}

Export-ModuleMember -Function *-E2E*, Initialize-E2E, Assert-E2EName
