# identity: #3190/#3196 (identity fields keep source values, no post-replay saves), #3180 (ClosedDate), #3205 (Activated/Resolved
# Date, TF26194 on reactivation), #3187 (null identity values) - against a target process rule that sets the current user.
# The organisations have one user, so the source uses an unresolved identity string ("Ghost Person <...>", stored by the
# service as-is with bypassRules) that can never be mistaken for the user running the migration.
param([string]$Src = "amte2e-identity-src", [string]$Tgt = "amte2e-identity-tgt", [string]$RuleProcess = "amte2e-idrule")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EProcess.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$ghost = "Ghost Person <ghost.person@example.invalid>"

Set-E2EOrg nkdagility-adomt-target
$rp = New-E2EBaseProcess -Name $RuleProcess -Org nkdagility-adomt-target
Add-E2ERule $rp "User Story" @{
    name = "amte2e assign current user on state change"; isDisabled = $false
    conditions = @(@{ conditionType = "whenChanged"; field = "System.State"; value = $null })
    actions = @(@{ actionType = "copyFromCurrentUser"; targetField = "System.AssignedTo"; value = "" })
}
New-E2EProject -Name $Tgt -Process $RuleProcess | Out-Null

Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process "amte2e-agile" | Out-Null

# I1: ghost on AssignedTo/ResolvedBy/ClosedBy, explicit ClosedDate.
$i1 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "identity I1 ghost" })
Set-E2EWorkItem -Id $i1.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = $ghost }) | Out-Null
Set-E2EWorkItem -Id $i1.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null
Set-E2EWorkItem -Id $i1.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = $ghost }) | Out-Null
Set-E2EWorkItem -Id $i1.id -Fields ([ordered]@{ "System.State" = "Resolved" }) | Out-Null
Set-E2EWorkItem -Id $i1.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = $ghost; "Microsoft.VSTS.Common.ResolvedBy" = $ghost }) | Out-Null
Set-E2EWorkItem -Id $i1.id -Fields ([ordered]@{ "System.State" = "Closed" }) | Out-Null
Set-E2EWorkItem -Id $i1.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = $ghost; "Microsoft.VSTS.Common.ClosedBy" = $ghost; "Microsoft.VSTS.Common.ClosedDate" = "2026-09-15T12:00:00Z" }) | Out-Null

# I2: AssignedTo never set; the rule would fill it on every state change.
$i2 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "identity I2 unassigned" })
Set-E2EWorkItem -Id $i2.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null
# Agile assigns the current user on activation; clear it so the item ends unassigned.
Set-E2EWorkItem -Id $i2.id -BypassRules -Ops @(@{ op = "remove"; path = "/fields/System.AssignedTo" }) | Out-Null

# I3: reactivation (Active -> Resolved -> Active) which used to raise TF26194 on Activated Date.
$i3 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "identity I3 reactivated" })
Set-E2EWorkItem -Id $i3.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null
Start-Sleep -Seconds 2
Set-E2EWorkItem -Id $i3.id -Fields ([ordered]@{ "System.State" = "Resolved" }) | Out-Null
Start-Sleep -Seconds 2
Set-E2EWorkItem -Id $i3.id -Fields ([ordered]@{ "System.State" = "Active" }) | Out-Null

Save-E2EState $PSScriptRoot @{ i1 = $i1.id; i2 = $i2.id; i3 = $i3.id; ghost = $ghost }
"seeded identity: I1 $($i1.id) I2 $($i2.id) I3 $($i3.id)"
