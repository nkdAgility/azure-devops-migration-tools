#Requires -Version 7.0
# statefields3270 assertions (#3270): for every source revision, the target revision with the same ChangedDate has the
# same State, Reason, state-stamped dates and By fields; the target has no extra (fix-up) revisions; the final state matches.
param([string]$Src = "amte2e-3270-src", [string]$Tgt = "amte2e-3270-tgt", [string]$OutFile)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$fields = "System.State", "System.Reason",
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
function T($rev) { ([datetime]$rev.fields.'System.ChangedDate').ToUniversalTime() }
function K($rev) { (T $rev).ToString("o") }
function Compare-Rev($sr, $tr, [string[]]$names, [string]$label) {
    foreach ($f in $names) {
        $sv = V $sr $f; $tv = V $tr $f
        # Ordinal, as the processor compares: PowerShell's -ne ignores case.
        if (-not [string]::Equals($sv, $tv, [StringComparison]::Ordinal)) { "src rev $($sr.rev)/tgt rev $($tr.rev)$label $($f -replace '^.*\.',''): src '$sv' tgt '$tv'" }
    }
}
$stamped = $fields | Where-Object { $_ -notin "System.State", "System.Reason" }
$r = @()
# The source kept the seeded identity, so a target showing the migration account cannot pass by matching a source that does too.
foreach ($k in "s1", "s2", "s3", "s6", "s7", "s8") {
    $act = @(Get-E2ERevisions $s.$k -Org nkdagility-adomt-source) | Where-Object { $_.fields.'Microsoft.VSTS.Common.ActivatedBy' } | Select-Object -First 1
    $by = if ($act) { $act.fields.'Microsoft.VSTS.Common.ActivatedBy'.displayName } else { "" }
    $r += New-E2EResult "$k source ActivatedBy is the seeded identity" ($by -ceq $s.ghost -or $by -ceq ($s.ghost -replace ' <.*$', '')) "'$by'"
}
foreach ($k in "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8") {
    $id = $s.$k
    $t = Find-E2ETargetItem $Tgt $id -Org nkdagility-adomt-target
    if (-not $t) { $r += New-E2EResult "$k src #$id migrated" $false "no target item"; continue }
    $sRevs = @(Get-E2ERevisions $id -Org nkdagility-adomt-source)
    $tRevs = @(Get-E2ERevisions $t.id -Org nkdagility-adomt-target)
    # Match in order, each target revision used once. A type change is replayed as a REST save dated 3 ms before the
    # source revision, then the object-model save at the source date; both must carry the source's stamped values.
    $bad = @(); $extra = @(); $typeChange = 0; $i = 0
    foreach ($sr in $sRevs) {
        while ($i -lt $tRevs.Count -and (T $tRevs[$i]) -lt (T $sr)) {
            $tr = $tRevs[$i]
            if (((T $sr) - (T $tr)).TotalMilliseconds -le 10 -and $tr.fields.'System.WorkItemType' -eq $sr.fields.'System.WorkItemType') {
                $typeChange++; $bad += @(Compare-Rev $sr $tr $stamped " (type change)")
            } else { $extra += $tr }
            $i++
        }
        if ($i -ge $tRevs.Count -or (K $tRevs[$i]) -ne (K $sr)) { $bad += "src rev $($sr.rev) $(K $sr): no target revision"; continue }
        $bad += @(Compare-Rev $sr $tRevs[$i] $fields ""); $i++
    }
    $extra += @($tRevs | Select-Object -Skip $i)
    $r += New-E2EResult "$k src #$id -> tgt #$($t.id) every revision: state, reason, dates, By fields" ($bad.Count -eq 0) $(if ($bad) { $bad -join "; " } else { "$($sRevs.Count) revisions match" + $(if ($typeChange) { ", plus $typeChange type-change save(s)" } else { "" }) })
    $r += New-E2EResult "$k tgt #$($t.id) no extra (fix-up) revisions" ($extra.Count -eq 0 -and $tRevs.Count -eq $sRevs.Count + $typeChange) $(if ($extra) { ($extra | ForEach-Object { "rev $($_.rev) $(K $_)" }) -join "; " } else { "$($tRevs.Count) target = $($sRevs.Count) source + $typeChange type change" })
    $r += New-E2EResult "$k final state" ((V $sRevs[-1] "System.State") -ceq (V $tRevs[-1] "System.State")) "src '$(V $sRevs[-1] 'System.State')' tgt '$(V $tRevs[-1] 'System.State')'"
}
$tgtItems = @(Get-E2EAllWorkItems $Tgt -Org nkdagility-adomt-target)
$srcItems = @(Get-E2EAllWorkItems $Src -Org nkdagility-adomt-source)
$r += New-E2EResult "target item count equals source (no duplicates)" ($tgtItems.Count -eq $srcItems.Count) "$($tgtItems.Count) target, $($srcItems.Count) source"
exit (Write-E2EResults $r $OutFile)

