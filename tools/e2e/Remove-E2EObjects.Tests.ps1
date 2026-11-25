# Pester 5+. Run: Invoke-Pester ./tools/e2e
# No test reaches Azure DevOps: every call the cleanup script makes is mocked.
BeforeAll {
    $script:Cleanup = Join-Path $PSScriptRoot "Remove-E2EObjects.ps1"
    Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1") -Force
    Import-Module (Join-Path $PSScriptRoot "E2EProcess.psm1") -Force
}

Describe "Get-E2EFieldFilter" {
    It "maps <Prefix> to <Filter>" -ForEach @(
        @{ Prefix = "amt3272-"; Filter = '^Custom\.amte2e3272(?!\d)' }
        @{ Prefix = "amte2e-3272-"; Filter = '^Custom\.amte2e3272(?!\d)' }
    ) {
        Get-E2EFieldFilter -Prefix $Prefix | Should -Be $Filter
    }

    It "matches only the issue's own fields" {
        $filter = Get-E2EFieldFilter -Prefix "amte2e-3272-"
        "Custom.amte2e3272Hidden" | Should -Match $filter
        "Custom.amte2e32720Hidden" | Should -Not -Match $filter
        "Custom.amte2e3242Hidden" | Should -Not -Match $filter
        "Custom.amte2e3227IssueOnly" | Should -Not -Match $filter
    }

    It "owns no fields for a prefix with no issue number (<Prefix>)" -ForEach @(
        @{ Prefix = "amte2e-fields-" }, @{ Prefix = "amte2e-abc-" }, @{ Prefix = "amte2e-3272x-" }
    ) {
        Get-E2EFieldFilter -Prefix $Prefix | Should -BeNullOrEmpty
    }

    It "refuses '<Prefix>', which matches every e2e object" -ForEach @(
        @{ Prefix = "" }, @{ Prefix = "a" }, @{ Prefix = "amte2e" }, @{ Prefix = "amte2e-" }
    ) {
        { Get-E2EFieldFilter -Prefix $Prefix } | Should -Throw "*matches every e2e object*"
    }

    It "matches every e2e field with -All" {
        Get-E2EFieldFilter -All | Should -Be '^Custom\.amte2e'
    }

    It "refuses -All together with -Prefix" {
        { Get-E2EFieldFilter -All -Prefix "amt3272-" } | Should -Throw "*not both*"
    }
}

Describe "Remove-E2EObjects.ps1" {
    BeforeEach {
        Mock Set-E2EOrg {}
        Mock Invoke-E2ERest { [pscustomobject]@{ value = @() } }
        Mock Get-E2EProjects { @([pscustomobject]@{ name = "amte2e-3272-src" }) }
        Mock Get-E2EProcesses { @([pscustomobject]@{ name = "amte2e-3272-proc" }) }
        Mock Get-E2EFields { @(
            [pscustomobject]@{ referenceName = "Custom.amte2e3272Kept" }
            [pscustomobject]@{ referenceName = "Custom.amte2e32720Other" }
            [pscustomobject]@{ referenceName = "Custom.amte2e3242Hidden" }
        ) }
        # Pool (#3342): one free pool project, and one pool project leased under the issue's name.
        Mock Get-E2EPoolProject { @(
            [pscustomobject]@{ Org = "nkdagility-adomt-source"; Id = "p1"; Name = "amte2e-pool-01"; Slot = 1; Lease = $null; State = "Free" }
            [pscustomobject]@{ Org = "nkdagility-adomt-source"; Id = "p2"; Name = "amte2e-3272-src"; Slot = 2; Lease = [pscustomobject]@{ Token = "m|1|run"; Expires = [datetime]::UtcNow.AddHours(-1) }; State = "Expired" }
            [pscustomobject]@{ Org = "nkdagility-adomt-source"; Id = "p3"; Name = "amte2e-3272-live"; Slot = 3; Lease = [pscustomobject]@{ Token = "m|2|live"; Expires = [datetime]::UtcNow.AddHours(1) }; State = "Held" }
        ) }
        Mock Get-E2EPoolLease { $null }
        Mock Reset-E2EPoolProject { "amte2e-pool-02" }
        Mock Remove-E2EProject {}
        Mock Remove-E2EProcess {}
        Mock Remove-E2EField {}
    }

    It "deletes only the issue's own fields" {
        & $script:Cleanup -Organisations "nkdagility-adomt-source" -Prefix "amte2e-3272-" | Out-Null
        Should -Invoke Remove-E2EField -Times 1 -Exactly -ParameterFilter { $ReferenceName -eq "Custom.amte2e3272Kept" }
        Should -Invoke Remove-E2EField -Times 1 -Exactly
    }

    It "never deletes a project: it gives back the issue's leased pool projects (#3342)" {
        $out = & $script:Cleanup -Organisations "nkdagility-adomt-source" -Prefix "amte2e-3272-"
        Should -Invoke Remove-E2EProject -Times 0 -Exactly
        Should -Invoke Reset-E2EPoolProject -Times 1 -Exactly -ParameterFilter { $Name -eq "amte2e-3272-src" -and $Force }
        $out | Should -Contain "released nkdagility-adomt-source/amte2e-3272-src -> amte2e-pool-02 (cleared)"
        # A lease a live run still holds is left alone.
        Should -Invoke Reset-E2EPoolProject -Times 0 -Exactly -ParameterFilter { $Name -eq "amte2e-3272-live" }
        @($out | Where-Object { $_ -like "kept pool project nkdagility-adomt-source/amte2e-3272-live: held by a live run*" }).Count | Should -Be 1
    }

    It "with -All never deletes or releases a free pool project" {
        & $script:Cleanup -Organisations "nkdagility-adomt-source" -All | Out-Null
        Should -Invoke Remove-E2EProject -Times 0 -Exactly
        Should -Invoke Reset-E2EPoolProject -Times 0 -Exactly -ParameterFilter { $Name -eq "amte2e-pool-01" }
    }

    It "with -ClearPool gives back every leased pool project and touches no process or field" {
        & $script:Cleanup -Organisations "nkdagility-adomt-source" -ClearPool | Out-Null
        Should -Invoke Reset-E2EPoolProject -Times 1 -Exactly -ParameterFilter { $Name -eq "amte2e-3272-src" -and $Force }
        Should -Invoke Remove-E2EProcess -Times 0 -Exactly
        Should -Invoke Remove-E2EField -Times 0 -Exactly
        Should -Invoke Remove-E2EProject -Times 0 -Exactly
    }

    It "refuses -ClearPool with -Prefix or -All" {
        { & $script:Cleanup -Organisations "nkdagility-adomt-source" -ClearPool -All } | Should -Throw "*on its own*"
    }

    It "with -WhatIf lists what it would do and changes nothing" {
        $out = & $script:Cleanup -Organisations "nkdagility-adomt-source" -Prefix "amte2e-3272-" -WhatIf 6>&1
        Should -Invoke Remove-E2EProject -Times 0 -Exactly
        Should -Invoke Reset-E2EPoolProject -Times 0 -Exactly
        $out | Should -Contain "would release nkdagility-adomt-source/amte2e-3272-src -> amte2e-pool-02 (held by m|1|run)"
        Should -Invoke Remove-E2EProcess -Times 0 -Exactly
        Should -Invoke Remove-E2EField -Times 0 -Exactly
        $out | Should -Contain "would delete process nkdagility-adomt-source/amte2e-3272-proc"
        $out | Should -Contain "would delete field nkdagility-adomt-source/Custom.amte2e3272Kept"
        $out | Should -Not -Contain "would delete field nkdagility-adomt-source/Custom.amte2e3242Hidden"
    }

    It "deletes no fields for a prefix with no issue number, and exits 1" {
        & $script:Cleanup -Organisations "nkdagility-adomt-source" -Prefix "amte2e-fields-" 3>$null | Out-Null
        $LASTEXITCODE | Should -Be 1
        Should -Invoke Remove-E2EField -Times 0 -Exactly
        Should -Invoke Remove-E2EProject -Times 0 -Exactly
    }

    It "refuses '<Prefix>' before deleting anything" -ForEach @(@{ Prefix = "amte2e-" }, @{ Prefix = "amte2e" }) {
        { & $script:Cleanup -Organisations "nkdagility-adomt-source" -Prefix $Prefix } | Should -Throw "*matches every e2e object*"
        Should -Invoke Remove-E2EProject -Times 0 -Exactly
        Should -Invoke Remove-E2EField -Times 0 -Exactly
    }

    It "refuses a run with neither -Prefix nor -All" {
        { & $script:Cleanup -Organisations "nkdagility-adomt-source" } | Should -Throw "*-All for every e2e object*"
        Should -Invoke Remove-E2EProject -Times 0 -Exactly
    }

    It "with -All deletes every e2e field" {
        & $script:Cleanup -Organisations "nkdagility-adomt-source" -All | Out-Null
        Should -Invoke Remove-E2EField -Times 3 -Exactly
    }
}
