#Requires -Version 7.0
# statefields3270: #3270. Every source revision that changes state carries Activated/Resolved/Closed Date and
# Activated/Resolved/Closed By values that differ from what the target would stamp at migration time (the server clock
# and the migration identity). Faults on main: A wrong dates, B closed-then-reactivated item part-migrated in the wrong
# state, C By fields set to the migration account.
# The organisations have one user, so the source's state changes are attributed to an unresolved identity string
# ("Ghost Person <...>", stored by the service as-is with bypassRules), which can never be mistaken for the migration
# account. Dates are back-dated in the same save as the state change, as a server would have stamped them in the past.
param([string]$Src = "amte2e-3270-src", [string]$Tgt = "amte2e-3270-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$ghost = "Ghost Person <ghost.person@example.invalid>"

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

# A state change made in the past by $ghost: the state, the stamped date and By field, and ChangedBy, in one save.
function Set-State([int]$Id, [string]$State, [System.Collections.IDictionary]$Stamp) {
    Start-Sleep -Seconds 2
    $f = [ordered]@{ "System.State" = $State; "System.ChangedBy" = $ghost }
    foreach ($k in $Stamp.Keys) { $f[$k] = $Stamp[$k] }
    Set-E2EWorkItem -Id $Id -Fields $f -BypassRules | Out-Null
}
function Set-PlainState([int]$Id, [string]$State) { Start-Sleep -Seconds 2; Set-E2EWorkItem -Id $Id -Fields ([ordered]@{ "System.State" = $State }) | Out-Null }
$act = { param($d) [ordered]@{ "Microsoft.VSTS.Common.ActivatedDate" = $d; "Microsoft.VSTS.Common.ActivatedBy" = $ghost } }
$res = { param($d) [ordered]@{ "Microsoft.VSTS.Common.ResolvedDate" = $d; "Microsoft.VSTS.Common.ResolvedBy" = $ghost } }
$clo = { param($d) [ordered]@{ "Microsoft.VSTS.Common.ClosedDate" = $d; "Microsoft.VSTS.Common.ClosedBy" = $ghost } }

# S1 (A, C): User Story New -> Active -> Resolved -> Closed by another user, in the past.
$s1 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3270 S1 story closed by another user" })
Set-State $s1.id "Active" (& $act "2026-09-01T09:00:00Z")
Set-State $s1.id "Resolved" (& $res "2026-09-02T09:00:00Z")
Set-State $s1.id "Closed" (& $clo "2026-09-03T09:00:00Z")

# S2 (B, A, C): User Story closed, then reactivated; the source ends Active.
$s2 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3270 S2 closed then reactivated" })
Set-State $s2.id "Active" (& $act "2026-09-05T09:00:00Z")
Set-State $s2.id "Closed" (& $clo "2026-09-06T09:00:00Z")
$re = & $act "2026-09-07T09:00:00Z"; $re["System.Reason"] = "Reactivated"
Set-State $s2.id "Active" $re

# S3 (B): Bug Active -> Resolved -> Closed -> Active, every change by another user in the past; ends Active.
$s3 = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "3270 S3 bug reactivated" })
Set-State $s3.id "Active" (& $act "2026-09-10T09:00:00Z")
Set-State $s3.id "Resolved" (& $res "2026-09-11T09:00:00Z")
Set-State $s3.id "Closed" (& $clo "2026-09-12T09:00:00Z")
Set-State $s3.id "Active" (& $act "2026-09-13T09:00:00Z")

# S4 (A): ordinary changes stamped by the server just now (as in the customer's run); the migration is later, so a
# target that stamps its own clock differs from the source.
$s4 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3270 S4 server-stamped" })
Set-PlainState $s4.id "Active"; Set-PlainState $s4.id "Resolved"; Set-PlainState $s4.id "Closed"

# S5 (B): server-stamped closed then reactivated.
$s5 = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "3270 S5 server-stamped reactivated" })
Set-PlainState $s5.id "Active"; Set-PlainState $s5.id "Resolved"; Set-PlainState $s5.id "Closed"; Set-PlainState $s5.id "Active"

# S6 (type change): Bug activated by another user, then changed to User Story and closed in the same revision. The target
# replays that revision with a REST type change first, which must carry the source's stamped values too.
$s6 = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "3270 S6 type change and close" })
Set-State $s6.id "Active" (& $act "2026-09-15T09:00:00Z")
$tc = & $clo "2026-09-16T09:00:00Z"; $tc["System.WorkItemType"] = "User Story"
Set-State $s6.id "Closed" $tc

Save-E2EState $PSScriptRoot @{ s1 = $s1.id; s2 = $s2.id; s3 = $s3.id; s4 = $s4.id; s5 = $s5.id; s6 = $s6.id; ghost = $ghost }
"seeded statefields3270: S1 $($s1.id) S2 $($s2.id) S3 $($s3.id) S4 $($s4.id) S5 $($s5.id) S6 $($s6.id)"

