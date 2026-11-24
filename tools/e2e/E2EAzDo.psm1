<#
.SYNOPSIS
    REST helpers for seeding and cleaning up end-to-end scenarios in the test organisations.

.DESCRIPTION
    Signs in like Invoke-MigrationE2E.ps1: Microsoft Entra ID through 'az login' by default, or AMT_TEST_PAT if set.
    The token is held in memory only, never printed, and only sent to the allowed test organisations.
    Only projects and processes whose names start with "amte2e-" or "amt<issue>-" can be created or deleted through this module.
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

# Workers fixing an issue use their own amt<issue>-* scratch objects so they never collide with amte2e-* or each other.
function Assert-E2EName([string]$Name) {
    if (-not ($Name.StartsWith($script:Prefix) -or $Name -match '^amt\d+-')) { throw "Refusing to touch '$Name': only '$($script:Prefix)*' or 'amt<issue>-*' objects are allowed." }
}

function Invoke-E2ERest {
    param(
        [Parameter(Mandatory)][string]$Path,
        [string]$Method = "GET",
        $Body,
        [string]$ContentType = "application/json",
        [string]$Org = $script:DefaultOrg,
        # Only Azure DevOps hosts: the request carries the credential.
        [ValidateSet("dev.azure.com", "vsrm.dev.azure.com", "vsaex.dev.azure.com", "vssps.dev.azure.com")][string]$Host_ = "dev.azure.com",
        [string]$ApiVersion = "7.1"
    )
    if ($script:AllowedOrgs -notcontains $Org) { throw "Organisation '$Org' is not a test organisation." }
    if (-not $script:Headers) { Initialize-E2E }
    $sep = if ($Path.Contains("?")) { "&" } else { "?" }
    $uri = "https://$Host_/$Org/$($Path.TrimStart('/'))$sep" + "api-version=$ApiVersion"
    $params = @{ Uri = $uri; Method = $Method; Headers = $script:Headers; ContentType = $ContentType }
    if ($null -ne $Body) {
        # Assigned directly: returning a byte[] through an if-expression would unroll it and send it as text.
        if ($Body -is [string] -or $Body -is [byte[]]) { $params.Body = $Body } else { $params.Body = $Body | ConvertTo-Json -Depth 20 }
    }
    Invoke-RestMethod @params
}

function Wait-E2EOperation($op, [int]$TimeoutSeconds = 600) {
    if (-not $op.url) { return $op }
    # The operation url comes from the server response; only send the credential back to an allowed organisation.
    $u = [Uri]$op.url
    if ($u.Scheme -ne "https" -or $u.Host -ne "dev.azure.com" -or $script:AllowedOrgs -notcontains $u.Segments[1].TrimEnd('/')) { throw "Refusing to poll operation url outside the test organisations: $($op.url)" }
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        if ((Get-Date) -gt $deadline) { throw "Operation $($op.id) did not finish within $TimeoutSeconds seconds." }
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
    if (-not $parentId) { throw "Parent process '$Parent' not found in $Org." }
    # No referenceName: a deleted process keeps its reference name reserved, so let the service generate a unique one.
    (Invoke-E2ERest "_apis/work/processes" -Org $Org -Method POST -Body @{ name = $Name; parentProcessTypeId = $parentId }).typeId
}

function Remove-E2EProcess([string]$Name, [string]$Org = $script:DefaultOrg) {
    Assert-E2EName $Name
    $id = Get-E2EProcessId $Name $Org
    if ($id) { Invoke-E2ERest "_apis/work/processes/$id" -Org $Org -Method DELETE | Out-Null }
}

# Projects come from the scratch project pool (E2EPool.ps1, #3342): deleted projects count toward the org's
# 1,000-project limit for about 28 days, so the harness never creates or deletes one. New-E2EProject takes a free
# pool project, clears it, puts it on $Process and renames it $Name; Remove-E2EProject gives it back.
. (Join-Path $PSScriptRoot "E2EPool.ps1")

function New-E2EProject {
    param([Parameter(Mandatory)][string]$Name, [string]$Process = "amte2e-agile", [string]$Org = $script:DefaultOrg)
    # The pool's projects all derive from Agile; plain "Agile" (the old default) means the shared amte2e-agile.
    if ($Process -eq "Agile") { $Process = "amte2e-agile" }
    $p = Request-E2EPoolProject -Name $Name -Process $Process -Org $Org
    # Classification nodes can lag a rename; wait until the project answers by its new name.
    for ($i = 0; ; $i++) {
        try { Invoke-E2ERest "$Name/_apis/wit/classificationnodes" -Org $Org | Out-Null; return $p }
        catch { if ($i -ge 19) { throw }; Start-Sleep -Seconds 3 }
    }
}

function Remove-E2EProject([string]$Name, [string]$Org = $script:DefaultOrg) {
    Reset-E2EPoolProject -Name $Name -Org $Org | Out-Null
}

# -Prefix selects one issue's scratch objects (for example "amt3242-"); it must itself pass Assert-E2EName.
function Get-E2EProjects([string]$Org = $script:DefaultOrg, [string]$Prefix = $script:Prefix) { Assert-E2EName $Prefix; (Invoke-E2ERest "_apis/projects?`$top=500" -Org $Org).value | Where-Object { $_.name.StartsWith($Prefix) } }
function Get-E2EProcesses([string]$Org = $script:DefaultOrg, [string]$Prefix = $script:Prefix) { Assert-E2EName $Prefix; (Invoke-E2ERest "_apis/work/processes" -Org $Org).value | Where-Object { $_.name.StartsWith($Prefix) } }

# Which Custom.amte2e* fields a cleanup may delete. Every field a scenario creates is named amte2e<issue>..., so an issue's
# prefix (amt<issue>- or amte2e-<issue>-) owns exactly the fields carrying that number, and not those of a longer number
# that starts with it. -All owns every e2e field. Any other prefix owns no fields ($null). A prefix that every scratch
# project name starts with would make a scoped cleanup a full one, so it is refused.
function Get-E2EFieldFilter([string]$Prefix, [switch]$All) {
    if ($All) {
        if ($Prefix) { throw "Use -All or -Prefix, not both." }
        return '^Custom\.amte2e'
    }
    if (-not $Prefix -or $script:Prefix.StartsWith($Prefix)) { throw "Refusing prefix '$Prefix': it matches every e2e object. Use -All for a full clean." }
    if ($Prefix -match '^amt(\d+)-$' -or $Prefix -match '^amte2e-(\d+)-$') { return "^Custom\.amte2e$($Matches[1])(?!\d)" }
    $null
}

# Creates or updates a work item. $Fields is an ordered hashtable of reference name -> value.
# Extra JSON-patch operations (relations, etc.) can be passed in $Ops.
function Set-E2EWorkItem {
    param([string]$Project, [string]$Type, [int]$Id, [System.Collections.IDictionary]$Fields = @{}, [object[]]$Ops = @(), [switch]$BypassRules, [string]$Org = $script:DefaultOrg)
    if ($Id) {
        # Updates go to the organisation-wide endpoint, so check the item belongs to a scratch project first.
        $existing = Invoke-E2ERest "_apis/wit/workitems/$Id`?fields=System.TeamProject" -Org $Org
        Assert-E2EName $existing.fields.'System.TeamProject'
    } else { Assert-E2EName $Project }
    $patch =@(foreach ($k in $Fields.Keys) { @{ op = "add"; path = "/fields/$k"; value = $Fields[$k] } }) + $Ops
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
    # The work items list API takes at most 200 ids per request.
    $ids = @($ids)
    for ($i = 0; $i -lt $ids.Count; $i += 200) {
        $batch = $ids[$i..([Math]::Min($i + 199, $ids.Count - 1))]
        (Invoke-E2ERest "_apis/wit/workitems?ids=$($batch -join ',')&`$expand=relations" -Org $Org).value
    }
}

function Get-E2ERevisions([int]$Id, [string]$Org = $script:DefaultOrg) { (Invoke-E2ERest "_apis/wit/workitems/$Id/revisions?`$expand=relations" -Org $Org).value }

function New-E2EAttachment([string]$FileName, [byte[]]$Bytes, [string]$Org = $script:DefaultOrg) {
    Invoke-E2ERest "_apis/wit/attachments?fileName=$([Uri]::EscapeDataString($FileName))" -Org $Org -Method POST -Body $Bytes -ContentType "application/octet-stream"
}

# Invoke-RestMethod decodes a binary response as text, so attachment bytes are read with Invoke-WebRequest.
# Takes the attachment url as it appears in a work item, so the project segment (when present) is part of the request.
function Get-E2EAttachmentBytes([string]$Url) {
    $u = [Uri]$Url
    $org = $u.AbsolutePath.Trim('/').Split('/')[0]
    if ($u.Scheme -ne "https" -or $u.Host -ne "dev.azure.com" -or $script:AllowedOrgs -notcontains $org -or $u.AbsolutePath -notmatch '/_apis/wit/attachments/[0-9a-fA-F-]{36}$') { throw "Not an attachment url in a test organisation: $Url" }
    if (-not $script:Headers) { Initialize-E2E }
    $sep = if ($u.Query) { "&" } else { "?" }
    $response = Invoke-WebRequest -Uri "$($u.GetLeftPart([UriPartial]::Query))$sep`api-version=7.1" -Headers $script:Headers
    [byte[]]$response.RawContentStream.ToArray()
}

Export-ModuleMember -Function *-E2E*, Initialize-E2E, Assert-E2EName, Get-E2EPoolInventory
