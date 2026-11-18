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
function K($rev) { ([datetime]$rev.fields.'System.ChangedDate').ToUniversalTime().ToString("o") }
$r = @()
foreach ($k in "s1", "s2", "s3", "s4", "s5") {
    $id = $s.$k
    $t = Find-E2ETargetItem $Tgt $id -Org nkdagility-adomt-target
    if (-not $t) { $r += New-E2EResult "$k src #$id migrated" $false "no target item"; continue }
    $sRevs = @(Get-E2ERevisions $id -Org nkdagility-adomt-source)
    $tRevs = @(Get-E2ERevisions $t.id -Org nkdagility-adomt-target)
    # The first target revision with a ChangedDate is the save that replayed it; a later one with the same date is a fix-up.
    $tByDate = @{}; foreach ($tr in $tRevs) { if (-not $tByDate.ContainsKey((K $tr))) { $tByDate[(K $tr)] = $tr } }
    $bad = @()
    foreach ($sr in $sRevs) {
        $tr = $tByDate[(K $sr)]
        if (-not $tr) { $bad += "src rev $($sr.rev) $(K $sr): no target revision"; continue }
        foreach ($f in $fields) {
            $sv = V $sr $f; $tv = V $tr $f
            if ($sv -ne $tv) { $bad += "src rev $($sr.rev)/tgt rev $($tr.rev) $($f -replace '^.*\.',''): src '$sv' tgt '$tv'" }
        }
    }
    $r += New-E2EResult "$k src #$id -> tgt #$($t.id) every revision: state, reason, dates, By fields" ($bad.Count -eq 0) $(if ($bad) { $bad -join "; " } else { "$($sRevs.Count) revisions match" })
    $sKeys = @($sRevs | ForEach-Object { K $_ })
    $extra = @($tRevs | Where-Object { $sKeys -notcontains (K $_) -or $tByDate[(K $_)].rev -ne $_.rev })
    $r += New-E2EResult "$k tgt #$($t.id) no extra (fix-up) revisions" ($extra.Count -eq 0) $(if ($extra) { ($extra | ForEach-Object { "rev $($_.rev) $(K $_)" }) -join "; " } else { "$($tRevs.Count) target = $($sRevs.Count) source" })
    $r += New-E2EResult "$k final state" ((V $sRevs[-1] "System.State") -eq (V $tRevs[-1] "System.State")) "src '$(V $sRevs[-1] 'System.State')' tgt '$(V $tRevs[-1] 'System.State')'"
}
$tgtItems = @(Get-E2EAllWorkItems $Tgt -Org nkdagility-adomt-target)
$srcItems = @(Get-E2EAllWorkItems $Src -Org nkdagility-adomt-source)
$r += New-E2EResult "target item count equals source (no duplicates)" ($tgtItems.Count -eq $srcItems.Count) "$($tgtItems.Count) target, $($srcItems.Count) source"
exit (Write-E2EResults $r $OutFile)

