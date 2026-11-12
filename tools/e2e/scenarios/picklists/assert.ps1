# Reports the target state the picklists scenario checks: each field's picklist (type, suggested, values), the User Story
# field defaults in the migrated process, and the migrated work item's values. Read-only.
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$tgtOrg = "nkdagility-adomt-target"
Set-E2EOrg $tgtOrg
$fields = (Invoke-E2ERest "_apis/wit/fields" -Org $tgtOrg).value | Where-Object referenceName -like "Custom.amte2eAmt3241*" | Sort-Object referenceName
foreach ($f in $fields) {
    $list = if ($f.picklistId) { Invoke-E2ERest "_apis/work/processes/lists/$($f.picklistId)" -Org $tgtOrg } else { $null }
    $desc = if ($list) { "picklist $($list.type) suggested=$($list.isSuggested) [$($list.items -join ', ')]" } else { "NO PICKLIST" }
    "field $($f.referenceName): type=$($f.type) isPicklist=$($f.isPicklist) $desc"
}
$procId = Get-E2EProcessId "amte2e-amt3241-pl-tgt" $tgtOrg
if ($procId) {
    $story = (Invoke-E2ERest "_apis/work/processes/$procId/workitemtypes" -Org $tgtOrg).value | Where-Object { $_.name -eq "User Story" -and $_.customization -ne "system" }
    $witFields = (Invoke-E2ERest "_apis/work/processes/$procId/workitemtypes/$($story.referenceName)/fields?`$expand=all" -Org $tgtOrg).value | Where-Object referenceName -like "Custom.amte2eAmt3241*"
    foreach ($w in $witFields) { "User Story field $($w.referenceName): default=[$($w.defaultValue)] allowed=[$($w.allowedValues -join ', ')]" }
}
$ids = try { (Invoke-E2ERest "amte2e-amt3241-tgt/_apis/wit/wiql" -Org $tgtOrg -Method POST -Body @{ query = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = 'amte2e-amt3241-tgt'" }).workItems.id } catch { "target project amte2e-amt3241-tgt not created yet" }
foreach ($id in $ids) {
    $wi = Invoke-E2ERest "_apis/wit/workitems/$id" -Org $tgtOrg
    $vals = $wi.fields.PSObject.Properties | Where-Object Name -like "Custom.amte2eAmt3241*" | ForEach-Object { "$($_.Name.Replace('Custom.amte2eAmt3241',''))=$($_.Value)" }
    "work item $id [$($wi.fields.'System.Title')]: $($vals -join '; ')"
}
