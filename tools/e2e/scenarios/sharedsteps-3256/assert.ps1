# sharedsteps-3256 assertions.
#   -Expect Unresolved (default): after config.json, which migrates only the Test Case. The Test Case keeps the source
#                       ids, and the run reports each reference (a warning and a reference-left-on-source review entry).
#   -Expect Resolved:   after a re-run with config-all.json. The Test Case points at the target Shared Steps and
#                       Shared Parameter.
# -LogDirectory defaults to the newest folder Invoke-MigrationE2E.ps1 wrote when it was given none, among those whose
# run log names this scenario's target project (other runs may be writing folders there at the same time).
param([string]$Src = "amte2e-3256-src", [string]$Tgt = "amte2e-3256-tgt", [string]$OutFile,
    [string]$LogDirectory, [ValidateSet("Unresolved", "Resolved")][string]$Expect = "Unresolved", [string]$StateDir = $PSScriptRoot)
$ErrorActionPreference = "Stop"
if (-not $LogDirectory) {
    $LogDirectory = Get-ChildItem ([IO.Path]::GetTempPath()) -Directory -Filter "amt-e2e-logs-*" | Sort-Object LastWriteTime -Descending |
        Where-Object { Get-ChildItem $_.FullName -Recurse -Filter "migration2*.log" | Select-String -SimpleMatch "Found target project as $Tgt" -List } |
        Select-Object -First 1 -ExpandProperty FullName
}
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$s = Read-E2EState $StateDir
$r = @()
$org = "nkdagility-adomt-target"

$items = @(Get-E2EAllWorkItems $Tgt -Org $org | ForEach-Object { Invoke-E2ERest "_apis/wit/workitems/$($_.id)?`$expand=all" -Org $org })
$ttc = @($items | Where-Object { $_.fields.'System.WorkItemType' -eq "Test Case" })
$r += New-E2EResult "exactly one Test Case" ($ttc.Count -eq 1) "$($ttc.Count) item(s)"
$ttc = $ttc | Select-Object -First 1
$steps = $ttc.fields.'Microsoft.VSTS.TCM.Steps'
$data = $ttc.fields.'Microsoft.VSTS.TCM.LocalDataSource'
$ids = @(($data | ConvertFrom-Json).sharedParameterDataSetIds)

$runLog = Get-ChildItem $LogDirectory -Recurse -Filter "migration2*.log" | Sort-Object LastWriteTime | Select-Object -Last 1
$review = Get-ChildItem $LogDirectory -Recurse -Filter "work-items-to-review.jsonl" | Sort-Object LastWriteTime | Select-Object -Last 1
$entries = if ($review) { @(Get-Content $review.FullName | ForEach-Object { $_ | ConvertFrom-Json }) } else { @() }
$refs = @($entries | Where-Object { $_.category -eq "reference-left-on-source" })
$text = if ($runLog) { Get-Content $runLog.FullName -Raw } else { "" }

if ($Expect -eq "Unresolved") {
    $r += New-E2EResult "steps still hold source Shared Steps id" ($steps -match "ref=""$($s.ss)""") $steps
    $r += New-E2EResult "data source still holds source Shared Parameter id" ($ids -contains $s.sp) $data
    $r += New-E2EResult "warning for Shared Steps" ($text -match "Shared Steps $($s.ss) used by Test Case $($s.tc) \(target \d+\) not remapped") "run log $($runLog.FullName)"
    $r += New-E2EResult "warning for Shared Parameter" ($text -match "Shared Parameter $($s.sp) used by Test Case $($s.tc) \(target \d+\) not remapped") "run log $($runLog.FullName)"
    $r += New-E2EResult "two reference-left-on-source entries" ($refs.Count -eq 2) "$($refs.Count) entr(ies)"
    $r += New-E2EResult "entries name the saved target Test Case" (@($refs | Where-Object { "$($_.targetId)" -ne "$($ttc.id)" }).Count -eq 0) "targetId(s) $(($refs | ForEach-Object targetId) -join ',') vs $($ttc.id)"
    $r += New-E2EResult "summary counts them" ($text -match "reference-left-on-source: 2") "summary"
} else {
    $tss = $items | Where-Object { $_.fields.'System.WorkItemType' -eq "Shared Steps" } | Select-Object -First 1
    $tsp = $items | Where-Object { $_.fields.'System.WorkItemType' -eq "Shared Parameter" } | Select-Object -First 1
    $r += New-E2EResult "steps point at target Shared Steps" ($tss -and $steps -match "ref=""$($tss.id)""") $steps
    $r += New-E2EResult "data source points at target Shared Parameter" ($tsp -and $ids.Count -ge 1 -and -not ($ids | Where-Object { $_ -ne $tsp.id })) $data
    $r += New-E2EResult "no reference-left-on-source entries" ($refs.Count -eq 0) "$($refs.Count) entr(ies)"
}

exit (Write-E2EResults $r $OutFile)
