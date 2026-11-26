#Requires -Version 7
<#
.SYNOPSIS
    The scratch project pool (#3342). Scenarios take a fixed pool project instead of creating one, and give it back
    instead of deleting it, because deleted projects count toward the org's 1,000-project limit for about 28 days.

.DESCRIPTION
    A free pool project is named amte2e-pool-NN and is on amte2e-agile. Taking one claims it by creating the team
    amte2e-pool-lease in it: the service refuses a second team of that name, so exactly one taker wins, on any machine.
    The team description holds the claim (slot, machine, owner pid, run, expiry). The taker clears the project, switches
    it to the requested process and renames it to the requested name. Releasing clears it, switches it back, renames it
    back and deletes the lease team last. No project is ever created or deleted here.

    Every project name passes Assert-E2EName before it is touched, so migrationSource1/migrationTarget1 never are.
    Dot-sourced by E2EAzDo.psm1 (so it shares its session state, the token and the guard), whose Invoke-E2ERest, Assert-E2EName, Wait-E2EOperation and Get-E2EProcessId it uses.
#>
$script:PoolPrefix = "amte2e-pool-"
$script:LeaseTeam = "amte2e-pool-lease"
$script:LeaseHours = 2
$script:BaseProcess = "amte2e-agile"

# ---------------------------------------------------------------- owner and lease

# The owner is the proof (AMTE2E_POOL_OWNER = "<pid>:<run>"), so seeds run as child processes share its claims.
# Without it, this process makes a run id and sets the variable, so its own children share it too. Its own pid is
# the owner pid, so once it exits its leases read as Orphaned and a later clean-up on this machine can give them back.
function Get-E2EPoolOwner {
    if (-not $env:AMTE2E_POOL_OWNER) { $env:AMTE2E_POOL_OWNER = "${PID}:" + [Guid]::NewGuid().ToString("N").Substring(0, 12) }
    $ownerPid, $run = $env:AMTE2E_POOL_OWNER.Split(":", 2)
    [pscustomobject]@{ Machine = [Environment]::MachineName; Pid = $ownerPid; Run = $run; Token = "$([Environment]::MachineName)|$ownerPid|$run" }
}

function ConvertTo-E2EPoolLease([int]$Slot, [datetime]$Claimed, [datetime]$Expires, [string]$Item = "") {
    $o = Get-E2EPoolOwner
    "slot={0:D2};machine={1};pid={2};run={3};item={4};claimed={5};expires={6}" -f $Slot, $o.Machine, $o.Pid, $o.Run, $Item, $Claimed.ToUniversalTime().ToString("o"), $Expires.ToUniversalTime().ToString("o")
}

function ConvertFrom-E2EPoolLease([string]$Text) {
    if (-not $Text) { return $null }
    $h = @{}
    foreach ($kv in $Text.Split(";")) { $k, $v = $kv.Split("=", 2); if ($k) { $h[$k.Trim()] = "$v".Trim() } }
    # A value that does not parse makes the whole text unreadable ($null), never an error: one bad lease must not stop
    # the scan of the others. Get-E2EPoolLease then treats it as an unreadable lease.
    $slot = 0; $expires = [datetime]::MinValue; $claimed = [datetime]::UtcNow
    if (-not [int]::TryParse("$($h.slot)", [ref]$slot) -or $slot -le 0) { return $null }
    if (-not [datetime]::TryParse("$($h.expires)", $null, "RoundtripKind", [ref]$expires)) { return $null }
    if ($h.claimed -and -not [datetime]::TryParse("$($h.claimed)", $null, "RoundtripKind", [ref]$claimed)) { return $null }
    [pscustomobject]@{
        Slot = $slot; Machine = $h.machine; Pid = $h.pid; Run = $h.run; Item = $h.item
        Claimed = $claimed.ToUniversalTime()
        Expires = $expires.ToUniversalTime()
        Token = "$($h.machine)|$($h.pid)|$($h.run)"
    }
}

# Free, Held, Expired or Orphaned (this machine, owner pid set and no longer running).
function Get-E2EPoolLeaseState($Lease, [datetime]$Now = [datetime]::UtcNow) {
    if (-not $Lease) { return "Free" }
    if ($Lease.Expires -lt $Now) { return "Expired" }
    $ownerPid = 0
    if ($Lease.Machine -eq [Environment]::MachineName -and [int]::TryParse("$($Lease.Pid)", [ref]$ownerPid) -and -not (Get-Process -Id $ownerPid -ErrorAction SilentlyContinue)) { return "Orphaned" }
    "Held"
}

function Get-E2EPoolLeaseTeam([string]$ProjectId, [string]$Org) {
    $r = Invoke-E2ERest "_apis/projects/$ProjectId/teams?`$top=500" -Org $Org
    if ($r -is [string]) { $r = $r | ConvertFrom-Json -AsHashtable }
    @($r.value) | Where-Object { $_.name -eq $script:LeaseTeam } | Select-Object -First 1
}

# The lease of a project, with the team behind it (Team) so a takeover can delete exactly that team.
# A lease team with an unreadable description counts as expired, so it cannot block a project for ever.
function Get-E2EPoolLease([string]$ProjectId, [string]$Org) {
    $team = Get-E2EPoolLeaseTeam $ProjectId $Org
    if (-not $team) { return $null }
    $lease = ConvertFrom-E2EPoolLease $team.description
    if (-not $lease) { $lease = [pscustomobject]@{ Slot = 0; Machine = ""; Pid = ""; Run = ""; Item = ""; Claimed = [datetime]::MinValue; Expires = [datetime]::MinValue; Token = "||" } }
    $lease | Add-Member -NotePropertyName Team -NotePropertyValue $team -PassThru -Force
}

# The re-read before every destructive step: stop if this run no longer holds the project.
# The teams list can briefly leave out a team that exists, so a missing lease is read again before it counts as lost.
function Assert-E2EPoolLeaseHeld([string]$ProjectId, [string]$Org, [int]$Reads = 4, [int]$WaitSeconds = 5) {
    $l = Get-E2EPoolLease $ProjectId $Org
    for ($i = 1; -not $l -and $i -lt $Reads; $i++) { Start-Sleep -Seconds $WaitSeconds; $l = Get-E2EPoolLease $ProjectId $Org }
    if (-not $l -or $l.Token -ne (Get-E2EPoolOwner).Token) { throw "Lease lost on project $ProjectId in ${Org}: it is now held by $(if ($l) { $l.Token } else { 'nobody' })." }
    $l
}

# An expired lease is never renewed: once it has expired another run may already be removing it, so its owner must
# stop rather than extend it. Only Expired and Orphaned leases are ever taken over, so a live owner's renewal never
# races a takeover.
function Set-E2EPoolLease([string]$ProjectId, [string]$Org, [switch]$Renew) {
    $l = Assert-E2EPoolLeaseHeld $ProjectId $Org
    if ($l.Expires -lt [datetime]::UtcNow) { throw "Lease lost on project $ProjectId in ${Org}: it expired at $($l.Expires.ToString('o')), so another run may take it." }
    $desc = ConvertTo-E2EPoolLease -Slot $l.Slot -Claimed $l.Claimed -Expires ([datetime]::UtcNow.AddHours($script:LeaseHours)) -Item $l.Item
    Invoke-E2ERest "_apis/projects/$ProjectId/teams/$($l.Team.id)" -Org $Org -Method PATCH -Body @{ description = $desc } | Out-Null
}

# Creating the lease team is the claim. Returns $true when this run won it.
function New-E2EPoolLease([string]$ProjectId, [int]$Slot, [string]$Org) {
    $now = [datetime]::UtcNow
    $desc = ConvertTo-E2EPoolLease -Slot $Slot -Claimed $now -Expires $now.AddHours($script:LeaseHours) -Item "$env:AMTE2E_POOL_ITEM"
    try { Invoke-E2ERest "_apis/projects/$ProjectId/teams" -Org $Org -Method POST -Body @{ name = $script:LeaseTeam; description = $desc } | Out-Null }
    catch { return $false }
    # The POST succeeded, so a lease team now exists; the teams list can lag it, so it is read until it shows one.
    # Another owner's lease means this run lost; a lease never seen is reported, since this run may hold it.
    for ($i = 0; $i -lt 4; $i++) {
        if ($i) { Start-Sleep -Seconds 5 }
        $l = Get-E2EPoolLease $ProjectId $Org
        if ($l) { return ($l.Token -eq (Get-E2EPoolOwner).Token) }
    }
    throw "Claimed project $ProjectId in $Org, but its lease team never showed in the teams list; release it with tools/e2e/Remove-E2EObjects.ps1 -ClearPool if it stays leased."
}

# Deletes the lease team only if it is still exactly the one that was read (no compare-and-delete exists, so re-read).
# The re-read must also still be stale (Expired or Orphaned), unless -EvenIfLive (the operator's -ClearPool).
function Remove-E2EPoolStaleLease([string]$ProjectId, $Lease, [string]$Org, [switch]$EvenIfLive) {
    $now = Get-E2EPoolLease $ProjectId $Org
    if (-not $now -or $now.Team.id -ne $Lease.Team.id -or $now.Team.description -ne $Lease.Team.description) { return $false }
    if (-not $EvenIfLive -and (Get-E2EPoolLeaseState $now) -notin "Expired", "Orphaned") { return $false }
    try { Invoke-E2ERest "_apis/projects/$ProjectId/teams/$($Lease.Team.id)" -Org $Org -Method DELETE | Out-Null; $true } catch { $false }
}

function Remove-E2EPoolLease([string]$ProjectId, [string]$Org) {
    $l = Get-E2EPoolLease $ProjectId $Org
    if ($l -and $l.Token -eq (Get-E2EPoolOwner).Token) { Invoke-E2ERest "_apis/projects/$ProjectId/teams/$($l.Team.id)" -Org $Org -Method DELETE | Out-Null }
}

# ---------------------------------------------------------------- listing

function Get-E2EPoolSlot([string]$Name) { if ($Name -match "^$([regex]::Escape($script:PoolPrefix))(\d{2})$") { [int]$Matches[1] } else { 0 } }
function Format-E2EPoolName([int]$Slot) { "{0}{1:D2}" -f $script:PoolPrefix, $Slot }

function Test-E2EGuardedName([string]$Name) { try { Assert-E2EName $Name; $true } catch { $false } }

function Get-E2EGuardedProjects([string]$Org) {
    @((Invoke-E2ERest "_apis/projects?`$top=1000" -Org $Org).value) | Where-Object { Test-E2EGuardedName $_.name }
}

# Pool projects with their lease and state. Free pool projects are found by name; leased ones carry the seed's name,
# so -All and -Owner read the lease of every guarded project. -Owner keeps those held by that run.
function Get-E2EPoolProject {
    param([Parameter(Mandatory)][string]$Org, [string]$Owner, [switch]$All)
    $projects = @(Get-E2EGuardedProjects $Org)
    if (-not $All -and -not $Owner) { $projects = @($projects | Where-Object { Get-E2EPoolSlot $_.name }) }
    foreach ($p in $projects) {
        $lease = Get-E2EPoolLease $p.id $Org
        $slot = if ($lease -and $lease.Slot) { $lease.Slot } else { Get-E2EPoolSlot $p.name }
        if (-not $slot) { continue }
        if ($Owner -and (-not $lease -or $lease.Run -ne $Owner)) { continue }
        [pscustomobject]@{ Org = $Org; Id = $p.id; Name = $p.name; Slot = $slot; Lease = $lease; State = Get-E2EPoolLeaseState $lease }
    }
}

# ---------------------------------------------------------------- clearing

function Get-E2EList([string]$Path, [string]$Org, [string]$HostName = "dev.azure.com", [string]$ApiVersion = "7.1") {
    @((Invoke-E2ERest $Path -Org $Org -Host_ $HostName -ApiVersion $ApiVersion).value) | Where-Object { $null -ne $_ }
}

function Get-E2EQueryItems([string]$ProjectId, [string]$Org) {
    foreach ($r in Get-E2EList "$ProjectId/_apis/wit/queries?`$depth=1" $Org) { foreach ($c in @($r.children)) { if ($c) { $c } } }
}

function Get-E2EChildNodes([string]$ProjectId, [string]$Group, [string]$Org) {
    $root = Invoke-E2ERest "$ProjectId/_apis/wit/classificationnodes/$Group`?`$depth=1" -Org $Org
    [pscustomobject]@{ Root = $root; Children = @($root.children | Where-Object { $_ }) }
}

function Get-E2EDashboards([string]$ProjectId, [string]$Org) {
    foreach ($t in Get-E2EList "_apis/projects/$ProjectId/teams?`$top=500" $Org) {
        foreach ($d in Get-E2EList "$ProjectId/$($t.id)/_apis/dashboard/dashboards" $Org -ApiVersion "7.1-preview.3") { $d | Add-Member -NotePropertyName TeamId -NotePropertyValue $t.id -PassThru -Force }
    }
}

function Get-E2EDashboardWidgets([string]$ProjectId, $Dashboard, [string]$Org) {
    @((Invoke-E2ERest "$ProjectId/$($Dashboard.TeamId)/_apis/dashboard/dashboards/$($Dashboard.id)" -Org $Org -ApiVersion "7.1-preview.3").widgets) | Where-Object { $null -ne $_ }
}

# Work items in the project, leftovers left out (FR-004b).
function Get-E2EProjectWorkItemIds([string]$ProjectId, [string]$Org) {
    @((Invoke-E2ERest "$ProjectId/_apis/wit/wiql?`$top=5000" -Org $Org -Method POST -Body @{ query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND $(Get-E2EPoolLeftoverFilter)" }).workItems | ForEach-Object id)
}

# Leftovers that are not as FR-004b allows: a leftover with a link or outside pool-leftovers, or any other item in it.
function Get-E2EPoolLeftoverProblems([string]$ProjectId, [string]$ProjectName, [string]$Org) {
    $area = "$ProjectName\$($script:LeftoverArea)"
    $ids = @((Invoke-E2ERest "$ProjectId/_apis/wit/wiql" -Org $Org -Method POST -Body @{ query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND NOT ($(Get-E2EPoolLeftoverFilter))" }).workItems | ForEach-Object id)
    foreach ($id in $ids) {
        $w = Invoke-E2ERest "_apis/wit/workitems/$id`?`$expand=relations" -Org $Org
        if ($w.fields.'System.AreaPath' -ne $area -or @($w.relations | Where-Object { $_ }).Count) { "leftover $id" }
    }
    if ((Get-E2EChildNodes $ProjectId Areas $Org).Children | Where-Object name -eq $script:LeftoverArea) {
        @((Invoke-E2ERest "$ProjectId/_apis/wit/wiql" -Org $Org -Method POST -Body @{ query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.AreaPath] UNDER '$area' AND $(Get-E2EPoolLeftoverFilter)" }).workItems | ForEach-Object { "item $($_.id) in $($script:LeftoverArea)" })
    }
}

# A count per surface, read by REST and independent of Clear-E2EPoolProject's own list. -1 means it could not be read.
# A cleared project is 0 everywhere: one repo with no refs, the default team, the lease team and default dashboards
# are what a fresh project has, so they are not counted.
function Get-E2EPoolInventory([string]$ProjectId, [string]$Org) {
    $p = Invoke-E2ERest "_apis/projects/$ProjectId" -Org $Org
    $count = { param($s) try { @(& $s).Count } catch { -1 } }
    $repos = try { @(Get-E2EList "$ProjectId/_apis/git/repositories" $Org) } catch { $null }
    [ordered]@{
        workItems          = & $count { Get-E2EProjectWorkItemIds $ProjectId $Org }
        recycleBin         = & $count { Get-E2EList "$ProjectId/_apis/wit/recyclebin" $Org }
        extraTeams         = & $count { Get-E2EList "_apis/projects/$ProjectId/teams?`$top=500" $Org | Where-Object { $_.id -ne $p.defaultTeam.id -and $_.name -ne $script:LeaseTeam } }
        areaNodes          = & $count { (Get-E2EChildNodes $ProjectId Areas $Org).Children | Where-Object name -ne $script:LeftoverArea }
        leftoverProblems   = & $count { Get-E2EPoolLeftoverProblems $ProjectId $p.name $Org }
        iterationNodes     = & $count { (Get-E2EChildNodes $ProjectId Iterations $Org).Children }
        queries            = & $count { Get-E2EQueryItems $ProjectId $Org }
        tags               = & $count { Get-E2EList "$ProjectId/_apis/wit/tags" $Org }
        reposNotOne        = if ($null -eq $repos) { -1 } else { [Math]::Abs($repos.Count - 1) }
        repoRefs           = if ($null -eq $repos -or $repos.Count -ne 1) { -1 } else { & $count { Get-E2EList "$ProjectId/_apis/git/repositories/$($repos[0].id)/refs" $Org } }
        repoRecycleBin     = & $count { Get-E2EList "$ProjectId/_apis/git/recycleBin/repositories" $Org }
        buildDefinitions   = & $count { Get-E2EList "$ProjectId/_apis/build/definitions" $Org }
        releaseDefinitions = & $count { Get-E2EList "$ProjectId/_apis/release/definitions" $Org -HostName "vsrm.dev.azure.com" }
        variableGroups     = & $count { Get-E2EList "$ProjectId/_apis/distributedtask/variablegroups" $Org }
        serviceEndpoints   = & $count { Get-E2EList "$ProjectId/_apis/serviceendpoint/endpoints" $Org }
        environments       = & $count { Get-E2EList "$ProjectId/_apis/distributedtask/environments" $Org }
        wikis              = & $count { Get-E2EList "$ProjectId/_apis/wiki/wikis" $Org }
        testPlans          = & $count { Get-E2EList "$ProjectId/_apis/testplan/plans" $Org }
        testRuns           = & $count { Get-E2EList "$ProjectId/_apis/test/runs" $Org }
        extraDashboards    = & $count { Get-E2EDashboards $ProjectId $Org | Where-Object { $_.name -notlike "*Overview" } }
        # A fresh project's Overview dashboards have no widgets.
        overviewWidgets    = & $count { foreach ($d in @(Get-E2EDashboards $ProjectId $Org | Where-Object { $_.name -like "*Overview" })) { Get-E2EDashboardWidgets $ProjectId $d $Org } }
    }
}

# Returns nothing when the project is cleared, otherwise one "surface=count" per difference.
function Test-E2EPoolClear([string]$ProjectId, [string]$Org) {
    $inv = Get-E2EPoolInventory $ProjectId $Org
    foreach ($k in $inv.Keys) { if ($inv[$k] -ne 0) { "$k=$($inv[$k])" } }
}

function Invoke-E2EStep([scriptblock]$Step, [string]$What, [System.Collections.Generic.List[string]]$Errors) {
    try { & $Step | Out-Null }
    catch {
        $msg = try { ($_.ErrorDetails.Message | ConvertFrom-Json).message } catch { $null }
        $Errors.Add("${What}: $(if ($msg) { $msg } else { $_.Exception.Message })")
    }
}

# Test Cases are refused by the work item delete (400); the test case endpoint deletes them permanently. Shared Steps
# and Shared Parameters have no delete in any REST API (only the web portal can delete them), so they are kept as
# leftovers (spec FR-004, Martin 2026-11-24 "Tolerate leftovers"): retitled, moved to the area pool-leftovers, every
# link removed. Every harness query leaves them out (Get-E2EPoolLeftoverFilter). Everything else is destroyed.
$script:LeftoverTypes = @("Shared Steps", "Shared Parameter")
$script:LeftoverTitle = "amte2e-pool leftover"
$script:LeftoverArea = "pool-leftovers"

# The WIQL clause that leaves leftovers out of a query.
function Get-E2EPoolLeftoverFilter { "NOT ([System.Title] = '$($script:LeftoverTitle)' AND [System.WorkItemType] IN ('$($script:LeftoverTypes -join "','")'))" }

function Set-E2EPoolLeftover([string]$ProjectId, [string]$ProjectName, [int]$Id, [string]$Org) {
    $areas = Get-E2EChildNodes $ProjectId Areas $Org
    if (-not ($areas.Children | Where-Object name -eq $script:LeftoverArea)) {
        Invoke-E2ERest "$ProjectId/_apis/wit/classificationnodes/Areas" -Org $Org -Method POST -Body @{ name = $script:LeftoverArea } | Out-Null
    }
    $full = Invoke-E2ERest "_apis/wit/workitems/$Id`?`$expand=relations" -Org $Org
    $area = "$ProjectName\$($script:LeftoverArea)"
    $rels = @($full.relations | Where-Object { $_ })
    if ($full.fields.'System.Title' -eq $script:LeftoverTitle -and $full.fields.'System.AreaPath' -eq $area -and -not $rels.Count) { return }
    $ops = @(@{ op = "add"; path = "/fields/System.Title"; value = $script:LeftoverTitle }, @{ op = "add"; path = "/fields/System.AreaPath"; value = $area })
    for ($i = $rels.Count - 1; $i -ge 0; $i--) { $ops += @{ op = "remove"; path = "/relations/$i" } }
    Invoke-E2ERest "_apis/wit/workitems/$Id`?bypassRules=true&suppressNotifications=true" -Org $Org -Method PATCH -Body (ConvertTo-Json @($ops) -Depth 10) -ContentType "application/json-patch+json" | Out-Null
}

function Remove-E2EPoolWorkItem([string]$ProjectId, [string]$ProjectName, $Item, [string]$Org) {
    $type = $Item.fields.'System.WorkItemType'
    if ($type -eq "Test Case") { Invoke-E2ERest "$ProjectId/_apis/test/testcases/$($Item.id)" -Org $Org -Method DELETE | Out-Null }
    elseif ($type -in $script:LeftoverTypes) { Set-E2EPoolLeftover $ProjectId $ProjectName $Item.id $Org }
    else { Invoke-E2ERest "_apis/wit/workitems/$($Item.id)?destroy=true" -Org $Org -Method DELETE | Out-Null }
}

# Clears what scenarios create (research R3), in an order where nothing still refers to what is removed.
function Clear-E2EPoolProject([string]$ProjectId, [string]$Org) {
    Assert-E2EPoolLeaseHeld $ProjectId $Org | Out-Null
    $p = Invoke-E2ERest "_apis/projects/$ProjectId" -Org $Org
    Assert-E2EName $p.name
    $e = [System.Collections.Generic.List[string]]::new()
    $pr = $ProjectId

    foreach ($x in Get-E2EList "$pr/_apis/testplan/plans" $Org) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/testplan/plans/$($x.id)" -Org $Org -Method DELETE } "test plan $($x.id)" $e }
    foreach ($x in Get-E2EList "$pr/_apis/test/runs" $Org) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/test/runs/$($x.id)" -Org $Org -Method DELETE } "test run $($x.id)" $e }

    # Work items: destroy=true deletes and purges in one call. The query returns at most 5,000 here, so loop.
    for ($round = 0; $round -lt 50; $round++) {
        $ids = @(Get-E2EProjectWorkItemIds $pr $Org)
        if (-not $ids.Count) { break }
        $before = $e.Count
        for ($i = 0; $i -lt $ids.Count; $i += 200) {
            $batch = $ids[$i..([Math]::Min($i + 199, $ids.Count - 1))]
            $read = "_apis/wit/workitems?ids=$($batch -join ',')&fields=System.Id,System.WorkItemType"
            $items = try { @((Invoke-E2ERest $read -Org $Org).value) } catch { throw "Reading work items to clear failed ($read): $($_.ErrorDetails.Message)" }
            foreach ($wi in $items) {
                Invoke-E2EStep { Remove-E2EPoolWorkItem $pr $p.name $wi $Org } "$($wi.fields.'System.WorkItemType') $($wi.id)" $e
            }
        }
        if ($e.Count -gt $before) { break }
    }
    foreach ($x in Get-E2EList "$pr/_apis/wit/recyclebin" $Org) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/wit/recyclebin/$($x.id)" -Org $Org -Method DELETE } "recycled work item $($x.id)" $e }

    foreach ($x in Get-E2EList "$pr/_apis/release/definitions" $Org -HostName "vsrm.dev.azure.com") { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/release/definitions/$($x.id)?forceDelete=true" -Org $Org -Host_ "vsrm.dev.azure.com" -Method DELETE } "release definition $($x.name)" $e }
    foreach ($x in Get-E2EList "$pr/_apis/build/definitions" $Org) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/build/definitions/$($x.id)" -Org $Org -Method DELETE } "build definition $($x.name)" $e }
    foreach ($x in Get-E2EList "$pr/_apis/distributedtask/variablegroups" $Org) { Invoke-E2EStep { Invoke-E2ERest "_apis/distributedtask/variablegroups/$($x.id)?projectIds=$pr" -Org $Org -Method DELETE } "variable group $($x.name)" $e }
    foreach ($x in Get-E2EList "$pr/_apis/serviceendpoint/endpoints" $Org) { Invoke-E2EStep { Invoke-E2ERest "_apis/serviceendpoint/endpoints/$($x.id)?projectIds=$pr" -Org $Org -Method DELETE } "service connection $($x.name)" $e }
    foreach ($x in Get-E2EList "$pr/_apis/distributedtask/environments" $Org) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/distributedtask/environments/$($x.id)" -Org $Org -Method DELETE } "environment $($x.name)" $e }
    foreach ($x in Get-E2EList "$pr/_apis/wiki/wikis" $Org) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/wiki/wikis/$($x.id)" -Org $Org -Method DELETE } "wiki $($x.name)" $e }
    foreach ($x in @(Get-E2EDashboards $pr $Org | Where-Object { $_.name -notlike "*Overview" })) { Invoke-E2EStep { Invoke-E2ERest "$pr/$($x.TeamId)/_apis/dashboard/dashboards/$($x.id)" -Org $Org -Method DELETE -ApiVersion "7.1-preview.3" } "dashboard $($x.name)" $e }
    # The Overview dashboards stay, as in a fresh project, but empty of widgets.
    foreach ($x in @(Get-E2EDashboards $pr $Org | Where-Object { $_.name -like "*Overview" })) {
        foreach ($w in @(Get-E2EDashboardWidgets $pr $x $Org)) { Invoke-E2EStep { Invoke-E2ERest "$pr/$($x.TeamId)/_apis/dashboard/dashboards/$($x.id)/widgets/$($w.id)" -Org $Org -Method DELETE -ApiVersion "7.1-preview.2" } "widget $($w.name) on $($x.name)" $e }
    }

    foreach ($x in @(Get-E2EQueryItems $pr $Org)) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/wit/queries/$($x.id)" -Org $Org -Method DELETE } "query $($x.path)" $e }
    foreach ($x in @(Get-E2EList "_apis/projects/$pr/teams?`$top=500" $Org | Where-Object { $_.id -ne $p.defaultTeam.id -and $_.name -ne $script:LeaseTeam })) {
        Invoke-E2EStep { Invoke-E2ERest "_apis/projects/$pr/teams/$($x.id)" -Org $Org -Method DELETE } "team $($x.name)" $e
    }
    foreach ($g in "Areas", "Iterations") {
        $n = Get-E2EChildNodes $pr $g $Org
        foreach ($x in @($n.Children | Where-Object { $g -ne "Areas" -or $_.name -ne $script:LeftoverArea })) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/wit/classificationnodes/$g/$([Uri]::EscapeDataString($x.name))?`$reclassifyId=$($n.Root.id)" -Org $Org -Method DELETE } "$g node $($x.name)" $e }
    }
    foreach ($x in Get-E2EList "$pr/_apis/wit/tags" $Org) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/wit/tags/$($x.id)" -Org $Org -Method DELETE } "tag $($x.name)" $e }

    # Repos: one new empty repo replaces them all (refs and pull requests go with the old ones). A project must keep a
    # repo, so the new one is made first; it takes the project's name in Rename-E2EPoolProject.
    $old = @(Get-E2EList "$pr/_apis/git/repositories" $Org)
    $keep = if ($old.Count -eq 1 -and -not $old[0].defaultBranch -and -not @(Get-E2EList "$pr/_apis/git/repositories/$($old[0].id)/refs" $Org).Count) { $old[0] }
    if (-not $keep) {
        $null = Invoke-E2ERest "$pr/_apis/git/repositories" -Org $Org -Method POST -Body @{ name = "amte2e-pool-new-$([Guid]::NewGuid().ToString('N').Substring(0, 6))"; project = @{ id = $pr } }
        foreach ($x in $old) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/git/repositories/$($x.id)" -Org $Org -Method DELETE } "repo $($x.name)" $e }
        # A deleted repo can stay in the listing for a moment; wait until only the new one is there.
        for ($i = 0; $i -lt 20 -and @(Get-E2EList "$pr/_apis/git/repositories" $Org).Count -gt 1; $i++) { Start-Sleep -Seconds 3 }
    }
    foreach ($x in Get-E2EList "$pr/_apis/git/recycleBin/repositories" $Org) { Invoke-E2EStep { Invoke-E2ERest "$pr/_apis/git/recycleBin/repositories/$($x.id)" -Org $Org -Method DELETE } "recycled repo $($x.name)" $e }

    if ($e.Count) { throw "Could not clear $Org/$($p.name):`n  " + ($e -join "`n  ") }
    $left = @(Test-E2EPoolClear $pr $Org)
    if ($left.Count) { throw "Clearing $Org/$($p.name) left: $($left -join ', ')" }
}

# ---------------------------------------------------------------- process and name

function Get-E2EProjectProcessId([string]$ProjectId, [string]$Org) {
    (Invoke-E2ERest "_apis/projects/$ProjectId/properties?keys=System.ProcessTemplateType" -Org $Org -ApiVersion "7.1-preview.1").value.value
}

function Switch-E2EPoolProcess([string]$ProjectId, [string]$Process, [string]$Org) {
    $typeId = Get-E2EProcessId $Process $Org
    if (-not $typeId) { throw "Process '$Process' not found in $Org. Run tools/e2e/Initialize-E2EEnvironment.ps1 first." }
    if ((Get-E2EProjectProcessId $ProjectId $Org) -eq $typeId) { return }
    Assert-E2EPoolLeaseHeld $ProjectId $Org | Out-Null
    Invoke-E2ERest "$ProjectId/_apis/wit/projectprocessmigration" -Org $Org -Method POST -ApiVersion "7.1-preview.1" -Body @{ typeId = $typeId } | Out-Null
    for ($i = 0; (Get-E2EProjectProcessId $ProjectId $Org) -ne $typeId; $i++) {
        if ($i -ge 20) { throw "Project $ProjectId in $Org did not switch to process '$Process'." }
        Start-Sleep -Seconds 3
    }
    # The switch is confirmed by the work item types too: the project must offer every type the process defines.
    $want = @((Invoke-E2ERest "_apis/work/processes/$typeId/workitemtypes" -Org $Org).value | Where-Object { -not $_.isDisabled } | ForEach-Object name)
    for ($i = 0; ; $i++) {
        # This response has keys differing only in case, so it comes back as text; parse it case-sensitively.
        $r = Invoke-E2ERest "$ProjectId/_apis/wit/workitemtypes" -Org $Org
        if ($r -is [string]) { $r = $r | ConvertFrom-Json -AsHashtable }
        $have = @($r.value | ForEach-Object { $_.name })
        $missing = @($want | Where-Object { $_ -notin $have })
        if (-not $missing) { break }
        if ($i -ge 20) { throw "Project $ProjectId in $Org is on '$Process' but lacks its work item types: $($missing -join ', ')." }
        Start-Sleep -Seconds 3
    }
}

function Rename-E2EPoolProject([string]$ProjectId, [string]$Name, [string]$Org) {
    Assert-E2EName $Name
    $p = Invoke-E2ERest "_apis/projects/$ProjectId" -Org $Org
    if ($p.name -ne $Name) {
        $clash = try { Invoke-E2ERest "_apis/projects/$([Uri]::EscapeDataString($Name))" -Org $Org } catch { $null }
        if ($clash) { throw "Cannot rename $Org/$($p.name) to '$Name': a project of that name exists." }
        Assert-E2EPoolLeaseHeld $ProjectId $Org | Out-Null
        Wait-E2EOperation (Invoke-E2ERest "_apis/projects/$ProjectId" -Org $Org -Method PATCH -Body @{ name = $Name }) | Out-Null
        for ($i = 0; ; $i++) {
            $q = try { Invoke-E2ERest "_apis/projects/$([Uri]::EscapeDataString($Name))" -Org $Org } catch { $null }
            if ($q -and $q.id -eq $ProjectId) { break }
            if ($i -ge 19) { throw "Project $ProjectId in $Org is not readable as '$Name' after the rename." }
            Start-Sleep -Seconds 3
        }
    }
    # The single repo is named after the project, as in a fresh project.
    $repos = @(Get-E2EList "$ProjectId/_apis/git/repositories" $Org)
    if ($repos.Count -eq 1 -and $repos[0].name -ne $Name) { Invoke-E2ERest "$ProjectId/_apis/git/repositories/$($repos[0].id)" -Org $Org -Method PATCH -Body @{ name = $Name } | Out-Null }
}

# ---------------------------------------------------------------- take and release

function Format-E2EPoolLeases([object[]]$Pool) {
    $held = @($Pool | Where-Object Lease | ForEach-Object { "$($_.Name) (slot $(Format-E2EPoolName $_.Slot)) held by $($_.Lease.Token) until $($_.Lease.Expires.ToString('o'))" })
    if ($held.Count) { $held -join "; " } else { "none (the pool has no projects; run tools/e2e/Initialize-E2EPool.ps1)" }
}

# Takes a pool project and returns it as project $Name on process $Process. Creates no project.
function Request-E2EPoolProject {
    param([Parameter(Mandatory)][string]$Name, [string]$Process = $script:BaseProcess, [Parameter(Mandatory)][string]$Org)
    Assert-E2EName $Name
    $me = (Get-E2EPoolOwner).Token
    $existing = Get-E2EProjectOrNull $Name $Org
    if ($existing) {
        $l = Get-E2EPoolLease $existing.id $Org
        if ($l -and $l.Token -eq $me) { Set-E2EPoolLease $existing.id $Org -Renew; return $existing | Add-Member -NotePropertyName PoolSlot -NotePropertyValue $l.Slot -PassThru -Force }
        throw "Project $Org/$Name exists and is not held by this run ($me). It is not a project this run can take."
    }

    $pool = @(Get-E2EPoolProject -Org $Org)
    # Free pool-named slots first, then stale leases, then (a crashed run's projects keep the seed's name) every other
    # guarded project with a stale lease.
    $all = @(Get-E2EPoolProject -Org $Org -All)
    $poolIds = @($pool.Id)
    $others = @($all | Where-Object { $_.Id -notin $poolIds -and $_.State -in "Expired", "Orphaned" })
    $candidates = @($pool | Where-Object State -eq "Free") + @($pool | Where-Object State -in "Expired", "Orphaned") + $others
    # Every slot found goes into the message if none can be taken, held ones under their scenario names too, so it
    # says which lease is in the way; only the candidates above are ever taken.
    $pool = @($pool) + @($all | Where-Object { $_.Id -notin $poolIds })
    $errors = @()
    foreach ($c in $candidates) {
        if ($c.State -ne "Free" -and -not (Remove-E2EPoolStaleLease $c.Id $c.Lease $Org)) { continue }
        if (-not (New-E2EPoolLease $c.Id $c.Slot $Org)) { continue }
        try {
            Clear-E2EPoolProject $c.Id $Org
            Switch-E2EPoolProcess $c.Id $Process $Org
            Rename-E2EPoolProject $c.Id $Name $Org
            Set-E2EPoolLease $c.Id $Org -Renew
            return (Invoke-E2ERest "_apis/projects/$($c.Id)" -Org $Org) | Add-Member -NotePropertyName PoolSlot -NotePropertyValue $c.Slot -PassThru -Force
        }
        catch {
            # The lease is freed (the next take clears the project first) and the next slot is tried, at most three, so a
            # broken slot cannot block the pool and a fault common to every slot cannot hold the whole pool. The lease is
            # the only record of the slot while the project has another name, so the pool name comes back first; if it
            # cannot, the lease is kept and expires, and the slot is still found and cleared on a later take.
            $err = "$(Format-E2EPoolName $c.Slot): $($_.Exception.Message)"
            try {
                $poolName = Format-E2EPoolName $c.Slot
                if ((Invoke-E2ERest "_apis/projects/$($c.Id)" -Org $Org).name -ne $poolName) { Rename-E2EPoolProject $c.Id $poolName $Org }
                Remove-E2EPoolLease $c.Id $Org
                $err += " (lease freed)"
            } catch { $err += " (lease kept until it expires: it could not be given its pool name back: $($_.Exception.Message))" }
            $errors += $err
            if ($errors.Count -ge 3) { break }
        }
    }
    $why = if ($errors) { " Slots that failed: $($errors -join '; ')." } else { "" }
    throw "No free pool project in $Org.$why Leases: $(Format-E2EPoolLeases $pool)"
}

# Gives a pool project back: clear, back to amte2e-agile, back to amte2e-pool-NN, lease deleted last. Never deletes it.
# A failure keeps the lease, so a half-cleared project is never offered; it is cleared on the next take after expiry.
function Reset-E2EPoolProject {
    # -Force takes over another run's lease; with -StaleOnly only an Expired or Orphaned one (ordinary clean-up), so a
    # run that has just claimed or renewed the project keeps it. -Force alone is the operator's -ClearPool.
    param([Parameter(Mandatory)][string]$Name, [Parameter(Mandatory)][string]$Org, [switch]$Force, [switch]$StaleOnly)
    Assert-E2EName $Name
    $p = Get-E2EProjectOrNull $Name $Org
    if (-not $p) { return "absent" }
    $l = Get-E2EPoolLease $p.id $Org
    if (-not $l) {
        if (Get-E2EPoolSlot $p.name) { return "free" }
        throw "Project $Org/$Name is not a leased pool project, so it is not released (and never deleted)."
    }
    if ($l.Token -ne (Get-E2EPoolOwner).Token) {
        if (-not $Force) { throw "Project $Org/$Name is held by $($l.Token), not by this run." }
        # Operator override (Remove-E2EObjects -ClearPool): take the lease over as this run, then release as usual.
        $slot = if ($l.Slot) { $l.Slot } else { Get-E2EPoolSlot $p.name }
        if (-not $slot) { throw "Project $Org/$Name has a lease with no slot; release it by hand." }
        if ($StaleOnly -and (Get-E2EPoolLeaseState $l) -notin "Expired", "Orphaned") { throw "Project $Org/$Name is held by a live run ($($l.Token)) until $($l.Expires.ToString('o')); kept." }
        if (-not (Remove-E2EPoolStaleLease $p.id $l $Org -EvenIfLive:(-not $StaleOnly)) -or -not (New-E2EPoolLease $p.id $slot $Org)) { throw "Could not take over the lease on $Org/$Name." }
        $l = Get-E2EPoolLease $p.id $Org
    }
    Clear-E2EPoolProject $p.id $Org
    Switch-E2EPoolProcess $p.id $script:BaseProcess $Org
    Rename-E2EPoolProject $p.id (Format-E2EPoolName $l.Slot) $Org
    Remove-E2EPoolLease $p.id $Org
    Format-E2EPoolName $l.Slot
}

# Releases every pool project held by run $Owner, or every leased one with -All. Reports each; never stops early.
function Clear-E2EPool {
    param([Parameter(Mandatory)][string]$Org, [string]$Owner, [switch]$All, [switch]$WhatIf)
    if (-not $Owner -and -not $All) { throw "Give -Owner <run> or -All." }
    $held = @(if ($All) { Get-E2EPoolProject -Org $Org -All | Where-Object Lease } else { Get-E2EPoolProject -Org $Org -Owner $Owner })
    foreach ($h in $held) {
        if ($WhatIf) { "would release $Org/$($h.Name) -> $(Format-E2EPoolName $h.Slot) (held by $($h.Lease.Token))"; continue }
        # Only a reset that ends on a pool name is a release; "absent" or "free" means there was nothing of this run's to give back.
        try {
            $slot = Reset-E2EPoolProject -Name $h.Name -Org $Org -Force:$All
            if (Get-E2EPoolSlot "$slot") { "released $Org/$($h.Name) -> $slot (cleared)" } else { "NOT RELEASED $Org/$($h.Name): the project was $slot when it was reset" }
        }
        catch { "NOT RELEASED $Org/$($h.Name): $($_.Exception.Message)" }
    }
}

