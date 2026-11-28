# Pester 5+. Run: Invoke-Pester ./tools/e2e
# The scratch project pool (#3342). No test reaches Azure DevOps: Invoke-E2ERest is mocked inside the module.
BeforeAll {
    Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1") -Force -WarningAction SilentlyContinue
}

Describe "lease text" {
    BeforeEach { $env:AMTE2E_POOL_OWNER = "4242:runA" }

    It "round-trips slot, owner and expiry" {
        $exp = [datetime]::UtcNow.AddHours(2)
        $l = ConvertFrom-E2EPoolLease (ConvertTo-E2EPoolLease -Slot 7 -Claimed ([datetime]::UtcNow) -Expires $exp -Item 3342)
        $l.Slot | Should -Be 7
        $l.Pid | Should -Be "4242"
        $l.Run | Should -Be "runA"
        $l.Item | Should -Be "3342"
        $l.Token | Should -Be "$([Environment]::MachineName)|4242|runA"
        [Math]::Abs(($l.Expires - $exp).TotalSeconds) | Should -BeLessThan 1
    }

    It "reads text that is not a lease as no lease" {
        ConvertFrom-E2EPoolLease "a team" | Should -BeNullOrEmpty
        ConvertFrom-E2EPoolLease "" | Should -BeNullOrEmpty
    }
}

Describe "lease state" {
    It "is <State> for <Case>" -ForEach @(
        @{ Case = "no lease"; State = "Free"; Lease = $null }
        @{ Case = "a past expiry"; State = "Expired"; Lease = [pscustomobject]@{ Expires = [datetime]::UtcNow.AddMinutes(-1); Machine = "elsewhere"; Pid = "" } }
        @{ Case = "another machine's live lease"; State = "Held"; Lease = [pscustomobject]@{ Expires = [datetime]::UtcNow.AddHours(1); Machine = "elsewhere"; Pid = "1" } }
        @{ Case = "a hand run (no pid)"; State = "Held"; Lease = [pscustomobject]@{ Expires = [datetime]::UtcNow.AddHours(1); Machine = [Environment]::MachineName; Pid = "" } }
        @{ Case = "this machine, owner running past expiry"; State = "Held"; Lease = [pscustomobject]@{ Expires = [datetime]::UtcNow.AddMinutes(-30); Machine = [Environment]::MachineName; Pid = "$PID" } }
        @{ Case = "this machine, owner running"; State = "Held"; Lease = [pscustomobject]@{ Expires = [datetime]::UtcNow.AddHours(1); Machine = [Environment]::MachineName; Pid = "$PID" } }
    ) {
        InModuleScope E2EAzDo -Parameters @{ L = $Lease; S = $State } { param($L, $S) Get-E2EPoolLeaseState $L | Should -Be $S }
    }

    It "is Orphaned when this machine's owner pid has exited" {
        $p = Start-Process pwsh -ArgumentList "-NoProfile", "-Command", "exit 0" -PassThru -NoNewWindow; $p.WaitForExit()
        InModuleScope E2EAzDo -Parameters @{ Dead = $p.Id } {
            param($Dead)
            Get-E2EPoolLeaseState ([pscustomobject]@{ Expires = [datetime]::UtcNow.AddHours(1); Machine = [Environment]::MachineName; Pid = "$Dead" }) | Should -Be "Orphaned"
        }
    }
}

Describe "pool names" {
    It "reads the slot of <Name> as <Slot>" -ForEach @(
        @{ Name = "amte2e-pool-07"; Slot = 7 }, @{ Name = "amte2e-pool-7"; Slot = 0 }, @{ Name = "amte2e-wicore-src"; Slot = 0 }, @{ Name = "migrationTarget1"; Slot = 0 }
    ) { Get-E2EPoolSlot $Name | Should -Be $Slot }

    It "keeps migrationSource1 and migrationTarget1 out of the guarded projects" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ value = @(
                [pscustomobject]@{ name = "migrationSource1" }, [pscustomobject]@{ name = "migrationTarget1" }, [pscustomobject]@{ name = "amte2e-pool-01" }, [pscustomobject]@{ name = "amt3326-src" }
            ) } }
            @(Get-E2EGuardedProjects "nkdagility-adomt-source").name | Should -Be @("amte2e-pool-01", "amt3326-src")
        }
    }
}

Describe "Assert-E2EPoolLeaseHeld" {
    BeforeEach { $env:AMTE2E_POOL_OWNER = "4242:runA" }

    It "reads again when the teams list briefly leaves the lease out" {
        InModuleScope E2EAzDo {
            $script:reads = 0
            Mock Start-Sleep {}
            Mock Get-E2EPoolLease { $script:reads++; if ($script:reads -lt 3) { $null } else { [pscustomobject]@{ Slot = 3; Token = (Get-E2EPoolOwner).Token } } }
            (Assert-E2EPoolLeaseHeld "p1" "nkdagility-adomt-target").Slot | Should -Be 3
            $script:reads | Should -Be 3
        }
    }

    It "says nobody holds it when every read misses the lease" {
        InModuleScope E2EAzDo {
            Mock Start-Sleep {}
            Mock Get-E2EPoolLease { $null }
            { Assert-E2EPoolLeaseHeld "p1" "nkdagility-adomt-target" } | Should -Throw "*held by nobody*"
            Should -Invoke Get-E2EPoolLease -Times 4 -Exactly
        }
    }

    It "does not read again when another run holds it" {
        InModuleScope E2EAzDo {
            Mock Start-Sleep {}
            Mock Get-E2EPoolLease { [pscustomobject]@{ Slot = 3; Token = "other|9|runB" } }
            { Assert-E2EPoolLeaseHeld "p1" "nkdagility-adomt-target" } | Should -Throw "*held by other|9|runB*"
            Should -Invoke Get-E2EPoolLease -Times 1 -Exactly
        }
    }
}

Describe "Request-E2EPoolProject" {
    BeforeEach { $env:AMTE2E_POOL_OWNER = "4242:runA" }

    It "refuses a name outside the guard before any call" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { throw "no call expected" }
            { Request-E2EPoolProject -Name "migrationTarget1" -Org "nkdagility-adomt-target" } | Should -Throw "*Refusing to touch*"
            Should -Invoke Invoke-E2ERest -Times 0 -Exactly
        }
    }

    It "fails when the pool is exhausted, naming each lease, and creates no project (Q-7)" {
        InModuleScope E2EAzDo {
            $held = [pscustomobject]@{ Token = "other|9|runB"; Expires = [datetime]::UtcNow.AddHours(1) }
            Mock Get-E2EProjectOrNull { $null } -ParameterFilter { $NameOrId -like "amte2e-*" }
            Mock Invoke-E2ERest { throw "no create expected" } -ParameterFilter { $Method -eq "POST" }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Org = "o"; Id = "p1"; Name = "amte2e-x-tgt"; Slot = 1; Lease = $held; State = "Held" }) }
            { Request-E2EPoolProject -Name "amte2e-new-tgt" -Org "nkdagility-adomt-target" } | Should -Throw "No free pool project in nkdagility-adomt-target. Leases: amte2e-x-tgt (slot amte2e-pool-01) held by other|9|runB*"
            Should -Invoke Invoke-E2ERest -Times 0 -Exactly -ParameterFilter { $Method -eq "POST" }
        }
    }

    It "returns a project of that name already held by this run, without taking another" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x-tgt" } } -ParameterFilter { $Path -eq "_apis/projects/amte2e-x-tgt" }
            Mock Get-E2EPoolLease { [pscustomobject]@{ Slot = 3; Token = (Get-E2EPoolOwner).Token } }
            Mock Set-E2EPoolLease {}
            Mock Get-E2EPoolProject { throw "no pool scan expected" }
            (Request-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target").PoolSlot | Should -Be 3
        }
    }

    It "refuses a project of that name held by another run" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x-tgt" } } -ParameterFilter { $Path -eq "_apis/projects/amte2e-x-tgt" }
            Mock Get-E2EPoolLease { [pscustomobject]@{ Slot = 3; Token = "other|9|runB" } }
            { Request-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target" } | Should -Throw "*not held by this run*"
        }
    }

    It "moves to the next project when another taker wins the claim" {
        InModuleScope E2EAzDo {
            Mock Get-E2EProjectOrNull { $null } -ParameterFilter { $NameOrId -eq "amte2e-x-tgt" }
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p2"; name = "amte2e-x-tgt" } } -ParameterFilter { $Path -eq "_apis/projects/p2" }
            Mock Get-E2EPoolProject { @(
                [pscustomobject]@{ Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }
                [pscustomobject]@{ Id = "p2"; Name = "amte2e-pool-02"; Slot = 2; Lease = $null; State = "Free" }
            ) }
            Mock New-E2EPoolLease { $ProjectId -eq "p2" }
            Mock Clear-E2EPoolProject {}; Mock Switch-E2EPoolProcess {}; Mock Rename-E2EPoolProject {}; Mock Set-E2EPoolLease {}
            (Request-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target").PoolSlot | Should -Be 2
            Should -Invoke Clear-E2EPoolProject -Times 1 -Exactly -ParameterFilter { $ProjectId -eq "p2" }
        }
    }

    It "frees a failed slot's lease and tries the next slot" {
        InModuleScope E2EAzDo {
            Mock Get-E2EProjectOrNull { $null } -ParameterFilter { $NameOrId -eq "amte2e-x-tgt" }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }, [pscustomobject]@{ Id = "p2"; Name = "amte2e-pool-02"; Slot = 2; Lease = $null; State = "Free" }) }
            Mock New-E2EPoolLease { $true }
            Mock Clear-E2EPoolProject { throw "VS403000 no permission to destroy" } -ParameterFilter { $ProjectId -eq "p1" }
            Mock Clear-E2EPoolProject {}
            Mock Switch-E2EPoolProcess {}
            Mock Rename-E2EPoolProject {}
            Mock Set-E2EPoolLease {}
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p2"; name = "amte2e-x-tgt" } } -ParameterFilter { $Path -eq "_apis/projects/p2" }
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-pool-01" } } -ParameterFilter { $Path -eq "_apis/projects/p1" }
            Mock Remove-E2EPoolLease {}
            (Request-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target").PoolSlot | Should -Be 2
            Should -Invoke Remove-E2EPoolLease -Times 1 -Exactly
        }
    }

    It "gives a slot that failed after its rename its pool name back before freeing the lease" {
        InModuleScope E2EAzDo {
            Mock Get-E2EProjectOrNull { $null }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }) }
            Mock New-E2EPoolLease { $true }
            Mock Clear-E2EPoolProject {}
            Mock Switch-E2EPoolProcess {}
            Mock Rename-E2EPoolProject {}
            Mock Set-E2EPoolLease { throw "renew failed" }
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x-tgt" } } -ParameterFilter { $Path -eq "_apis/projects/p1" }
            Mock Remove-E2EPoolLease {}
            { Request-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target" } | Should -Throw "*renew failed (lease freed)*"
            Should -Invoke Rename-E2EPoolProject -Times 1 -Exactly -ParameterFilter { $Name -eq "amte2e-pool-01" }
            Should -Invoke Remove-E2EPoolLease -Times 1 -Exactly
        }
    }

    It "keeps the lease when the pool name cannot be given back, so the slot is still found" {
        InModuleScope E2EAzDo {
            Mock Get-E2EProjectOrNull { $null }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }) }
            Mock New-E2EPoolLease { $true }
            Mock Clear-E2EPoolProject {}
            Mock Switch-E2EPoolProcess {}
            Mock Rename-E2EPoolProject {}
            Mock Rename-E2EPoolProject { throw "rename refused" } -ParameterFilter { $Name -eq "amte2e-pool-01" }
            Mock Set-E2EPoolLease { throw "renew failed" }
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x-tgt" } } -ParameterFilter { $Path -eq "_apis/projects/p1" }
            Mock Remove-E2EPoolLease {}
            { Request-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target" } | Should -Throw "*lease kept until it expires*"
            Should -Invoke Remove-E2EPoolLease -Times 0 -Exactly
        }
    }

    It "does not renew a lease with less than the margin left" {
        InModuleScope E2EAzDo {
            Mock Get-E2EPoolLease { [pscustomobject]@{ Slot = 3; Token = (Get-E2EPoolOwner).Token; Expires = [datetime]::UtcNow.AddMinutes(5); Team = [pscustomobject]@{ id = "t1" } } }
            Mock Invoke-E2ERest { throw "no patch expected" }
            { Set-E2EPoolLease "p1" "nkdagility-adomt-target" -Renew } | Should -Throw "*less than 15 minutes*"
        }
    }

    It "stops when a takeover lands between the renewal's check and its PATCH" {
        InModuleScope E2EAzDo {
            # Before the PATCH the lease is this run's with time left; a stale remover deletes it and another run claims
            # it, so the read after the PATCH shows the other run's lease.
            $script:patched = $false
            Mock Get-E2EPoolLease {
                if (-not $script:patched) { [pscustomobject]@{ Slot = 3; Token = (Get-E2EPoolOwner).Token; Claimed = [datetime]::UtcNow; Expires = [datetime]::UtcNow.AddMinutes(30); Team = [pscustomobject]@{ id = "t1" } } }
                else { [pscustomobject]@{ Slot = 3; Token = "other|9|runB"; Expires = [datetime]::UtcNow.AddHours(2); Team = [pscustomobject]@{ id = "t2" } } }
            }
            Mock Invoke-E2ERest { $script:patched = $true } -ParameterFilter { $Method -eq "PATCH" }
            { Set-E2EPoolLease "p1" "nkdagility-adomt-target" -Renew } | Should -Throw "*held by other|9|runB*"
        }
    }

    It "stops when the renewed lease reads back as a different team" {
        InModuleScope E2EAzDo {
            $script:patched = $false
            Mock Get-E2EPoolLease {
                $team = if ($script:patched) { "t2" } else { "t1" }
                [pscustomobject]@{ Slot = 3; Token = (Get-E2EPoolOwner).Token; Claimed = [datetime]::UtcNow; Expires = [datetime]::UtcNow.AddHours(2); Team = [pscustomobject]@{ id = $team } }
            }
            Mock Invoke-E2ERest { $script:patched = $true } -ParameterFilter { $Method -eq "PATCH" }
            { Set-E2EPoolLease "p1" "nkdagility-adomt-target" -Renew } | Should -Throw "*did not hold*"
        }
    }

    It "never renews an expired lease" {
        InModuleScope E2EAzDo {
            Mock Get-E2EPoolLease { [pscustomobject]@{ Slot = 3; Token = (Get-E2EPoolOwner).Token; Expires = [datetime]::UtcNow.AddMinutes(-1); Team = [pscustomobject]@{ id = "t1" } } }
            Mock Invoke-E2ERest { throw "no patch expected" }
            { Set-E2EPoolLease "p1" "nkdagility-adomt-target" -Renew } | Should -Throw "*not renewed*"
        }
    }

    It "reads a bad lease value as unreadable instead of failing" {
        InModuleScope E2EAzDo { ConvertFrom-E2EPoolLease "slot=01;expires=invalid" | Should -BeNullOrEmpty; ConvertFrom-E2EPoolLease "slot=x;expires=2026-01-01T00:00:00Z" | Should -BeNullOrEmpty }
    }

    It "takes over an expired lease only if it is unchanged since it was read" {
        InModuleScope E2EAzDo {
            $stale = [pscustomobject]@{ Token = "x|1|old"; Expires = [datetime]::UtcNow.AddHours(-1); Team = [pscustomobject]@{ id = "t1"; description = "d1" } }
            Mock Get-E2EProjectOrNull { $null } -ParameterFilter { $NameOrId -eq "amte2e-x-tgt" }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $stale; State = "Expired" }) }
            # Someone renewed it in between: the team's description is no longer the one that was read.
            Mock Get-E2EPoolLease { [pscustomobject]@{ Token = "x|1|old"; Team = [pscustomobject]@{ id = "t1"; description = "d2" } } }
            Mock Invoke-E2ERest { throw "no delete expected" } -ParameterFilter { $Method -eq "DELETE" }
            Mock New-E2EPoolLease { throw "no claim expected" }
            { Request-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target" } | Should -Throw "No free pool project*"
            Should -Invoke Invoke-E2ERest -Times 0 -Exactly -ParameterFilter { $Method -eq "DELETE" }
        }
    }
}

Describe "Reset-E2EPoolProject" {
    BeforeEach { $env:AMTE2E_POOL_OWNER = "4242:runA" }

    It "refuses a project that is not a leased pool project, and never deletes it" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-old-tgt" } } -ParameterFilter { -not $Method -or $Method -eq "GET" }
            Mock Invoke-E2ERest { throw "no change expected" } -ParameterFilter { $Method -and $Method -ne "GET" }
            Mock Get-E2EPoolLease { $null }
            { Reset-E2EPoolProject -Name "amte2e-old-tgt" -Org "nkdagility-adomt-target" } | Should -Throw "*not a leased pool project*"
        }
    }

    It "refuses a project held by another run unless forced" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x-tgt" } } -ParameterFilter { -not $Method -or $Method -eq "GET" }
            Mock Get-E2EPoolLease { [pscustomobject]@{ Slot = 4; Token = "other|9|runB" } }
            { Reset-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target" } | Should -Throw "*held by other|9|runB*"
        }
    }

    It "clears, switches back to amte2e-agile, renames to its slot and deletes the lease last" {
        InModuleScope E2EAzDo {
            $script:order = [System.Collections.Generic.List[string]]::new()
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x-tgt" } } -ParameterFilter { -not $Method -or $Method -eq "GET" }
            Mock Get-E2EPoolLease { [pscustomobject]@{ Slot = 4; Token = (Get-E2EPoolOwner).Token } }
            Mock Clear-E2EPoolProject { $script:order.Add("clear") }
            Mock Switch-E2EPoolProcess { $script:order.Add("switch $Process") }
            Mock Rename-E2EPoolProject { $script:order.Add("rename $Name") }
            Mock Remove-E2EPoolLease { $script:order.Add("lease") }
            Reset-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target" | Should -Be "amte2e-pool-04"
            $script:order | Should -Be @("clear", "switch amte2e-agile", "rename amte2e-pool-04", "lease")
        }
    }

    It "keeps the lease when clearing fails, so a half-cleared project is never offered" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x-tgt" } } -ParameterFilter { -not $Method -or $Method -eq "GET" }
            Mock Get-E2EPoolLease { [pscustomobject]@{ Slot = 4; Token = (Get-E2EPoolOwner).Token } }
            Mock Clear-E2EPoolProject { throw "boom" }
            Mock Remove-E2EPoolLease {}
            { Reset-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target" } | Should -Throw "boom"
            Should -Invoke Remove-E2EPoolLease -Times 0 -Exactly
        }
    }
}

Describe "clearing work items" {
    It "deletes a <Type> through <Route>" -ForEach @(
        @{ Type = "Test Case"; Route = "the test case API"; Expect = "pid/_apis/test/testcases/5" }
        @{ Type = "Task"; Route = "the work item API with destroy"; Expect = "_apis/wit/workitems/5?destroy=true" }
    ) {
        InModuleScope E2EAzDo -Parameters @{ T = $Type; E = $Expect } {
            param($T, $E)
            Mock Invoke-E2ERest {}
            Mock Set-E2EPoolLeftover { throw "not a leftover" }
            Remove-E2EPoolWorkItem "pid" "proj" ([pscustomobject]@{ id = 5; fields = @{ "System.WorkItemType" = $T } }) "o"
            Should -Invoke Invoke-E2ERest -Times 1 -Exactly -ParameterFilter { $Path -eq $E -and $Method -eq "DELETE" }
        }
    }

    It "keeps a <Type> as a leftover, since no API can delete it" -ForEach @(@{ Type = "Shared Steps" }, @{ Type = "Shared Parameter" }) {
        InModuleScope E2EAzDo -Parameters @{ T = $Type } {
            param($T)
            Mock Invoke-E2ERest { throw "no delete expected" }
            Mock Set-E2EPoolLeftover {}
            Remove-E2EPoolWorkItem "pid" "proj" ([pscustomobject]@{ id = 5; fields = @{ "System.WorkItemType" = $T } }) "o"
            Should -Invoke Set-E2EPoolLeftover -Times 1 -Exactly -ParameterFilter { $Id -eq 5 -and $ProjectName -eq "proj" }
        }
    }

    It "leaves leftovers out of queries" {
        Get-E2EPoolLeftoverFilter | Should -Be "NOT ([System.Title] = 'amte2e-pool leftover' AND [System.WorkItemType] IN ('Shared Steps','Shared Parameter'))"
    }
}

Describe "disposable projects (linkscope gone, wicore-fresh target)" {
    # The flag is cleared for each case and put back after, so a shell that ran a live proof gives the same result.
    BeforeEach { $script:savedFlag = $env:AMTE2E_FRESH_PROJECTS; $env:AMTE2E_FRESH_PROJECTS = $null }
    AfterEach { $env:AMTE2E_FRESH_PROJECTS = $script:savedFlag }

    It "refuses an existing target-org fresh project, which is not brand-new" {
        $env:AMTE2E_FRESH_PROJECTS = "1"
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x-fresh" } }
            { New-E2EDisposableProject -Name "amte2e-x-fresh" -Org "nkdagility-adomt-target" } | Should -Throw "*already exists*"
            Should -Invoke Invoke-E2ERest -Times 0 -Exactly -ParameterFilter { $Method -eq "POST" }
        }
    }

    It "fails a removal whose lookup fails for a reason other than not found" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { throw "503 Service Unavailable" }
            { Remove-E2EDisposableProject -Name "amte2e-x-gone" -Org "nkdagility-adomt-source" } | Should -Throw "*503*"
        }
    }

    It "refuses <Case>" -ForEach @(
        @{ Case = "a gone project in the target org"; Name = "amte2e-x-gone"; Org = "nkdagility-adomt-target"; Msg = "*only names matching '-fresh$'*" }
        @{ Case = "a fresh project in the source org"; Name = "amte2e-x-fresh"; Org = "nkdagility-adomt-source"; Msg = "*only names matching '-gone$'*" }
        @{ Case = "a name that is not a gone project"; Name = "amte2e-x-src"; Org = "nkdagility-adomt-source"; Msg = "*only names matching*" }
        @{ Case = "another org"; Name = "amte2e-x-fresh"; Org = "nkdagility-preview"; Msg = "*only the test orgs*" }
        @{ Case = "a fresh target project without the flag"; Name = "amte2e-x-fresh"; Org = "nkdagility-adomt-target"; Msg = "*set AMTE2E_FRESH_PROJECTS=1*" }
        @{ Case = "a name outside the guard"; Name = "migration-gone"; Org = "nkdagility-adomt-source"; Msg = "*Refusing to touch*" }
    ) {
        InModuleScope E2EAzDo -Parameters @{ N = $Name; O = $Org; M = $Msg } {
            param($N, $O, $M)
            Mock Invoke-E2ERest { throw "no call expected" }
            { New-E2EDisposableProject -Name $N -Org $O } | Should -Throw $M
            { Remove-E2EDisposableProject -Name $N -Org $O } | Should -Throw $M
            Should -Invoke Invoke-E2ERest -Times 0 -Exactly
        }
    }
}

Describe "replies that are not JSON (#3351)" {
    BeforeEach { $env:AMTE2E_POOL_OWNER = "4242:runA" }
    BeforeAll {
        $script:Html = "<!DOCTYPE html><html><body>Azure DevOps is busy</body></html>"
        $script:Org = "nkdagility-adomt-source"
    }

    It "HTML then JSON: reads again after a wait and returns the JSON" {
        InModuleScope E2EAzDo -Parameters @{ Html = $Html; Org = $Org } {
            param($Html, $Org)
            $script:Headers = @{ Authorization = "x" }; $script:n = 0
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { $script:n++; if ($script:n -lt 3) { $Html } else { [pscustomobject]@{ value = @(@{ name = "t" }) } } }
            (Invoke-E2ERest "_apis/projects/p1/teams" -Org $Org).value[0].name | Should -Be "t"
            Should -Invoke Invoke-RestMethod -Times 3 -Exactly
            Should -Invoke Start-Sleep -Times 1 -Exactly -ParameterFilter { $Seconds -eq 5 }
            Should -Invoke Start-Sleep -Times 1 -Exactly -ParameterFilter { $Seconds -eq 15 }
        }
    }

    It "JSON that comes back as text is returned as it is, not retried" {
        InModuleScope E2EAzDo -Parameters @{ Org = $Org } {
            param($Org)
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { '{"value":[{"name":"a","Name":"b"}]}' }
            Invoke-E2ERest "p1/_apis/wit/workitemtypes" -Org $Org | Should -BeOfType [string]
            Should -Invoke Invoke-RestMethod -Times 1 -Exactly
            Should -Invoke Start-Sleep -Times 0 -Exactly
        }
    }

    It "permanent HTML: one clear line naming the call and the organisation, after 4 tries" {
        InModuleScope E2EAzDo -Parameters @{ Html = $Html; Org = $Org } {
            param($Html, $Org)
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { $Html }
            $e = { Invoke-E2ERest "_apis/projects/p1/teams" -Org $Org } | Should -Throw -PassThru
            $e.Exception | Should -BeOfType [E2EUnavailableException]
            $e.Exception.Message | Should -Be "GET _apis/projects/p1/teams in $Org returned HTML instead of JSON after 4 tries; Azure DevOps is unavailable. Every lease is kept."
            Should -Invoke Invoke-RestMethod -Times 4 -Exactly
        }
    }

    It "a write answered with HTML is not sent again" {
        InModuleScope E2EAzDo -Parameters @{ Html = $Html; Org = $Org } {
            param($Html, $Org)
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { $Html }
            { Invoke-E2ERest "_apis/projects/p1/teams" -Org $Org -Method POST -Body @{ name = "x" } } | Should -Throw "POST _apis/projects/p1/teams in $Org returned HTML instead of JSON; it is not sent again*"
            Should -Invoke Invoke-RestMethod -Times 1 -Exactly
        }
    }

    It "HTTP <Status> on a read is waited out like an HTML page" -ForEach @(@{ Status = 429 }, @{ Status = 503 }) {
        InModuleScope E2EAzDo -Parameters @{ Org = $Org; Status = $Status } {
            param($Org, $Status)
            $script:Headers = @{ Authorization = "x" }; $script:n = 0; $script:s = $Status
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { $script:n++; if ($script:n -eq 1) { throw [Microsoft.PowerShell.Commands.HttpResponseException]::new("busy", [System.Net.Http.HttpResponseMessage]::new($script:s)) }; [pscustomobject]@{ value = @() } }
            Invoke-E2ERest "_apis/projects" -Org $Org | Out-Null
            Should -Invoke Invoke-RestMethod -Times 2 -Exactly
        }
    }

    It "any other HTTP error is not retried" {
        InModuleScope E2EAzDo -Parameters @{ Org = $Org } {
            param($Org)
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { throw [Microsoft.PowerShell.Commands.HttpResponseException]::new("gone", [System.Net.Http.HttpResponseMessage]::new(404)) }
            { Invoke-E2ERest "_apis/projects/x" -Org $Org } | Should -Throw "gone"
            Should -Invoke Invoke-RestMethod -Times 1 -Exactly
        }
    }

    It "an unreadable teams reply never reads as no lease" {
        InModuleScope E2EAzDo -Parameters @{ Html = $Html; Org = $Org } {
            param($Html, $Org)
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { $Html }
            { Get-E2EPoolLease "p1" $Org } | Should -Throw "*returned HTML instead of JSON*"
            { Assert-E2EPoolLeaseHeld "p1" $Org } | Should -Throw "*returned HTML instead of JSON*"
            { Get-E2EList "_apis/projects/p1/teams" $Org } | Should -Throw "*returned HTML instead of JSON*"
        }
    }

    It "giving a project back while Azure DevOps stays unavailable deletes nothing" {
        InModuleScope E2EAzDo -Parameters @{ Html = $Html; Org = $Org } {
            param($Html, $Org)
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Get-E2EProjectOrNull { [pscustomobject]@{ id = "p1"; name = "amte2e-x-src" } }
            Mock Invoke-RestMethod { $Html }
            { Reset-E2EPoolProject -Name "amte2e-x-src" -Org $Org } | Should -Throw "*returned HTML instead of JSON*"
            Should -Invoke Invoke-RestMethod -Times 0 -Exactly -ParameterFilter { $Method -ne "GET" -and $Method }
        }
    }

    It "a take that meets an unavailable Azure DevOps stops at once and keeps the lease" {
        InModuleScope E2EAzDo -Parameters @{ Org = $Org } {
            param($Org)
            Mock Get-E2EProjectOrNull { $null }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Org = $Org; Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }, [pscustomobject]@{ Org = $Org; Id = "p2"; Name = "amte2e-pool-02"; Slot = 2; Lease = $null; State = "Free" }) }
            Mock New-E2EPoolLease { $true }
            Mock Clear-E2EPoolProject { throw [E2EUnavailableException]::new("GET x in $Org returned HTML instead of JSON after 4 tries; Azure DevOps is unavailable. Every lease is kept.") }
            Mock Remove-E2EPoolLease {}
            { Request-E2EPoolProject -Name "amte2e-new-src" -Org $Org } | Should -Throw "GET x in $Org returned HTML*"
            Should -Invoke Remove-E2EPoolLease -Times 0 -Exactly
            Should -Invoke New-E2EPoolLease -Times 1 -Exactly
        }
    }

    It "Wait-E2EOperation reads past an HTML page" {
        InModuleScope E2EAzDo -Parameters @{ Html = $Html } {
            param($Html)
            $script:Headers = @{ Authorization = "x" }; $script:n = 0
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { $script:n++; if ($script:n -eq 1) { $Html } else { [pscustomobject]@{ status = "succeeded" } } }
            (Wait-E2EOperation ([pscustomobject]@{ id = 1; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/operations/1" })).status | Should -Be "succeeded"
        }
    }
}

Describe "replies that are not JSON: limits and lease safety (#3351 review)" {
    BeforeEach { $env:AMTE2E_POOL_OWNER = "4242:runA" }

    It "a persistent HTTP <Status> on a read stops after 4 tries, waiting 5, 15 and 30 seconds" -ForEach @(@{ Status = 429 }, @{ Status = 503 }) {
        InModuleScope E2EAzDo -Parameters @{ Status = $Status } {
            param($Status)
            $script:Headers = @{ Authorization = "x" }; $script:s = $Status
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { throw [Microsoft.PowerShell.Commands.HttpResponseException]::new("busy", [System.Net.Http.HttpResponseMessage]::new($script:s)) }
            $e = { Invoke-E2ERest "_apis/projects" -Org "nkdagility-adomt-source" } | Should -Throw -PassThru
            $e.Exception | Should -BeOfType [E2EUnavailableException]
            $e.Exception.Message | Should -Be "GET _apis/projects in nkdagility-adomt-source returned HTTP $Status after 4 tries; Azure DevOps is unavailable. Every lease is kept."
            Should -Invoke Invoke-RestMethod -Times 4 -Exactly
            foreach ($w in 5, 15, 30) { Should -Invoke Start-Sleep -Times 1 -Exactly -ParameterFilter { $Seconds -eq $w } }
        }
    }

    It "a dropped connection does not use up the busy tries" {
        InModuleScope E2EAzDo {
            $script:Headers = @{ Authorization = "x" }; $script:n = 0
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { $script:n++; if ($script:n -le 2) { throw [System.Net.Http.HttpRequestException]::new("dropped") }; "<html>busy</html>" }
            { Invoke-E2ERest "_apis/projects" -Org "nkdagility-adomt-source" } | Should -Throw "*HTML instead of JSON after 4 tries*"
            Should -Invoke Invoke-RestMethod -Times 6 -Exactly
        }
    }

    It "an empty body on a read is not an empty answer; on a write it is fine" {
        InModuleScope E2EAzDo {
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { "" }
            { Get-E2EPoolLease "p1" "nkdagility-adomt-source" } | Should -Throw "*returned an empty reply after 4 tries*"
            { Invoke-E2ERest "_apis/projects/p1/teams/t1" -Org "nkdagility-adomt-source" -Method DELETE } | Should -Not -Throw
        }
    }

    It "listing the pool while Azure DevOps answers with HTML throws, and never reports a project as Free" {
        InModuleScope E2EAzDo {
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Get-E2EGuardedProjects { @([pscustomobject]@{ id = "p1"; name = "amte2e-pool-01" }) }
            Mock Invoke-RestMethod { "<!DOCTYPE html><html></html>" }
            { Get-E2EPoolProject -Org "nkdagility-adomt-source" } | Should -Throw "*returned HTML instead of JSON*"
        }
    }

    It "clearing a project stops at once on an unavailable Azure DevOps, rather than collecting it as one step's failure" {
        InModuleScope E2EAzDo {
            $errors = [System.Collections.Generic.List[string]]::new()
            { Invoke-E2EStep { throw [E2EUnavailableException]::new("GET x in o returned HTML instead of JSON after 4 tries; Azure DevOps is unavailable. Every lease is kept.") } "a step" $errors } | Should -Throw "GET x in o returned HTML*"
            $errors.Count | Should -Be 0
        }
    }

    It "Wait-E2EOperation stops with the clear line when the poll keeps answering with HTML" {
        InModuleScope E2EAzDo {
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { "<html>busy</html>" }
            $e = { Wait-E2EOperation ([pscustomobject]@{ id = 1; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/operations/1" }) } | Should -Throw -PassThru
            $e.Exception | Should -BeOfType [E2EUnavailableException]
            $e.Exception.Message | Should -Be "GET _apis/operations/1 in nkdagility-adomt-source returned HTML instead of JSON after 4 tries; Azure DevOps is unavailable. Every lease is kept."
            # The proof's stop line trims a child's error text down to this message; it must come out clean.
            ("     | " + $e.Exception.Message) -replace '^.*?(\S+ \S+ in \S+ returned)', '$1' | Should -Be $e.Exception.Message
        }
    }

    It "Wait-E2EOperation does not retry any other HTTP error" {
        InModuleScope E2EAzDo {
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { throw [Microsoft.PowerShell.Commands.HttpResponseException]::new("denied", [System.Net.Http.HttpResponseMessage]::new(401)) }
            { Wait-E2EOperation ([pscustomobject]@{ id = 1; url = "https://dev.azure.com/nkdagility-adomt-source/_apis/operations/1" }) } | Should -Throw "denied"
            Should -Invoke Invoke-RestMethod -Times 1 -Exactly
        }
    }
}

Describe "claiming a slot while Azure DevOps is unavailable (#3351, Codex review)" {
    BeforeEach { $env:AMTE2E_POOL_OWNER = "4242:runA" }

    It "a claim answered with HTML stops the take at once, without claiming another slot" {
        InModuleScope E2EAzDo {
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Get-E2EProjectOrNull { $null }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Org = "o"; Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }, [pscustomobject]@{ Org = "o"; Id = "p2"; Name = "amte2e-pool-02"; Slot = 2; Lease = $null; State = "Free" }) }
            Mock Invoke-RestMethod { "<!DOCTYPE html><html></html>" }
            { Request-E2EPoolProject -Name "amte2e-new-src" -Org "nkdagility-adomt-source" } | Should -Throw "POST _apis/projects/p1/teams in nkdagility-adomt-source returned HTML instead of JSON; it is not sent again*"
            Should -Invoke Invoke-RestMethod -Times 1 -Exactly -ParameterFilter { $Method -eq "POST" }
        }
    }

    It "a claim another run won still reads as lost, and the next slot is tried" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { throw "TF400898: a team with this name already exists" } -ParameterFilter { $Method -eq "POST" }
            New-E2EPoolLease "p1" 1 "nkdagility-adomt-source" | Should -BeFalse
        }
    }
}

Describe "unavailable Azure DevOps during the clear check (#3351, Codex review)" {
    It "the inventory stops on an unavailable Azure DevOps instead of counting the surface as unreadable" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x"; defaultTeam = @{ id = "t0" } } } -ParameterFilter { $Path -eq "_apis/projects/p1" }
            Mock Get-E2EList { throw [E2EUnavailableException]::new("GET p1/_apis/git/repositories in o returned HTML instead of JSON after 4 tries; Azure DevOps is unavailable. Every lease is kept.") }
            { Get-E2EPoolInventory "p1" "o" } | Should -Throw "GET p1/_apis/git/repositories in o returned HTML*"
        }
    }

    It "an ordinary failure to read one surface still counts as -1" {
        InModuleScope E2EAzDo {
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-x"; defaultTeam = @{ id = "t0" } } } -ParameterFilter { $Path -eq "_apis/projects/p1" }
            Mock Get-E2EList { throw "403 forbidden" }
            Mock Get-E2EProjectWorkItemIds { throw "403 forbidden" }
            Mock Get-E2EChildNodes { throw "403 forbidden" }
            Mock Get-E2EPoolLeftoverProblems { throw "403 forbidden" }
            Mock Get-E2EQueryItems { throw "403 forbidden" }
            Mock Get-E2EDashboards { throw "403 forbidden" }
            (Get-E2EPoolInventory "p1" "o").workItems | Should -Be -1
        }
    }
}

Describe "busy and empty replies to writes (#3351, CodeRabbit review)" {
    It "HTTP <Status> on a write is not sent again and reads as Azure DevOps unavailable" -ForEach @(@{ Status = 429 }, @{ Status = 503 }) {
        InModuleScope E2EAzDo -Parameters @{ Status = $Status } {
            param($Status)
            $script:Headers = @{ Authorization = "x" }; $script:s = $Status
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { throw [Microsoft.PowerShell.Commands.HttpResponseException]::new("busy", [System.Net.Http.HttpResponseMessage]::new($script:s)) }
            $e = { Invoke-E2ERest "_apis/projects/p1/teams" -Org "nkdagility-adomt-source" -Method POST -Body @{ name = "x" } } | Should -Throw -PassThru
            $e.Exception | Should -BeOfType [E2EUnavailableException]
            $e.Exception.Message | Should -Be "POST _apis/projects/p1/teams in nkdagility-adomt-source returned HTTP $Status; it is not sent again, as it may have been applied. Every lease is kept."
            Should -Invoke Invoke-RestMethod -Times 1 -Exactly
        }
    }

    It "an empty reply to a WIQL query (a POST) is not read as no work items" {
        InModuleScope E2EAzDo {
            $script:Headers = @{ Authorization = "x" }
            Mock Start-Sleep {}
            Mock Invoke-RestMethod { "" }
            { Get-E2EProjectWorkItemIds "p1" "nkdagility-adomt-source" } | Should -Throw "POST p1/_apis/wit/wiql*returned an empty reply; it is not sent again*"
        }
    }

    It "a refused write (not busy) is still rethrown as it was" {
        InModuleScope E2EAzDo {
            $script:Headers = @{ Authorization = "x" }
            Mock Invoke-RestMethod { throw [Microsoft.PowerShell.Commands.HttpResponseException]::new("conflict", [System.Net.Http.HttpResponseMessage]::new(409)) }
            $e = { Invoke-E2ERest "_apis/projects/p1/teams" -Org "nkdagility-adomt-source" -Method POST -Body @{ name = "x" } } | Should -Throw -PassThru
            $e.Exception | Should -Not -BeOfType [E2EUnavailableException]
        }
    }
}

Describe "an outage during a failed slot's recovery (#3351, CodeRabbit review)" {
    It "stops the take, keeping the slot's lease, instead of trying the next slot" {
        InModuleScope E2EAzDo {
            $env:AMTE2E_POOL_OWNER = "4242:runA"
            Mock Get-E2EProjectOrNull { $null }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Org = "o"; Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }, [pscustomobject]@{ Org = "o"; Id = "p2"; Name = "amte2e-pool-02"; Slot = 2; Lease = $null; State = "Free" }) }
            Mock New-E2EPoolLease { $true }
            Mock Clear-E2EPoolProject { throw "Could not clear o/amte2e-pool-01: a step failed" }
            Mock Invoke-E2ERest { throw [E2EUnavailableException]::new("GET _apis/projects/p1 in o returned HTML instead of JSON after 4 tries; Azure DevOps is unavailable. Every lease is kept.") }
            Mock Remove-E2EPoolLease {}
            { Request-E2EPoolProject -Name "amte2e-new-src" -Org "o" } | Should -Throw "GET _apis/projects/p1 in o returned HTML*"
            Should -Invoke New-E2EPoolLease -Times 1 -Exactly
            Should -Invoke Remove-E2EPoolLease -Times 0 -Exactly
        }
    }
}

Describe "every catch that answers for a failure passes an outage on (#3351, CodeRabbit review)" {
    BeforeAll { $script:Down = "GET x in o returned HTML instead of JSON after 4 tries; Azure DevOps is unavailable. Every lease is kept." }
    BeforeEach { $env:AMTE2E_POOL_OWNER = "4242:runA" }

    It "releasing the pool stops at the first outage and does not go on to release the next project" {
        InModuleScope E2EAzDo -Parameters @{ Down = $Down } {
            param($Down)
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Name = "amte2e-a"; Slot = 1; Lease = [pscustomobject]@{ Token = "t" } }, [pscustomobject]@{ Name = "amte2e-b"; Slot = 2; Lease = [pscustomobject]@{ Token = "t" } }) }
            $script:resets = 0
            Mock Reset-E2EPoolProject { $script:resets++; throw [E2EUnavailableException]::new($Down) }
            { Clear-E2EPool -Org "o" -Owner "runA" } | Should -Throw "GET x in o returned HTML*"
            $script:resets | Should -Be 1
        }
    }

    It "the rename's name-clash lookup stops on an outage instead of reading it as no clash" {
        InModuleScope E2EAzDo -Parameters @{ Down = $Down } {
            param($Down)
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p1"; name = "amte2e-pool-01" } } -ParameterFilter { $Path -eq "_apis/projects/p1" }
            Mock Invoke-E2ERest { throw [E2EUnavailableException]::new($Down) } -ParameterFilter { $Path -like "_apis/projects/amte2e-new*" }
            Mock Assert-E2EPoolLeaseHeld { throw "no rename expected" }
            { Rename-E2EPoolProject "p1" "amte2e-new-src" "o" } | Should -Throw "GET x in o returned HTML*"
            Should -Invoke Assert-E2EPoolLeaseHeld -Times 0 -Exactly
        }
    }

    It "a takeover's delete of a stale lease stops on an outage instead of reading it as lost" {
        InModuleScope E2EAzDo -Parameters @{ Down = $Down } {
            param($Down)
            $lease = [pscustomobject]@{ Team = [pscustomobject]@{ id = "t1"; description = "d" } }
            Mock Get-E2EPoolLease { $lease }
            Mock Get-E2EPoolLeaseState { "Expired" }
            Mock Invoke-E2ERest { throw [E2EUnavailableException]::new($Down) } -ParameterFilter { $Method -eq "DELETE" }
            { Remove-E2EPoolStaleLease "p1" $lease "o" } | Should -Throw "GET x in o returned HTML*"
        }
    }
}
