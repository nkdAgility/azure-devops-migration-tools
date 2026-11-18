# usermapping-3272 (#3272): loading the target identities for ValidateMappingTargets, and the ValidateMappingTargets scope option.
# The organisations have one real user, so the mapped source identity is an unresolved string "Ghost Person <...>".
#  K1 - AssignedTo = ghost (mapped in every mapping file).
#  K2 - ResolvedBy = a second ghost that no mapping file maps (the unmapped user).
# One target project per build (main and branch) so re-runs on the same side skip what is already there.
param([string]$Src = "amte2e-3272-src", [string[]]$Tgt = @("amte2e-3272-tgt-main", "amte2e-3272-tgt-branch"))
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$ghost = "Ghost Person <ghost.person@example.invalid>"

Set-E2EOrg nkdagility-adomt-target
foreach ($t in $Tgt) { New-E2EProject -Name $t -Process "amte2e-agile" | Out-Null }
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process "amte2e-agile" | Out-Null

$k1 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3272 K1 mapped ghost" })
Set-E2EWorkItem -Id $k1.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = $ghost }) | Out-Null

$k2 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "3272 K2 unmapped ghost" })
Set-E2EWorkItem -Id $k2.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null
Set-E2EWorkItem -Id $k2.id -Fields ([ordered]@{ "System.State" = "Resolved" }) | Out-Null
Set-E2EWorkItem -Id $k2.id -BypassRules -Fields ([ordered]@{ "Microsoft.VSTS.Common.ResolvedBy" = "Unmapped Ghost <unmapped.ghost@example.invalid>" }) | Out-Null

"seeded usermapping-3272: K1 $($k1.id) K2 $($k2.id)"
