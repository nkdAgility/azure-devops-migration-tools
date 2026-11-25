#Requires -Version 7
<#
.SYNOPSIS
    Builds the scratch project pool (#3342) from the scratch projects already in the test organisations.
.DESCRIPTION
    Renames each eligible live scratch project to amte2e-pool-NN, filling the lowest free numbers. It creates no project
    and deletes nothing, so the org's project count never changes. A project's process is left as it is; the first take
    switches it.

    Eligible: the name passes the harness name guard (amte2e-* or amt<issue>-*, so never migrationSource1 or
    migrationTarget1), it is not already a pool project, it has no lease team, and nothing in it changed in the last
    -MinIdleHours (the project, its newest work item, its newest push), unless -Include names it (its owner has said it
    may go into the pool now). Every skipped project is listed with its reason.
    Safe to run again: a second run finds nothing new to rename.

    Exit 0 when every org's live project count is the same afterwards and nothing failed; 1 otherwise.
.EXAMPLE
    ./tools/e2e/Initialize-E2EPool.ps1 -WhatIf
#>
[CmdletBinding(SupportsShouldProcess)]
param([string[]]$Organisations = @("nkdagility-adomt-source", "nkdagility-adomt-target"), [int]$Size = 0, [int]$MinIdleHours = 24,
    # Wildcard patterns for projects whose owner has said they may be adopted now, whatever their last change.
    # They must still pass the name guard and carry no lease.
    [string[]]$Include = @())
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1") -Force -WarningAction SilentlyContinue

function Get-LastChange($Project, [string]$Org) {
    $dates = @([datetime]$Project.lastUpdateTime)
    $wi = @((Invoke-E2ERest "$($Project.id)/_apis/wit/wiql?`$top=1" -Org $Org -Method POST -Body @{ query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project ORDER BY [System.ChangedDate] DESC" }).workItems)
    if ($wi.Count) { $dates += [datetime](Invoke-E2ERest "_apis/wit/workitems/$($wi[0].id)?fields=System.ChangedDate" -Org $Org).fields.'System.ChangedDate' }
    foreach ($r in @((Invoke-E2ERest "$($Project.id)/_apis/git/repositories" -Org $Org).value)) {
        $push = @((Invoke-E2ERest "$($Project.id)/_apis/git/repositories/$($r.id)/pushes?`$top=1" -Org $Org).value)
        if ($push.Count) { $dates += [datetime]$push[0].date }
    }
    ($dates | ForEach-Object { $_.ToUniversalTime() } | Measure-Object -Maximum).Maximum
}

# projectprocessmigration only moves a project between processes with the same parent, and the pool's base process,
# amte2e-agile, inherits from Agile.
$agile = "adcc42ab-9882-485e-a3ed-7678f01f66bc"
function Test-AgileBased($Project, [string]$Org) {
    $id = Get-E2EProjectProcessId $Project.id $Org
    if ($id -eq $agile) { return $true }
    $proc = try { Invoke-E2ERest "_apis/work/processes/$id" -Org $Org } catch { $null }
    $proc -and $proc.parentProcessTypeId -eq $agile
}

$failed = $false
foreach ($org in $Organisations) {
    $all = @((Invoke-E2ERest "_apis/projects?`$top=1000" -Org $org).value)
    $before = $all.Count
    $taken = @($all | ForEach-Object { Get-E2EPoolSlot $_.name } | Where-Object { $_ })
    $renamed = 0
    foreach ($p in $all | Sort-Object name) {
        $why = $null
        if ($p.name -in "migrationSource1", "migrationTarget1") { $why = "used by the automated tests" }
        elseif (-not (Test-E2EGuardedName $p.name)) { $why = "outside the harness name guard" }
        elseif (Get-E2EPoolSlot $p.name) { continue }
        elseif ($lease = Get-E2EPoolLease $p.id $org) { $why = "leased by $($lease.Token)" }
        elseif (-not (Test-AgileBased $p $org)) { $why = "its process is not Agile or inherited from Agile, so a take could never switch it" }
        elseif (-not @($Include | Where-Object { $p.name -like $_ }).Count) {
            $last = Get-LastChange $p $org
            if ($last -gt [datetime]::UtcNow.AddHours(-$MinIdleHours)) { $why = "changed in the last $MinIdleHours hours ($($last.ToString('o')))" }
        }
        if ($why) { "skipped $org/$($p.name): $why"; continue }
        if ($Size -and ($taken.Count + $renamed) -ge $Size) { "skipped $org/$($p.name): the pool already has $Size projects"; continue }

        $slot = 1; while ($taken -contains $slot) { $slot++ }
        if ($slot -gt 99) { "skipped $org/$($p.name): no free pool number"; continue }
        $new = Format-E2EPoolName $slot
        if (-not $PSCmdlet.ShouldProcess("$org/$($p.name)", "Rename to $new")) { "would rename $org/$($p.name) -> $org/$new"; $taken += $slot; continue }
        # A rename needs the claim like any other change, so two adopters (or an adopt and a take) never clash.
        if (-not (New-E2EPoolLease $p.id $slot $org)) { "skipped $org/$($p.name): claimed by someone else just now"; continue }
        try {
            Rename-E2EPoolProject $p.id $new $org
            Remove-E2EPoolLease $p.id $org
            "renamed $org/$($p.name) -> $org/$new"
            $taken += $slot; $renamed++
        }
        catch { "FAILED $org/$($p.name): $($_.Exception.Message)"; try { Remove-E2EPoolLease $p.id $org } catch {}; $failed = $true }
    }
    $after = @((Invoke-E2ERest "_apis/projects?`$top=1000" -Org $org).value).Count
    $pool = @(Get-E2EPoolProject -Org $org).Count
    "pool in $($org): $pool projects, live $before -> $after"
    if ($after -ne $before) { $failed = $true }
}
if ($failed) { exit 1 }
exit 0
