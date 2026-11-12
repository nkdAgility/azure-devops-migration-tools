# sharedsteps assertions: one target item per source item, Test Case steps and parameters point at the target
# Shared Steps and Shared Parameter, and both carry the source hyperlink that lets a re-run find them.
param([string]$Src = "amte2e-ss-src", [string]$Tgt = "amte2e-ss-tgt", [string]$OutFile)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()
$org = "nkdagility-adomt-target"

$items = @(Get-E2EAllWorkItems $Tgt -Org $org | ForEach-Object { Invoke-E2ERest "_apis/wit/workitems/$($_.id)?`$expand=all" -Org $org })
foreach ($t in "Shared Steps", "Shared Parameter", "Test Case") {
    $n = @($items | Where-Object { $_.fields.'System.WorkItemType' -eq $t }).Count
    $r += New-E2EResult "exactly one $t (no duplicates)" ($n -eq 1) "$n item(s)"
}
$tss = $items | Where-Object { $_.fields.'System.WorkItemType' -eq "Shared Steps" } | Select-Object -First 1
$tsp = $items | Where-Object { $_.fields.'System.WorkItemType' -eq "Shared Parameter" } | Select-Object -First 1
$ttc = $items | Where-Object { $_.fields.'System.WorkItemType' -eq "Test Case" } | Select-Object -First 1

foreach ($pair in @(@($tss, $s.ss), @($tsp, $s.sp))) {
    $hl = @($pair[0].relations | Where-Object { $_.rel -eq "Hyperlink" -and $_.attributes.comment -eq "Migrated from (ReflectedWorkItemId)" })
    $ok = $hl.Count -eq 1 -and $hl[0].url -match "/_workitems/edit/$($pair[1])$"
    $r += New-E2EResult "$($pair[0].fields.'System.WorkItemType') has one source hyperlink" $ok "$($hl.Count): $($hl.url -join ', ')"
}
$steps = $ttc.fields.'Microsoft.VSTS.TCM.Steps'
$r += New-E2EResult "test case compref points at target shared steps" ($steps -match "ref=""$($tss.id)""") $(if ($steps -match 'ref="(\d+)"') { "ref=$($matches[1]), target SS $($tss.id)" } else { "no compref" })
$data = $ttc.fields.'Microsoft.VSTS.TCM.LocalDataSource'
$r += New-E2EResult "test case data source points at target shared parameter" ($data -match """sharedParameterDataSetIds"":\[$($tsp.id)\]") "$data"
$ssLink = @($ttc.relations | Where-Object { $_.rel -eq "Microsoft.VSTS.TestCase.SharedStepReferencedBy-Reverse" -and $_.url -match "/$($tss.id)$" })
$r += New-E2EResult "test case links to target shared steps" ($ssLink.Count -eq 1) "$($ssLink.Count) link(s)"
$spLink = @($ttc.relations | Where-Object { $_.rel -eq "Microsoft.VSTS.TestCase.SharedParameterReferencedBy-Reverse" -and $_.url -match "/$($tsp.id)$" })
$r += New-E2EResult "test case links to target shared parameter" ($spLink.Count -eq 1) "$($spLink.Count) link(s)"

Write-E2EResults $r $OutFile
