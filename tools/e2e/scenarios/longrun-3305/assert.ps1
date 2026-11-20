#Requires -Version 7.0
# longrun-3305 assertions (#3305): for every source item, the target item exists, and every source revision has a target
# revision with the same ChangedDate whose State and every state-stamped field match the source, over REST. Reports each
# item whose target has an empty value where the source has one, in migration order, so the point where it starts shows.
param([string]$Src = "amt3305-long-src", [string]$Tgt = "amt3305-long-tgt", [string]$OutFile,
    # The migration order of the config's query: config.json orders by ChangedDate descending, config-chain.json by Id.
    [ValidateSet("ChangedDateDesc", "Id")][string]$Order = "ChangedDateDesc")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
$fields = "System.State", "Microsoft.VSTS.Common.StateChangeDate",
    "Microsoft.VSTS.Common.ActivatedDate", "Microsoft.VSTS.Common.ResolvedDate", "Microsoft.VSTS.Common.ClosedDate",
    "Microsoft.VSTS.Common.ActivatedBy", "Microsoft.VSTS.Common.ResolvedBy", "Microsoft.VSTS.Common.ClosedBy"
function V($rev, [string]$f) {
    $v = $rev.fields.$f
    if ($null -eq $v) { return "" }
    if ($v -is [datetime]) { return $v.ToUniversalTime().ToString("o") }
    if ($v.PSObject.Properties["uniqueName"]) { return "$($v.displayName) <$($v.uniqueName)>" }
    if ($f -like "*Date") { return ([datetime]$v).ToUniversalTime().ToString("o") }
    "$v"
}
function K($rev) { ([datetime]$rev.fields.'System.ChangedDate').ToUniversalTime().ToString("o") }

$srcItems = @(Get-E2EAllWorkItems $Src -Org nkdagility-adomt-source)
$tgtItems = @(Get-E2EAllWorkItems $Tgt -Org nkdagility-adomt-target)
$byReflected = @{}
foreach ($t in $tgtItems) { if ($t.fields.'Custom.ReflectedWorkItemId' -match '/_workitems/edit/(\d+)$') { $byReflected[[int]$Matches[1]] = $t } }
$ordered = if ($Order -eq "Id") { @($srcItems | Sort-Object { [int]$_.id }) } else { @($srcItems | Sort-Object { [datetime]$_.fields.'System.ChangedDate' } -Descending) }
$r = @(); $empty = @(); $mismatch = @(); $current = @(); $revCount = 0; $missing = @()
for ($n = 0; $n -lt $ordered.Count; $n++) {
    $s = $ordered[$n]
    $t = $byReflected[[int]$s.id]
    if (-not $t) { $missing += $s.id; continue }
    $sRevs = @(Get-E2ERevisions $s.id -Org nkdagility-adomt-source)
    $tRevs = @(Get-E2ERevisions $t.id -Org nkdagility-adomt-target)
    $tByDate = @{}; foreach ($tr in $tRevs) { $tByDate[(K $tr)] = $tr }
    foreach ($sr in $sRevs) {
        $revCount++
        $tr = $tByDate[(K $sr)]
        if (-not $tr) { $mismatch += "src #$($s.id) rev $($sr.rev): no target revision dated $(K $sr)"; continue }
        foreach ($f in $fields) {
            $sv = V $sr $f; $tv = V $tr $f
            if ([string]::Equals($sv, $tv, [StringComparison]::Ordinal)) { continue }
            $line = "order $($n + 1) src #$($s.id) rev $($sr.rev) -> tgt #$($t.id) rev $($tr.rev) $($f -replace '^.*\.',''): src '$sv' tgt '$tv'"
            if ($tv -eq "" -and $sv -ne "") { $empty += $line } else { $mismatch += $line }
        }
    }
    # The current values too: a later target revision (a catch-up save) is never paired with a source revision.
    foreach ($f in $fields) {
        $sv = V $sRevs[-1] $f; $tv = V $tRevs[-1] $f
        if (-not [string]::Equals($sv, $tv, [StringComparison]::Ordinal)) { $current += "src #$($s.id) -> tgt #$($t.id) current $($f -replace '^.*\.',''): src '$sv' tgt '$tv'" }
    }
}
$emptyItems = @($empty | ForEach-Object { if ($_ -match 'src #(\d+)') { $Matches[1] } } | Select-Object -Unique)
$first = if ($empty) { $empty[0] -replace ' rev .*$', '' } else { "none" }
"Compared $($ordered.Count) source items and $revCount source revisions."
$r += New-E2EResult "the fixture has work items and revisions to compare" ($srcItems.Count -gt 0 -and $revCount -gt 0) "$($srcItems.Count) source items, $revCount revisions compared"
$r += New-E2EResult "every source item migrated" ($missing.Count -eq 0) $(if ($missing) { "missing: $($missing -join ', ')" } else { "$($ordered.Count) items" })
$r += New-E2EResult "target item count equals source (no duplicates)" ($tgtItems.Count -eq $srcItems.Count) "$($tgtItems.Count) target, $($srcItems.Count) source"
$r += New-E2EResult "no empty state-stamped value where the source has one" ($empty.Count -eq 0) $(if ($empty) { "$($empty.Count) values on $($emptyItems.Count) items, first at $first; " + (($empty | Select-Object -First 20) -join "; ") } else { "$revCount revisions" })
$r += New-E2EResult "every state-stamped value matches the source" ($mismatch.Count -eq 0) $(if ($mismatch) { "$($mismatch.Count): " + (($mismatch | Select-Object -First 20) -join "; ") } else { "$revCount revisions" })
$r += New-E2EResult "every current target value matches the current source value" ($current.Count -eq 0) $(if ($current) { "$($current.Count): " + (($current | Select-Object -First 20) -join "; ") } else { "$($ordered.Count - $missing.Count) items" })
exit (Write-E2EResults $r $OutFile)
