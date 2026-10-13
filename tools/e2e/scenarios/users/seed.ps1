# users: #3165 (both mapping file formats), #3187 (exact IdentityFieldsToCheck match, null values, source not mutated),
# #3201 (@mentions through the embedded link tool). The organisations have one real user, so the mapped source identity is an
# unresolved string "Ghost Person <...>" mapped to the real target user.
#  U1 - AssignedTo and ResolvedBy = ghost. Only AssignedTo is listed (plus the decoy "Microsoft.VSTS.Common.Resolved", a
#       prefix of ResolvedBy): AssignedTo must be mapped, ResolvedBy must stay the ghost.
#  U2 - AssignedTo empty.
#  U3 - Description with an @mention of the real user.
param([string]$Src = "amte2e-users-src", [string[]]$Tgt = @("amte2e-users-tgt", "amte2e-users2-tgt"))
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$ghost = "Ghost Person <ghost.person@example.invalid>"

Set-E2EOrg nkdagility-adomt-target
foreach ($t in $Tgt) { New-E2EProject -Name $t -Process "amte2e-agile" | Out-Null }
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process "amte2e-agile" | Out-Null
$me = (Invoke-E2ERest "_apis/connectionData" -ApiVersion "7.1-preview").authenticatedUser

$u1 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "users U1 ghost" })
Set-E2EWorkItem -Id $u1.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null
Set-E2EWorkItem -Id $u1.id -Fields ([ordered]@{ "System.State" = "Resolved" }) | Out-Null
Set-E2EWorkItem -Id $u1.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = $ghost; "Microsoft.VSTS.Common.ResolvedBy" = $ghost }) | Out-Null

$u2 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "users U2 unassigned" })

$mention = "<div>ping <a href=""#"" data-vss-mention=""version:2.0,$($me.id)"">@$($me.providerDisplayName)</a> please</div>"
$u3 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "users U3 mention"; "System.Description" = $mention })

# U4: AssignedTo = ghost (mapped), ResolvedBy = a second, unmapped ghost.
$u4 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "users U4 two ghosts" })
Set-E2EWorkItem -Id $u4.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null
Set-E2EWorkItem -Id $u4.id -Fields ([ordered]@{ "System.State" = "Resolved" }) | Out-Null
Set-E2EWorkItem -Id $u4.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = $ghost; "Microsoft.VSTS.Common.ResolvedBy" = "Other Ghost <other.ghost@example.invalid>" }) | Out-Null

Save-E2EState $PSScriptRoot @{ u1 = $u1.id; u2 = $u2.id; u3 = $u3.id; u4 = $u4.id; ghost = $ghost; meId = $me.id; meName = $me.providerDisplayName }
"seeded users: U1 $($u1.id) U2 $($u2.id) U3 $($u3.id)"
