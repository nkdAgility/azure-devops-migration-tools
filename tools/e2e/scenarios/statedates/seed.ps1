# statedates: #3240 (TF26194 "The value for field 'Activated Date' / 'Closed Date' cannot be changed" in the state-stamped date
# fix-up). Items move through Active, Resolved and Closed; some also have the stamped dates changed later in a revision without
# a state change, as in a source that was itself migrated, so the target needs a fix-up where it cannot write those fields.
param([string]$Src = "amte2e-3240-src", [string]$Tgt = "amte2e-3240-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

function Step([int]$Id, [System.Collections.IDictionary]$Fields, [switch]$BypassRules) {
    Start-Sleep -Seconds 2
    Set-E2EWorkItem -Id $Id -Fields $Fields -BypassRules:$BypassRules | Out-Null
}

# D1: User Story New -> Active -> Resolved -> Closed.
$d1 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "statedates D1 story" })
Step $d1.id ([ordered]@{ "System.State" = "Active" })
Step $d1.id ([ordered]@{ "System.State" = "Resolved" })
Step $d1.id ([ordered]@{ "System.State" = "Closed" })

# D2: Bug New -> Active -> Resolved -> Closed, then an edit after it is closed.
$d2 = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "statedates D2 bug" })
Step $d2.id ([ordered]@{ "System.State" = "Active" })
Step $d2.id ([ordered]@{ "System.State" = "Resolved" })
Step $d2.id ([ordered]@{ "System.State" = "Closed" })
Step $d2.id ([ordered]@{ "System.Title" = "statedates D2 bug (edited after close)" })

# D3: closed, then Activated/Resolved/Closed Date back-dated in a revision with no state change (a source that was migrated).
$d3 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "statedates D3 back-dated" })
Step $d3.id ([ordered]@{ "System.State" = "Active" })
Step $d3.id ([ordered]@{ "System.State" = "Resolved" })
Step $d3.id ([ordered]@{ "System.State" = "Closed" })
Step $d3.id ([ordered]@{ "Microsoft.VSTS.Common.ActivatedDate" = "2026-08-01T09:00:00Z"; "Microsoft.VSTS.Common.ResolvedDate" = "2026-08-02T09:00:00Z"; "Microsoft.VSTS.Common.ClosedDate" = "2026-08-03T09:00:00Z" }) -BypassRules
Step $d3.id ([ordered]@{ "System.Title" = "statedates D3 back-dated (edited)" })

# D4: Active -> Resolved, Activated Date changed without a state change, then Closed.
$d4 = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "statedates D4 bug activated date changed" })
Step $d4.id ([ordered]@{ "System.State" = "Active" })
Step $d4.id ([ordered]@{ "System.State" = "Resolved" })
Step $d4.id ([ordered]@{ "Microsoft.VSTS.Common.ActivatedDate" = "2026-08-10T09:00:00Z" }) -BypassRules
Step $d4.id ([ordered]@{ "System.State" = "Closed" })

# D5: Closed -> Active -> Resolved -> Closed (reactivated).
$d5 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "statedates D5 reactivated" })
Step $d5.id ([ordered]@{ "System.State" = "Active" })
Step $d5.id ([ordered]@{ "System.State" = "Closed" })
Step $d5.id ([ordered]@{ "System.State" = "Active" })
Step $d5.id ([ordered]@{ "System.State" = "Resolved" })
Step $d5.id ([ordered]@{ "System.State" = "Closed" })

Save-E2EState $PSScriptRoot @{ d1 = $d1.id; d2 = $d2.id; d3 = $d3.id; d4 = $d4.id; d5 = $d5.id }
"seeded statedates: D1 $($d1.id) D2 $($d2.id) D3 $($d3.id) D4 $($d4.id) D5 $($d5.id)"
