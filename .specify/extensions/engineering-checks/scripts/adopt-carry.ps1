# Adoption step 6: carries over from the old system. Each item with a live WKR (the old status comment)
# gets its WKR file; the old Definition of Workflow is marked as replaced.
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfg = Get-WorkflowConfig
$ad = Get-BoardAdapter $cfg
$n = 0
foreach ($item in @(& $ad.GetItems)) {
    $status = @($item.comments | Where-Object { "$($_.body)" -like "$($cfg.adopt.wkrMarker)*" }) | Select-Object -Last 1
    if (-not $status) { continue }
    $f = Join-Path (New-ItemFolder '.' $item.id $item.title) 'item.md'
    if (-not (Test-Path $f)) {
        $tag = [regex]::Match("$($status.body)", '<!--\s*(?:worker|wkr)[:=\s]*([^\s>]+)').Groups[1].Value
        $wkr = if ($tag) { $tag } else { "WKR-$($item.id)" }
        "# $wkr`n`nWKR: $wkr`n`n## Carried over`n`n$($status.body)`n" | Set-Content $f -Encoding utf8
        $n++
    }
}
"WKR files created: $n"
$old = $cfg.adopt.oldWorkflow
if ($old -and (Test-Path $old) -and -not (Select-String $old -Pattern 'Replaced by the nkda-speckit' -Quiet)) {
    $body = Get-Content $old -Raw
    "> **Replaced by the nkda-speckit engineering Definition of Workflow.** Kept until this repo meets every intent, then retired.`n`n$body" | Set-Content $old -Encoding utf8
    "marked $old as replaced"
}
$oldJson = if ($old) { [IO.Path]::ChangeExtension($old, '.json') } else { $null }
if ($oldJson -and (Test-Path $oldJson)) {
    $j = Get-Content $oldJson -Raw | ConvertFrom-Json -AsHashtable
    if ($j.ContainsKey('storage') -and $j.storage.ContainsKey('statusField') -and $j.storage.statusField) {
        $j.storage.statusField = $null
        $j['replacedBy'] = 'nkda-speckit engineering Definition of Workflow'
        $j | ConvertTo-Json -Depth 10 | Set-Content $oldJson -Encoding utf8
        "switched off the old tool's Status writes in $oldJson"
    }
}
exit 0