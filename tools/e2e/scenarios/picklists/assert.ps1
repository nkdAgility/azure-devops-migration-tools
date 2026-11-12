# Reports the target state the picklists scenario checks: each field's picklist (type, suggested, values), the User Story
# field defaults in the migrated process, and the migrated work item's values. Read-only.
# -Check compares that state with what seed.ps1 sets up and throws on any mismatch; -AfterWorkItems also checks the
# work item migrated by step2-workitems.json.
param([switch]$Check, [switch]$AfterWorkItems)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$tgtOrg = "nkdagility-adomt-target"
Set-E2EOrg $tgtOrg
$failures = [System.Collections.Generic.List[string]]::new()
function Expect([bool]$Condition, [string]$What) { if (-not $Condition) { $failures.Add($What) } }

$expectedLists = @{
    Colour = @{ Type = "String"; Suggested = $false; Items = @("Red", "Green", "Blue") }
    Points = @{ Type = "Integer"; Suggested = $false; Items = @("1", "2", "3", "5", "8") }
    Tag    = @{ Type = "String"; Suggested = $true; Items = @("Alpha", "Beta") }
    Diff   = @{ Type = "String"; Suggested = $false; Items = @("One", "Four", "Two", "Three") }
    Size   = @{ Type = "Integer"; Suggested = $false; Items = @("1", "2") }
}

$fields = (Invoke-E2ERest "_apis/wit/fields" -Org $tgtOrg).value | Where-Object referenceName -like "Custom.amte2eAmt3241*" | Sort-Object referenceName
foreach ($f in $fields) {
    $short = $f.referenceName.Replace("Custom.amte2eAmt3241", "")
    $list = if ($f.picklistId) { Invoke-E2ERest "_apis/work/processes/lists/$($f.picklistId)" -Org $tgtOrg } else { $null }
    $desc = if ($list) { "picklist $($list.type) suggested=$($list.isSuggested) [$($list.items -join ', ')]" } else { "NO PICKLIST" }
    "field $($f.referenceName): type=$($f.type) isPicklist=$($f.isPicklist) $desc"
    if ($short -eq "AsText") { Expect (-not $list) "AsText should stay a plain field" }
    elseif ($expectedLists.ContainsKey($short)) {
        $e = $expectedLists[$short]
        Expect ($null -ne $list) "$short should have a picklist"
        if ($list) {
            Expect ($list.type -eq $e.Type) "$short picklist type: expected $($e.Type), got $($list.type)"
            Expect ($list.isSuggested -eq $e.Suggested) "$short suggested: expected $($e.Suggested), got $($list.isSuggested)"
            Expect ((@($list.items) -join "|") -eq ($e.Items -join "|")) "$short values: expected [$($e.Items -join ', ')], got [$($list.items -join ', ')]"
        }
    }
}
$expectedNames = @("AsText", "Colour", "Diff", "Points", "Size", "Tag") | ForEach-Object { "Custom.amte2eAmt3241$_" }
$actualNames = @($fields | ForEach-Object referenceName)
$missing = @($expectedNames | Where-Object { $actualNames -notcontains $_ })
$extra = @($actualNames | Where-Object { $expectedNames -notcontains $_ })
Expect ($missing.Count -eq 0) "missing target fields: $($missing -join ', ')"
Expect ($extra.Count -eq 0) "unexpected target fields: $($extra -join ', ')"

$procId = Get-E2EProcessId "amte2e-amt3241-pl-tgt" $tgtOrg
Expect ([bool]$procId) "target process amte2e-amt3241-pl-tgt should exist"
if ($procId) {
    $storyType = (Invoke-E2ERest "_apis/work/processes/$procId/workitemtypes" -Org $tgtOrg).value | Where-Object { $_.name -eq "User Story" -and $_.customization -ne "system" }
    $witFields = (Invoke-E2ERest "_apis/work/processes/$procId/workitemtypes/$($storyType.referenceName)/fields?`$expand=all" -Org $tgtOrg).value | Where-Object referenceName -like "Custom.amte2eAmt3241*"
    foreach ($w in $witFields) { "User Story field $($w.referenceName): default=[$($w.defaultValue)] allowed=[$($w.allowedValues -join ', ')]" }
    Expect ((@($witFields) | Where-Object referenceName -eq "Custom.amte2eAmt3241Colour").defaultValue -eq "Green") "User Story Colour default should be Green"
}

$ids = try { (Invoke-E2ERest "amte2e-amt3241-tgt/_apis/wit/wiql" -Org $tgtOrg -Method POST -Body @{ query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = 'amte2e-amt3241-tgt'" }).workItems.id } catch { Write-Warning "target project amte2e-amt3241-tgt not created yet"; @() }
$story = $null
foreach ($id in $ids) {
    $wi = Invoke-E2ERest "_apis/wit/workitems/$id" -Org $tgtOrg
    $vals = $wi.fields.PSObject.Properties | Where-Object Name -like "Custom.amte2eAmt3241*" | ForEach-Object { "$($_.Name.Replace('Custom.amte2eAmt3241',''))=$($_.Value)" }
    "work item $id [$($wi.fields.'System.Title')]: $($vals -join '; ')"
    if ($wi.fields.'System.Title' -eq "amt3241 picklist story") { $story = $wi }
}
if ($AfterWorkItems) {
    Expect ($null -ne $story) "the migrated 'amt3241 picklist story' should exist in amte2e-amt3241-tgt"
    if ($story) {
        $expected = [ordered]@{ Colour = "Blue"; Points = 5; Tag = "Gamma"; AsText = "High"; Diff = "Three" }
        foreach ($k in $expected.Keys) {
            $actual = $story.fields."Custom.amte2eAmt3241$k"
            Expect ("$actual" -eq "$($expected[$k])") "work item $k`: expected $($expected[$k]), got $actual"
        }
    }
}

if ($Check) {
    if ($failures.Count -gt 0) { throw "picklists assert failed:`n - $($failures -join "`n - ")" }
    "picklists assert passed"
}
