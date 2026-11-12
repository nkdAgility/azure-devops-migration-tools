# procdef-visibility (#3242): ProcessDefinitionProcessor must copy hidden controls, groups and system controls to the target.
#   source org: process amt3242-vis (Agile-derived) with User Story and Task customised:
#     User Story: inherited control Risk hidden; custom field amte2e3242Hidden added to the inherited Planning group, hidden;
#                 custom group "amt3242 Custom" with amte2e3242Shown (visible) and amte2e3242Secret (hidden);
#                 custom group "amt3242 Custom" itself hidden; system control System.Reason hidden.
#     Task: inherited control Remaining Work hidden.
#   target org: project amt3242-vis-tgt on amte2e-agile; the amt3242-vis process must NOT exist there before the run.
param([string]$Src = "amt3242-vis-src", [string]$Tgt = "amt3242-vis-tgt", [string]$Process = "amt3242-vis")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EProcess.psm1") -Force
$srcOrg = "nkdagility-adomt-source"; $tgtOrg = "nkdagility-adomt-target"

Set-E2EOrg $tgtOrg
New-E2EProject -Name $Tgt -Process "amte2e-agile" -Org $tgtOrg | Out-Null
if (Get-E2EProcessId $Process $tgtOrg) { Write-Warning "Process $Process already exists in $tgtOrg; the run will take the update path instead of create." }

Set-E2EOrg $srcOrg
$procId = New-E2EBaseProcess -Name $Process -Wits @("User Story", "Task") -Org $srcOrg
$story = Get-E2EDerivedWit $procId "User Story"
$task = Get-E2EDerivedWit $procId "Task"
$layout = "_apis/work/processes/$procId/workitemtypes"

function Get-Group([string]$Wit, [string]$Label) {
    $l = Invoke-E2ERest "$layout/$Wit/layout"
    foreach ($p in $l.pages) { foreach ($s in $p.sections) { foreach ($g in $s.groups) { if ($g.label -eq $Label) { return [pscustomobject]@{ Page = $p.id; Section = $s.id; Group = $g } } } } }
}
function Set-ControlVisible([string]$Wit, [string]$Group, [string]$Control, [bool]$Visible) {
    Invoke-E2ERest "$layout/$Wit/layout/groups/$([Uri]::EscapeDataString($Group))/controls/$Control" -Method PATCH -Body @{ visible = $Visible } | Out-Null
}
function Add-LayoutControl([string]$Wit, [string]$Group, [string]$Field, [bool]$Visible) {
    Add-E2EFieldToWit $procId "User Story" $Field
    try { Invoke-E2ERest "$layout/$Wit/layout/groups/$([Uri]::EscapeDataString($Group))/controls" -Method POST -Body @{ id = $Field; visible = $true } | Out-Null } catch { if ($_.ErrorDetails.Message -notmatch "already") { throw } }
    if (-not $Visible) { Set-ControlVisible $Wit $Group $Field $false }
}

$hidden = New-E2EField "amte2e3242Hidden" -Org $srcOrg
$shown = New-E2EField "amte2e3242Shown" -Org $srcOrg
$secret = New-E2EField "amte2e3242Secret" -Org $srcOrg

$planning = Get-Group $story "Planning"
Set-ControlVisible $story $planning.Group.id "Microsoft.VSTS.Common.Risk" $false
Add-LayoutControl $story $planning.Group.id $hidden $false

$custom = Get-Group $story "amt3242 Custom"
if (-not $custom) {
    Invoke-E2ERest "$layout/$story/layout/pages/$($planning.Page)/sections/Section2/groups" -Method POST -Body @{ label = "amt3242 Custom"; visible = $true } | Out-Null
    $custom = Get-Group $story "amt3242 Custom"
}
Add-LayoutControl $story $custom.Group.id $shown $true
Add-LayoutControl $story $custom.Group.id $secret $false

# Azure DevOps accepts visible=false on an inherited group but does not store it, so only a custom group can be hidden.
Invoke-E2ERest "$layout/$story/layout/pages/$($custom.Page)/sections/$($custom.Section)/groups/$($custom.Group.id)" -Method PATCH -Body @{ label = "amt3242 Custom"; visible = $false } | Out-Null
Invoke-E2ERest "$layout/$story/layout/systemcontrols/System.Reason" -Method PATCH -Body @{ visible = $false } | Out-Null

$taskPlanning = foreach ($p in (Invoke-E2ERest "$layout/$task/layout").pages) { foreach ($s in $p.sections) { foreach ($g in $s.groups) { if ($g.controls.id -contains "Microsoft.VSTS.Scheduling.RemainingWork") { $g.id } } } }
Set-ControlVisible $task @($taskPlanning)[0] "Microsoft.VSTS.Scheduling.RemainingWork" $false

New-E2EProject -Name $Src -Process $Process -Org $srcOrg | Out-Null
"seeded procdef-visibility in $Process ($procId)"
