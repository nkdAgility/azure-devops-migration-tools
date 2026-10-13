# Inherited-process helpers for end-to-end scenarios. Only touches processes named amte2e-*.
Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1")

$script:SystemWits = @{
    Agile = @{
        "User Story" = "Microsoft.VSTS.WorkItemTypes.UserStory"; "Bug" = "Microsoft.VSTS.WorkItemTypes.Bug"
        "Task" = "Microsoft.VSTS.WorkItemTypes.Task"; "Feature" = "Microsoft.VSTS.WorkItemTypes.Feature"
        "Epic" = "Microsoft.VSTS.WorkItemTypes.Epic"; "Issue" = "Microsoft.VSTS.WorkItemTypes.Issue"
        "Test Case" = "Microsoft.VSTS.WorkItemTypes.TestCase"; "Test Plan" = "Microsoft.VSTS.WorkItemTypes.TestPlan"; "Test Suite" = "Microsoft.VSTS.WorkItemTypes.TestSuite"
    }
}

# Returns the inherited (derived) WIT reference name in the process, creating it if needed.
function Get-E2EDerivedWit {
    param([string]$ProcessId, [string]$WitName, [string]$Parent = "Agile")
    $wits = (Invoke-E2ERest "_apis/work/processes/$ProcessId/workitemtypes").value
    $w = $wits | Where-Object { $_.name -eq $WitName -and $_.customization -ne "system" }
    if ($w) { return $w.referenceName }
    (Invoke-E2ERest "_apis/work/processes/$ProcessId/workitemtypes" -Method POST -Body @{
        name = $WitName; inheritsFrom = $script:SystemWits[$Parent][$WitName]; color = "009CCC"; icon = "icon_book"; isDisabled = $false
    }).referenceName
}

function Add-E2EFieldToWit {
    param([string]$ProcessId, [string]$WitName, [string]$FieldRef)
    $wit = Get-E2EDerivedWit $ProcessId $WitName
    $has = (Invoke-E2ERest "_apis/work/processes/$ProcessId/workitemtypes/$wit/fields").value | Where-Object referenceName -eq $FieldRef
    if (-not $has) { Invoke-E2ERest "_apis/work/processes/$ProcessId/workitemtypes/$wit/fields" -Method POST -Body @{ referenceName = $FieldRef } | Out-Null }
}

function Set-E2EWitDisabled {
    param([string]$ProcessId, [string]$WitName, [bool]$Disabled = $true)
    $wit = Get-E2EDerivedWit $ProcessId $WitName
    Invoke-E2ERest "_apis/work/processes/$ProcessId/workitemtypes/$wit" -Method PATCH -Body @{ isDisabled = $Disabled } | Out-Null
}

function Add-E2ERule {
    param([string]$ProcessId, [string]$WitName, [hashtable]$Rule)
    $wit = Get-E2EDerivedWit $ProcessId $WitName
    $existing = (Invoke-E2ERest "_apis/work/processes/$ProcessId/workitemtypes/$wit/rules").value | Where-Object name -eq $Rule.name
    if (-not $existing) { Invoke-E2ERest "_apis/work/processes/$ProcessId/workitemtypes/$wit/rules" -Method POST -Body $Rule | Out-Null }
}

# Creates an inherited Agile process with the ReflectedWorkItemId field on the given types.
function New-E2EBaseProcess {
    param([string]$Name, [string[]]$Wits = @("User Story", "Bug", "Task", "Feature", "Epic", "Issue", "Test Case", "Test Plan", "Test Suite"), [string]$ReflectedField = "Custom.ReflectedWorkItemId", [string]$Org)
    $id = New-E2EProcess -Name $Name -Parent Agile -Org $Org
    foreach ($w in $Wits) { Add-E2EFieldToWit $id $w $ReflectedField }
    $id
}

Export-ModuleMember -Function *-E2E*
