#Requires -Version 7
<#
.SYNOPSIS
    Live checks for the scratch project pool (#3342), run against the test organisations.
.DESCRIPTION
    -Case Cycle     both orgs: a take returns an empty project (by inventory) on the requested process with the requested
                    name; a release returns amte2e-pool-NN on amte2e-agile with no lease team.
    -Case Race      target org: two takes started at once, as separate pwsh processes, never get the same project.
    -Case Lease     target org: a live lease is left alone, an expired lease and a dead local owner's lease are taken
                    over, and a project left half-filled is cleared on take.
    -Case Linkscope both orgs: linkscope seeded, migrated and asserted through the pool; its gone project is made and deleted
                    in the source org only, and no target-org project is created or deleted.
    -Case Scenario  both orgs: runs -Scenario -Runs times through the pool (seed, migration, assert, release) and checks
                    the live, deleted, process and Custom.amte2e* field counts are the same before and after.
    Prints PASS/FAIL per check and exits 0 only if every check passes. Takes only pool projects and gives back all it took.
#>
param([Parameter(Mandatory)][ValidateSet("Cycle", "Race", "Lease", "Scenario", "Linkscope")][string]$Case, [int]$Trials = 10, [string]$Scenario = "wicore", [int]$Runs = 2)
$ErrorActionPreference = "Stop"
$here = $PSScriptRoot
Import-Module (Join-Path $here "E2EAzDo.psm1") -Force -WarningAction SilentlyContinue
$src = "nkdagility-adomt-source"; $tgt = "nkdagility-adomt-target"
$run = "test3342-" + [Guid]::NewGuid().ToString("N").Substring(0, 6)
$env:AMTE2E_POOL_OWNER = "${PID}:$run"
$results = [System.Collections.Generic.List[object]]::new()
function Check([string]$Name, [bool]$Pass, [string]$Detail) { $results.Add([pscustomobject]@{ Name = $Name; Pass = $Pass }); "$(if ($Pass) { 'PASS' } else { 'FAIL' }) ${Name}: $Detail" }
function Get-Agile([string]$Org) { Get-E2EProcessId "amte2e-agile" $Org }

try {
switch ($Case) {
    "Cycle" {
        foreach ($org in $src, $tgt) {
            $name = "amt3342-$run-cycle"
            # A process other than the base, so the switch both ways is exercised: the first other amte2e* Agile child.
            $other = (Invoke-E2ERest "_apis/work/processes" -Org $org).value | Where-Object { $_.name -like "amte2e-*" -and $_.name -ne "amte2e-agile" -and $_.parentProcessTypeId -eq ((Invoke-E2ERest "_apis/work/processes" -Org $org).value | Where-Object name -eq "Agile").typeId } | Select-Object -First 1
            $proc = if ($other) { $other.name } else { "amte2e-agile" }
            $live = @((Invoke-E2ERest "_apis/projects?`$top=1000" -Org $org).value).Count
            $p = New-E2EProject -Name $name -Process $proc -Org $org
            $left = @(Test-E2EPoolClear $p.id $org)
            Check "$org take: empty" ($left.Count -eq 0) "$($p.name) slot $($p.PoolSlot) $(if ($left) { $left -join ', ' } else { 'inventory all 0' })"
            Check "$org take: process" ((Get-E2EProjectProcessId $p.id $org) -eq (Get-E2EProcessId $proc $org)) "on $proc"
            $repos = @((Invoke-E2ERest "$($p.id)/_apis/git/repositories" -Org $org).value.name)
            Check "$org take: name" ($p.name -eq $name -and ($repos -join ',') -eq $name) "project $($p.name), repo $($repos -join ',')"
            Remove-E2EProject $name -Org $org
            $back = Invoke-E2ERest "_apis/projects/$($p.id)" -Org $org
            Check "$org release: name" ($back.name -eq (Format-E2EPoolName $p.PoolSlot)) $back.name
            Check "$org release: agile" ((Get-E2EProjectProcessId $p.id $org) -eq (Get-Agile $org)) "process reset"
            Check "$org release: no lease" (-not (Get-E2EPoolLease $p.id $org)) "lease team gone"
            Check "$org project count" (@((Invoke-E2ERest "_apis/projects?`$top=1000" -Org $org).value).Count -eq $live) "live $live"
        }
    }
    "Race" {
        $script = {
            param($here, $name, $owner)
            $env:AMTE2E_POOL_OWNER = $owner
            Import-Module (Join-Path $here "E2EAzDo.psm1") -Force -WarningAction SilentlyContinue
            try { $p = New-E2EProject -Name $name -Org "nkdagility-adomt-target"; "$($p.id)" } catch { "ERROR $($_.Exception.Message)" }
        }
        for ($i = 1; $i -le $Trials; $i++) {
            $names = "amt3342-$run-race$i-a", "amt3342-$run-race$i-b"
            $jobs = @(foreach ($n in $names) { Start-Job -ScriptBlock $script -ArgumentList $here, $n, "${PID}:$run" })
            $ids = @($jobs | Wait-Job | Receive-Job); $jobs | Remove-Job
            $ok = $ids.Count -eq 2 -and -not ($ids -like "ERROR*") -and $ids[0] -ne $ids[1]
            Check "race trial $i" $ok ($ids -join " / ")
            foreach ($n in $names) { try { Remove-E2EProject $n -Org $tgt } catch { "release ${n}: $($_.Exception.Message)" } }
        }
    }
    "Lease" {
        $org = $tgt
        # Held: a lease of another (live) run on a free pool project is left alone.
        $free = @(Get-E2EPoolProject -Org $org | Where-Object State -eq "Free")
        $a = $free[0]; $b = $free[1]; $c = $free[2]
        $plant = {
            param($p, $owner, [datetime]$expires)
            $saved = $env:AMTE2E_POOL_OWNER; $env:AMTE2E_POOL_OWNER = $owner
            try {
                $desc = ConvertTo-E2EPoolLease -Slot $p.Slot -Claimed ([datetime]::UtcNow) -Expires $expires
                Invoke-E2ERest "_apis/projects/$($p.Id)/teams" -Org $org -Method POST -Body @{ name = "amte2e-pool-lease"; description = $desc } | Out-Null
            } finally { $env:AMTE2E_POOL_OWNER = $saved }
        }
        & $plant $a ":other-live-run" ([datetime]::UtcNow.AddHours(1))
        Check "held lease state" ((@(Get-E2EPoolProject -Org $org | Where-Object Id -eq $a.Id))[0].State -eq "Held") (Format-E2EPoolName $a.Slot)
        # Expired: a lease past its expiry is taken over, and the project (half-filled first) is cleared on take.
        & $plant $b ":crashed-run" ([datetime]::UtcNow.AddHours(-1))
        $saved = $env:AMTE2E_POOL_OWNER; $env:AMTE2E_POOL_OWNER = ":crashed-run"
        try { Set-E2EWorkItem (Invoke-E2ERest "_apis/projects/$($b.Id)" -Org $org).name "Task" -Org $org -Fields ([ordered]@{ "System.Title" = "left by a crashed run" }) | Out-Null } finally { $env:AMTE2E_POOL_OWNER = $saved }
        # Orphaned: a lease of this machine whose owner pid is no longer running.
        $dead = Start-Process pwsh -ArgumentList "-NoProfile", "-Command", "exit 0" -PassThru -WindowStyle Hidden; $dead.WaitForExit()
        & $plant $c "$($dead.Id):dead-run" ([datetime]::UtcNow.AddHours(1))
        Check "orphaned lease state" ((@(Get-E2EPoolProject -Org $org | Where-Object Id -eq $c.Id))[0].State -eq "Orphaned") "pid $($dead.Id) not running"
        # Take until both planted leases are taken over (free projects come first), then check the held one never was.
        $took = @()
        for ($i = 0; $i -lt 60 -and -not (($took.Id -contains $b.Id) -and ($took.Id -contains $c.Id)); $i++) {
            $free = @(Get-E2EPoolProject -Org $org | Where-Object State -eq "Free")
            # Park free projects under this run so the next take must use an expired or orphaned lease.
            if ($free.Count) { foreach ($f in $free) { if (New-E2EPoolLease $f.Id $f.Slot $org) { $took += $f } }; continue }
            $p = New-E2EProject -Name "amt3342-$run-lease$i" -Org $org
            $took += [pscustomobject]@{ Id = $p.id; Slot = $p.PoolSlot; Name = $p.name }
        }
        Check "expired lease taken over" ($took.Id -contains $b.Id) (Format-E2EPoolName $b.Slot)
        Check "half-filled project cleared" (($took.Id -contains $b.Id) -and -not @(Test-E2EPoolClear $b.Id $org).Count) "inventory after take"
        Check "orphaned lease taken over" ($took.Id -contains $c.Id) (Format-E2EPoolName $c.Slot)
        Check "held lease left alone" (-not ($took.Id -contains $a.Id) -and (Get-E2EPoolLease $a.Id $org).Run -eq "other-live-run") (Format-E2EPoolName $a.Slot)
        # Give everything back: parked leases are deleted, taken projects released, the planted held lease removed.
        foreach ($t in $took) {
            $p = Invoke-E2ERest "_apis/projects/$($t.Id)" -Org $org
            if (Get-E2EPoolSlot $p.name) { Remove-E2EPoolLease $t.Id $org } else { Remove-E2EProject $p.name -Org $org }
        }
        $l = Get-E2EPoolLease $a.Id $org; if ($l) { Invoke-E2ERest "_apis/projects/$($a.Id)/teams/$($l.Team.id)" -Org $org -Method DELETE | Out-Null }
    }
    "Linkscope" {
        $d = Join-Path $here "scenarios/linkscope"
        $logs = Join-Path ([IO.Path]::GetTempPath()) "amt3342-linkscope-$run"
        $tgtBefore = @((Invoke-E2ERest "_apis/projects?`$top=1000" -Org $tgt).value).Count + @((Invoke-E2ERest "_apis/projects?stateFilter=deleted&`$top=1000" -Org $tgt).value).Count
        pwsh -NoProfile -File (Join-Path $d "seed.ps1") 2>&1 | Select-Object -Last 1
        Check "linkscope seed" ($LASTEXITCODE -eq 0) "exit $LASTEXITCODE"
        pwsh -NoProfile -File (Join-Path $here "Invoke-MigrationE2E.ps1") -Config (Join-Path $d "config.json") -LogDirectory $logs 2>&1 | Select-Object -Last 1
        Check "linkscope migration" ($LASTEXITCODE -eq 0) "exit $LASTEXITCODE"
        pwsh -NoProfile -File (Join-Path $d "assert.ps1") -LogDirectory $logs 2>&1 | Where-Object { "$_" -like "FAIL*" }
        Check "linkscope assertions" ($LASTEXITCODE -eq 0) "exit $LASTEXITCODE"
        foreach ($org in $src, $tgt) { Clear-E2EPool -Org $org -Owner $run | Out-Null }
        $tgtAfter = @((Invoke-E2ERest "_apis/projects?`$top=1000" -Org $tgt).value).Count + @((Invoke-E2ERest "_apis/projects?stateFilter=deleted&`$top=1000" -Org $tgt).value).Count
        Check "target org project count" ($tgtAfter -eq $tgtBefore) "live + deleted $tgtBefore -> $tgtAfter"
    }
    "Scenario" {
        function Get-Counts {
            foreach ($org in $src, $tgt) {
                [pscustomobject]@{
                    Org = $org
                    Live = @((Invoke-E2ERest "_apis/projects?`$top=1000" -Org $org).value).Count
                    Deleted = @((Invoke-E2ERest "_apis/projects?stateFilter=deleted&`$top=1000" -Org $org).value).Count
                    Processes = @((Invoke-E2ERest "_apis/work/processes" -Org $org).value).Count
                    Fields = @((Invoke-E2ERest "_apis/wit/fields" -Org $org).value | Where-Object referenceName -like "Custom.amte2e*").Count
                }
            }
        }
        $before = @(Get-Counts)
        Push-Location (Resolve-Path (Join-Path $here "..\.."))
        try {
            for ($r = 1; $r -le $Runs; $r++) {
                $id = "$(Get-Date -Format 'MMddHHmm')$r"
                $env:AMTE2E_POOL_OWNER = ""
                $out = pwsh -NoProfile -Command "& ./.specify/memory/proof.ps1 -Item 3342 -RunId $id -Only $Scenario; exit `$LASTEXITCODE" 2>&1 | ForEach-Object { "$_" }
                $code = $LASTEXITCODE
                $out | Select-Object -Last 8
                Check "$Scenario run $r" ($code -eq 0) "proof exit $code"
                # The run's processes and fields are cleaned with its prefix, as a worker does after a proof.
                pwsh -NoProfile -File tools/e2e/Remove-E2EObjects.ps1 -Prefix "amte2e-3342-p$id-" 2>&1 | Select-Object -Last 3
            }
        } finally { Pop-Location }
        $after = @(Get-Counts)
        foreach ($b in $before) {
            $a = $after | Where-Object Org -eq $b.Org
            Check "$($b.Org) counts" ($a.Live -eq $b.Live -and $a.Processes -eq $b.Processes -and $a.Fields -eq $b.Fields -and ($b.Org -eq $src -or $a.Deleted -eq $b.Deleted)) "live $($b.Live)->$($a.Live), deleted $($b.Deleted)->$($a.Deleted), processes $($b.Processes)->$($a.Processes), fields $($b.Fields)->$($a.Fields)"
        }
    }
}
}
finally {
    # Whatever happened, give back what this run holds.
    foreach ($org in $src, $tgt) { Clear-E2EPool -Org $org -Owner $run | Where-Object { $_ -like "NOT RELEASED*" } }
}
$failed = @($results | Where-Object { -not $_.Pass }).Count
"$Case`: $($results.Count - $failed) of $($results.Count) checks passed"
exit $(if ($failed -or -not $results.Count) { 1 } else { 0 })
