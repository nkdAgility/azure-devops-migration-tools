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
        @{ Case = "this machine, owner running"; State = "Held"; Lease = [pscustomobject]@{ Expires = [datetime]::UtcNow.AddHours(1); Machine = [Environment]::MachineName; Pid = "$PID" } }
    ) {
        InModuleScope E2EAzDo -Parameters @{ L = $Lease; S = $State } { param($L, $S) Get-E2EPoolLeaseState $L | Should -Be $S }
    }

    It "is Orphaned when this machine's owner pid has exited" {
        $p = Start-Process pwsh -ArgumentList "-NoProfile", "-Command", "exit 0" -PassThru -WindowStyle Hidden; $p.WaitForExit()
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
            Mock Invoke-E2ERest { throw "404" } -ParameterFilter { $Path -like "_apis/projects/amte2e-*" -and $Method -eq "GET" }
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
            Mock Invoke-E2ERest { throw "404" } -ParameterFilter { $Path -eq "_apis/projects/amte2e-x-tgt" }
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
            Mock Invoke-E2ERest { throw "404" } -ParameterFilter { $Path -eq "_apis/projects/amte2e-x-tgt" }
            Mock Get-E2EPoolProject { @([pscustomobject]@{ Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }, [pscustomobject]@{ Id = "p2"; Name = "amte2e-pool-02"; Slot = 2; Lease = $null; State = "Free" }) }
            Mock New-E2EPoolLease { $true }
            Mock Clear-E2EPoolProject { throw "VS403000 no permission to destroy" } -ParameterFilter { $ProjectId -eq "p1" }
            Mock Clear-E2EPoolProject {}
            Mock Switch-E2EPoolProcess {}
            Mock Rename-E2EPoolProject {}
            Mock Set-E2EPoolLease {}
            Mock Invoke-E2ERest { [pscustomobject]@{ id = "p2"; name = "amte2e-x-tgt" } } -ParameterFilter { $Path -eq "_apis/projects/p2" }
            Mock Remove-E2EPoolLease {}
            (Request-E2EPoolProject -Name "amte2e-x-tgt" -Org "nkdagility-adomt-target").PoolSlot | Should -Be 2
            Should -Invoke Remove-E2EPoolLease -Times 1 -Exactly
        }
    }

    It "takes over an expired lease only if it is unchanged since it was read" {
        InModuleScope E2EAzDo {
            $stale = [pscustomobject]@{ Token = "x|1|old"; Expires = [datetime]::UtcNow.AddHours(-1); Team = [pscustomobject]@{ id = "t1"; description = "d1" } }
            Mock Invoke-E2ERest { throw "404" } -ParameterFilter { $Path -eq "_apis/projects/amte2e-x-tgt" }
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
