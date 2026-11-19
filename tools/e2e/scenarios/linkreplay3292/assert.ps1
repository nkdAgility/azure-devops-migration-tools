#Requires -Version 7.0
# Checks every seeded item reached the target with all its source revisions and every child is under its final parent.
param([string]$Tgt = "amte2e-3292-tgt", [string]$Log)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$final = [ordered]@{ K1 = "B1"; K2 = "C2"; K3 = "Q3"; E1 = "M4"; E2 = "M4"; E3 = "M4"; E4 = "M4" }
$fail = 0
$targets = @{}
foreach ($p in $s.PSObject.Properties) {
    $srcRevs = @(Get-E2ERevisions $p.Value -Org nkdagility-adomt-source).Count
    $t = Find-E2ETargetItem $Tgt $p.Value
    if (-not $t) { "FAIL $($p.Name) ($($p.Value)) not in target"; $fail++; continue }
    $targets[$p.Name] = $t
    $tgtRevs = @(Get-E2ERevisions $t.id -Org nkdagility-adomt-target).Count
    $ok = $tgtRevs -ge $srcRevs
    if (-not $ok) { $fail++ }
    "{0} {1} {2} -> {3}: source revisions {4}, target revisions {5}" -f ($ok ? "PASS" : "FAIL"), $p.Name, $p.Value, $t.id, $srcRevs, $tgtRevs
}
foreach ($c in $final.Keys) {
    $t = $targets[$c]; $want = $targets[$final[$c]]
    if (-not $t -or -not $want) { "FAIL $c parent cannot be checked"; $fail++; continue }
    $parents = @($t.relations | Where-Object rel -eq "System.LinkTypes.Hierarchy-Reverse" | ForEach-Object { [int]($_.url -split '/')[-1] })
    $ok = $parents.Count -eq 1 -and $parents[0] -eq $want.id
    if (-not $ok) { $fail++ }
    "{0} {1} parent {2} (want {3} = {4})" -f ($ok ? "PASS" : "FAIL"), $c, ($parents -join ","), $final[$c], $want.id
}
if ($Log) {
    $text = Get-ChildItem $Log -Recurse -File | Get-Content
    $ex = @($text | Select-String -Pattern 'Exception|   at |\[ERR\]|\[ERROR\]' | Where-Object { $_ -notmatch '\[DBG\]' })
    $skips = @($text | Select-String -Pattern '\[SKIP\] Child link')
    "Log: $($ex.Count) exception/error lines outside Debug, $($skips.Count) skipped child-link warnings"
    $ex | Select-Object -First 10 | ForEach-Object { "  $_" }
    if ($ex.Count) { $fail++ }
}
"RESULT: $(if ($fail) { "FAIL ($fail)" } else { "PASS" })"
