# picklists: ProcessDefinitionProcessor with picklist fields (#3241), then a work item migration that sets their values.
#   source org: process amte2e-amt3241-pl (Agile-derived) whose User Story has five picklist fields and whose Bug has Size:
#     Colour   String  Red, Green, Blue (default Green on the User Story)
#     Points   Integer 1, 2, 3, 5, 8
#     Tag      String  Alpha, Beta, suggested (free values allowed)
#     AsText   String  High, Low        target already has it as plain text
#     Diff     String  One, Two, Three  target already has it with a list of One, Four
#     Size     String  S, M (default M on the Bug)  target already has it with an Integer list of 1, 2 (type mismatch)
#   project amte2e-amt3241-src with one User Story that sets every field, including a free "Gamma" in the suggested Tag.
#   target org: the process amte2e-amt3241-pl-tgt must NOT exist; step 1 creates it. Step 2 needs the target project
#   created from it: run this script again with -TargetProject after step 1.
param([switch]$TargetProject)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EProcess.psm1") -Force
$srcOrg = "nkdagility-adomt-source"; $tgtOrg = "nkdagility-adomt-target"
$process = "amte2e-amt3241-pl"; $targetProcess = "amte2e-amt3241-pl-tgt"
$src = "amte2e-amt3241-src"; $tgt = "amte2e-amt3241-tgt"; $tgt0 = "amte2e-amt3241-tgt0"

function New-PicklistField([string]$Name, [string]$Type, [string[]]$Items, [bool]$Suggested, [string]$Org) {
    if (-not $Name.StartsWith("amte2eAmt3241")) { throw "Refusing to create field '$Name'." }
    $existing = (Invoke-E2ERest "_apis/wit/fields" -Org $Org).value | Where-Object name -eq $Name
    if ($existing) { return $existing.referenceName }
    $list = Invoke-E2ERest "_apis/work/processes/lists" -Org $Org -Method POST -Body @{ name = "picklist_$([guid]::NewGuid())"; type = $Type; isSuggested = $Suggested; items = $Items }
    (Invoke-E2ERest "_apis/wit/fields" -Org $Org -Method POST -Body @{ name = $Name; type = $Type.ToLower(); usage = "workItem"; isPicklist = $true; isPicklistSuggested = $Suggested; picklistId = $list.id }).referenceName
}

if ($TargetProject) {
    Set-E2EOrg $tgtOrg
    New-E2EProject -Name $tgt -Process $targetProcess -Org $tgtOrg | Out-Null
    "created target project $tgt on $targetProcess"
    return
}

Set-E2EOrg $tgtOrg
New-E2EProject -Name $tgt0 -Process "amte2e-agile" -Org $tgtOrg | Out-Null
if (Get-E2EProcessId $targetProcess $tgtOrg) { throw "Process $targetProcess already exists in $tgtOrg; remove it first." }
New-E2EField "amte2eAmt3241AsText" -Org $tgtOrg | Out-Null
New-PicklistField "amte2eAmt3241Diff" "String" @("One", "Four") $false $tgtOrg | Out-Null
New-PicklistField "amte2eAmt3241Size" "Integer" @("1", "2") $false $tgtOrg | Out-Null

Set-E2EOrg $srcOrg
$fields = [ordered]@{
    Colour = New-PicklistField "amte2eAmt3241Colour" "String" @("Red", "Green", "Blue") $false $srcOrg
    Points = New-PicklistField "amte2eAmt3241Points" "Integer" @("1", "2", "3", "5", "8") $false $srcOrg
    Tag    = New-PicklistField "amte2eAmt3241Tag" "String" @("Alpha", "Beta") $true $srcOrg
    AsText = New-PicklistField "amte2eAmt3241AsText" "String" @("High", "Low") $false $srcOrg
    Diff   = New-PicklistField "amte2eAmt3241Diff" "String" @("One", "Two", "Three") $false $srcOrg
    Size   = New-PicklistField "amte2eAmt3241Size" "String" @("S", "M") $false $srcOrg
}
$procId = New-E2EBaseProcess -Name $process -Wits @("User Story", "Bug", "Task", "Feature", "Epic", "Issue") -Org $srcOrg
$story = Get-E2EDerivedWit $procId "User Story"
foreach ($k in $fields.Keys) {
    # Size is on Bug: a story that leaves it empty would send "" into the Integer target field.
    $wit = if ($k -eq "Size") { Get-E2EDerivedWit $procId "Bug" } else { $story }
    $body = @{ referenceName = $fields[$k] }
    if ($k -eq "Colour") { $body.defaultValue = "Green" }
    if ($k -eq "Size") { $body.defaultValue = "M" }
    $has = (Invoke-E2ERest "_apis/work/processes/$procId/workitemtypes/$wit/fields" -Org $srcOrg).value | Where-Object referenceName -eq $fields[$k]
    if (-not $has) { Invoke-E2ERest "_apis/work/processes/$procId/workitemtypes/$wit/fields" -Org $srcOrg -Method POST -Body $body | Out-Null }
}
New-E2EProject -Name $src -Process $process -Org $srcOrg | Out-Null
$wi = Set-E2EWorkItem -Project $src -Type "User Story" -Org $srcOrg -Fields ([ordered]@{
    "System.Title" = "amt3241 picklist story"
    $fields.Colour = "Blue"; $fields.Points = 5; $fields.Tag = "Gamma"; $fields.AsText = "High"; $fields.Diff = "Three"
})
$mismatch = Set-E2EWorkItem -Project $src -Type "Bug" -Org $srcOrg -Fields ([ordered]@{
    "System.Title" = "amt3241 size mismatch"; $fields.Size = "M"
})
"seeded picklists: process $process, fields $($fields.Values -join ', '), work items $($wi.id) (all values) and $($mismatch.id) (Size only) in $src"
