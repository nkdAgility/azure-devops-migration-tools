<#
.SYNOPSIS
    Deletes one issue's e2e scratch projects, processes and Custom.amte2e<issue>* fields from the test organisations, or,
    with -All, every amte2e-* project and process and every Custom.amte2e* field. Reports leftovers.
.DESCRIPTION
    -Prefix amt<issue>- or amte2e-<issue>- removes that issue's projects and processes and its Custom.amte2e<issue>* fields.
    Any other -Prefix (for example amte2e-fields-) removes its projects and processes but no fields, because no field can be
    tied to it, and exits 1. A prefix that matches every e2e object is refused; use -All for that. -WhatIf lists what would
    be deleted and deletes nothing.

    Order matters: projects first (processes in use can't be deleted), then processes, then fields (fields in use by a process
    can't be deleted). Deleted projects go to the organisation's recycle bin; they are then destroyed so the processes they used
    can be deleted too.
.EXAMPLE
    ./tools/e2e/Remove-E2EObjects.ps1 -Prefix amte2e-3278- -WhatIf
.EXAMPLE
    ./tools/e2e/Remove-E2EObjects.ps1 -All
#>
[CmdletBinding(SupportsShouldProcess)]
param([string[]]$Organisations = @("nkdagility-adomt-source", "nkdagility-adomt-target"), [string]$Prefix, [switch]$All, [switch]$ClearPool)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1") -Force -WarningAction SilentlyContinue
Import-Module (Join-Path $PSScriptRoot "E2EProcess.psm1") -Force

if ($ClearPool) {
    if ($All -or $Prefix) { throw "Use -ClearPool on its own: it gives back every leased pool project and touches no process or field." }
    foreach ($org in $Organisations) {
        foreach ($h in @(Get-E2EPoolProject -Org $org -All | Where-Object Lease)) {
            if (-not $PSCmdlet.ShouldProcess("$org/$($h.Name)", "Release pool project (held by $($h.Lease.Token))")) { "would release $org/$($h.Name) -> $(Format-E2EPoolName $h.Slot) (held by $($h.Lease.Token), until $($h.Lease.Expires.ToString('o')))"; continue }
            "releasing $org/$($h.Name), held by $($h.Lease.Token)"
            try { $slot = Reset-E2EPoolProject -Name $h.Name -Org $org -Force; "released $org/$($h.Name) -> $slot (cleared)" } catch { "NOT RELEASED $org/$($h.Name): $($_.Exception.Message)"; $failedRelease = $true }
        }
        "pool in $($org): $(@(Get-E2EPoolProject -Org $org).Count) free"
    }
    if ($failedRelease) { exit 1 }
    exit 0
}
if (-not $All -and -not $Prefix) { throw "Give -Prefix amt<issue>- (or amte2e-<issue>-) to clean one issue, -All for every e2e object, or -ClearPool to give back every leased pool project." }
$fieldFilter = Get-E2EFieldFilter -Prefix $Prefix -All:$All
if ($All) { $Prefix = "amte2e-" }
Assert-E2EName $Prefix
"field filter: " + $(if ($fieldFilter) { $fieldFilter } else { "none (prefix '$Prefix' owns no fields)" })

$verb = if ($WhatIfPreference) { "would delete" } else { "deleted" }
function Get-ScopedFields([string]$Org) { if ($fieldFilter) { @(Get-E2EFields -Org $Org) | Where-Object { $_.referenceName -match $fieldFilter } } }
$leftovers = @()
foreach ($org in $Organisations) {
    Set-E2EOrg $org
    # Projects are never deleted (#3342): a deleted project counts toward the org's 1,000-project limit for about 28 days.
    # Pool projects leased under this prefix are given back (cleared, back on amte2e-agile, renamed amte2e-pool-NN), so
    # the processes they used can be deleted below. Any other project of the prefix is left and reported.
    foreach ($h in @(Get-E2EPoolProject -Org $org -All | Where-Object { $_.Lease -and ($ClearPool -or $_.Name.StartsWith($Prefix)) })) {
        if (-not $PSCmdlet.ShouldProcess("$org/$($h.Name)", "Release pool project (held by $($h.Lease.Token))")) { "would release $org/$($h.Name) -> $(Format-E2EPoolName $h.Slot) (held by $($h.Lease.Token))"; continue }
        try { $slot = Reset-E2EPoolProject -Name $h.Name -Org $org -Force; "released $org/$($h.Name) -> $slot (cleared)" } catch { $leftovers += "pool project $org/$($h.Name): $($_.Exception.Message)" }
    }
    foreach ($p in @(Get-E2EProjects -Org $org -Prefix $Prefix | Where-Object { -not (Get-E2EPoolSlot $_.name) -and -not (Get-E2EPoolLease $_.id $org) })) {
        "kept project $org/$($p.name): not a pool project; projects are never deleted (adopt it with Initialize-E2EPool.ps1)"
    }
    # Destroy scratch projects waiting in the recycle bin so their processes are no longer referenced.
    try {
        $bin = (Invoke-E2ERest "_apis/projects?stateFilter=deleted&`$top=500" -Org $org).value | Where-Object { $_.name.StartsWith($Prefix) }
        foreach ($p in $bin) {
            if (-not $PSCmdlet.ShouldProcess("$org/$($p.name)", "Destroy recycled project")) { "$verb recycled project $org/$($p.name)"; continue }
            try { Wait-E2EOperation (Invoke-E2ERest "_apis/projects/$($p.id)?hardDelete=true" -Org $org -Method DELETE -ApiVersion "7.1-preview.4") | Out-Null; "destroyed recycled project $org/$($p.name)" }
            catch { $leftovers += "project $org/$($p.name): soft-deleted, in the recycle bin until Azure DevOps purges it ($(($_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue).message))" }
        }
    } catch { "recycle bin not listed for $($org): $($_.Exception.Message)" }
    foreach ($pr in @(Get-E2EProcesses -Org $org -Prefix $Prefix)) {
        if (-not $PSCmdlet.ShouldProcess("$org/$($pr.name)", "Delete process")) { "$verb process $org/$($pr.name)"; continue }
        # Projects deleted moments ago can still hold the process, so retry before reporting it.
        for ($try = 1; ; $try++) {
            try { Remove-E2EProcess $pr.name -Org $org; "deleted process $org/$($pr.name)"; break }
            catch { if ($try -ge 4) { $leftovers += "process $org/$($pr.name): $(($_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue).message)"; break }; Start-Sleep -Seconds 15 }
        }
    }
    foreach ($f in @(Get-ScopedFields $org)) {
        if (-not $PSCmdlet.ShouldProcess("$org/$($f.referenceName)", "Delete field")) { "$verb field $org/$($f.referenceName)"; continue }
        try { Remove-E2EField $f.referenceName -Org $org; "deleted field $org/$($f.referenceName)" } catch { $leftovers += "field $org/$($f.referenceName): $(($_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue).message)" }
    }
    if (-not $WhatIfPreference) {
        $remaining = @(Get-E2EProjects -Org $org -Prefix $Prefix | Where-Object { -not (Get-E2EPoolSlot $_.name) }).name + @(Get-E2EProcesses -Org $org -Prefix $Prefix).name + @(Get-ScopedFields $org).referenceName | Where-Object { $_ }
        "remaining in $($org): " + $(if ($remaining) { $remaining -join ", " } else { "none" })
    }
}
if ($leftovers) { "LEFTOVERS:"; $leftovers | ForEach-Object { "  $_" } }
if (-not $fieldFilter) { Write-Warning "Cannot map prefix '$Prefix' to a field filter; no fields were deleted."; exit 1 }
