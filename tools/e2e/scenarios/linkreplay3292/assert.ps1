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
    $srcDates = @(Get-E2ERevisions $p.Value -Org nkdagility-adomt-source | ForEach-Object { ([datetime]$_.fields.'System.ChangedDate').ToUniversalTime().ToString("o") })
    $t = Find-E2ETargetItem $Tgt $p.Value
    if (-not $t) { "FAIL $($p.Name) ($($p.Value)) not in target"; $fail++; continue }
    $targets[$p.Name] = $t
    # Each source revision is replayed with its ChangedDate; extra target revisions (a child moved to its parent) are allowed.
    $tgtDates = @(Get-E2ERevisions $t.id -Org nkdagility-adomt-target | ForEach-Object { ([datetime]$_.fields.'System.ChangedDate').ToUniversalTime().ToString("o") })
    $missing = @($srcDates | Where-Object { $tgtDates -notcontains $_ })
    $ok = $missing.Count -eq 0
    if (-not $ok) { $fail++ }
    "{0} {1} {2} -> {3}: {4} source revisions, {5} target revisions, missing: {6}" -f ($ok ? "PASS" : "FAIL"), $p.Name, $p.Value, $t.id, $srcDates.Count, $tgtDates.Count, ($missing -join ", ")
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
    # A log event is its header line plus the exception lines after it; only the header carries the level.
    $events = [System.Collections.Generic.List[string]]::new()
    foreach ($line in (Get-ChildItem $Log -Recurse -File | Get-Content)) {
        if ($line.StartsWith("[") -or $events.Count -eq 0) { $events.Add($line) } else { $events[$events.Count - 1] += "`n$line" }
    }
    $kept = @($events | Where-Object { $_ -notmatch '^\[[^\]]* DBG\]' })
    $ex = @($kept | Where-Object { $_ -match 'Exception|\n   at |\[ERR\]|\[ERROR\]' })
    $skips = @($kept | Where-Object { $_ -match 'WRN\].*\[SKIP\] Child link' })
    "Log: $($ex.Count) exception/error events outside Debug, $($skips.Count) skipped child-link warnings"
    $ex | Select-Object -First 10 | ForEach-Object { "  " + ($_ -split "`n")[0] }
    if ($ex.Count) { $fail++ }
}
"RESULT: $(if ($fail) { "FAIL ($fail)" } else { "PASS" })"
if ($fail) { exit 1 }
