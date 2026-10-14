# identity assertions: for every source revision, the target revision with the same ChangedDate (the last one, after any
# same-date fix-up save) must carry the same identity and state-date values.
param([string]$Tgt = "amte2e-identity-tgt", [string]$OutFile)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$fields = "System.State", "System.AssignedTo", "Microsoft.VSTS.Common.ActivatedBy", "Microsoft.VSTS.Common.ResolvedBy", "Microsoft.VSTS.Common.ClosedBy",
    "Microsoft.VSTS.Common.ActivatedDate", "Microsoft.VSTS.Common.ResolvedDate", "Microsoft.VSTS.Common.ClosedDate"
function Get-Val($f, $name) { $v = $f.$name; if ($null -eq $v) { return "" }; if ($v.displayName) { return $v.displayName }; if ($v -is [datetime]) { return $v.ToString("yyyy-MM-dd HH:mm:ss") }; "$v" }
$r = @()
foreach ($k in "i1", "i2", "i3") {
    $t = Find-E2ETargetItem $Tgt $s.$k
    $r += Test-E2EItemFidelity -SourceId $s.$k -SrcProject "amte2e-identity-src" -TgtProject $Tgt | Where-Object { $_.Check -match "revision sequence|dated after|Migration" }
    $sr = @(Get-E2ERevisions $s.$k -Org nkdagility-adomt-source)
    $tr = @(Get-E2ERevisions $t.id -Org nkdagility-adomt-target)
    # A fix-up save (#3180/#3205) is dated its revision's ChangedDate + 1 ms; it replaces that revision's values.
    $collapsed = [System.Collections.Generic.List[object]]::new()
    foreach ($x in $tr) {
        $dt = [datetime]$x.fields.'System.ChangedDate'
        if ($collapsed.Count -and ($dt - [datetime]$collapsed[-1].fields.'System.ChangedDate').TotalMilliseconds -le 1) { $collapsed[$collapsed.Count - 1] = $x } else { $collapsed.Add($x) }
    }
    if ($collapsed.Count -ne $sr.Count) { $r += New-E2EResult "$k collapsed target revisions" $false "source $($sr.Count), target $($collapsed.Count) after collapsing fix-ups"; continue }
    $i = 0
    $diffs = foreach ($rev in $sr) {
        $match = $collapsed[$i]; $i++
        $sd = ([datetime]$rev.fields.'System.ChangedDate').ToString("HH:mm:ss.fff"); $td = ([datetime]$match.fields.'System.ChangedDate').ToString("HH:mm:ss.fff")
        if ($sd -ne $td) { "src rev $($rev.rev)/tgt rev $($match.rev) ChangedDate source $sd target $td" }
        foreach ($f in $fields) {
            $sv = Get-Val $rev.fields $f; $tv = Get-Val $match.fields $f
            if ($sv -ne $tv) { "src rev $($rev.rev)/tgt rev $($match.rev) $f source '$sv' target '$tv'" }
        }
    }
    $r += New-E2EResult "$k src #$($s.$k) identity and state dates at every revision" (-not $diffs) $(if ($diffs) { $diffs -join "; " } else { "$($sr.Count) source revisions x $($fields.Count) fields equal" })
    $ghostOverwritten = $tr | Where-Object { (Get-Val $_.fields "System.AssignedTo") -match "Martin" -and $k -eq "i1" -and $_.rev -gt 2 }
    if ($k -eq "i1") { $r += New-E2EResult "i1 rule never replaced the ghost AssignedTo after rev 2" (-not $ghostOverwritten) "$(@($ghostOverwritten).Count) target revision(s) with the current user" }
}
$lines = @(Write-E2EResults $r $OutFile); $lines[0..($lines.Count - 2)]; exit $lines[-1]
