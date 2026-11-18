#Requires -Version 7.0
# usermapping-3272 multi-user check (#3272). Each organisation has one real Entra user, so the other users are unresolved
# strings written with BypassRules:
#  W1 - AssignedTo = "Ghost Person <...>", mapped in the file to the real user; CreatedBy/ChangedBy = the real user, who exists
#       in the target with the same email (the email-match case).
#  W2 - ResolvedBy = "Unmapped Ghost <...>": not in the mapping file (the unmapped user).
#  W3 - AssignedTo = "Second Ghost <...>": mapped to "Nobody Here <...>", which no target identity matches (the target that does
#       not resolve).
# One target project per build, so main and the branch can be compared field by field.
param([string]$Src = "amte2e-3272-users-src", [string[]]$Tgt = @("amte2e-3272-users-tgt-main", "amte2e-3272-users-tgt-branch"))
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
foreach ($t in $Tgt) { New-E2EProject -Name $t -Process "amte2e-agile" | Out-Null }
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process "amte2e-agile" | Out-Null

$w1 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3272 W1 mapped and email-matched" })
Set-E2EWorkItem -Id $w1.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = "Ghost Person <ghost.person@example.invalid>" }) | Out-Null

$w2 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3272 W2 unmapped" })
Set-E2EWorkItem -Id $w2.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null
Set-E2EWorkItem -Id $w2.id -Fields ([ordered]@{ "System.State" = "Resolved" }) | Out-Null
Set-E2EWorkItem -Id $w2.id -BypassRules -Fields ([ordered]@{ "Microsoft.VSTS.Common.ResolvedBy" = "Unmapped Ghost <unmapped.ghost@example.invalid>" }) | Out-Null

$w3 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3272 W3 target does not resolve" })
Set-E2EWorkItem -Id $w3.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = "Second Ghost <second.ghost@example.invalid>" }) | Out-Null

"seeded usermapping-3272 users: W1 $($w1.id) W2 $($w2.id) W3 $($w3.id)"
