# identity assertions: every source revision is paired with its target revision (after collapsing date fix-up saves, which are
# dated <= 1 ms after the revision they fix) and must carry the same ChangedDate, identities and state dates.
param([string]$Tgt = "amte2e-identity-tgt", [string]$OutFile)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$fields = "System.State", "System.AssignedTo", "Microsoft.VSTS.Common.ActivatedBy", "Microsoft.VSTS.Common.ResolvedBy", "Microsoft.VSTS.Common.ClosedBy",
    "Microsoft.VSTS.Common.ActivatedDate", "Microsoft.VSTS.Common.ResolvedDate", "Microsoft.VSTS.Common.ClosedDate"
function Get-Val($f, $name) { $v = $f.$name; if ($null -eq $v) { return "" }; if ($v.displayName) { return $v.displayName }; if ($v -is [datetime]) { return $v.ToUniversalTime().ToString("o") }; "$v" }
# The identity the migration runs as, which a "copy current user" rule would write.
$runner = (Invoke-E2ERest "_apis/connectionData" -Org nkdagility-adomt-target -ApiVersion "7.1-preview").authenticatedUser
$r = @()
foreach ($k in "i1", "i2", "i3") {
    $t = Find-E2ETargetItem $Tgt $s.$k
    if (-not $t) { $r += New-E2EResult "$k src #$($s.$k) migrated" $false "no target item reflects it"; continue }
    $r += Test-E2EItemFidelity -SourceId $s.$k -SrcProject "amte2e-identity-src" -TgtProject $Tgt | Where-Object { $_.Check -match "revision sequence|dated after|Migration" }
    $sr = @(Get-E2ERevisions $s.$k -Org nkdagility-adomt-source)
    $tr = @(Get-E2ERevisions $t.id -Org nkdagility-adomt-target)
    $collapsed = @(Get-E2ECollapsedRevisions $tr)
    if ($collapsed.Count -ne $sr.Count) { $r += New-E2EResult "$k collapsed target revisions" $false "source $($sr.Count), target $($collapsed.Count) after collapsing fix-ups"; continue }
    $i = 0
    $diffs = foreach ($rev in $sr) {
        $match = $collapsed[$i]; $i++
        $sd = Get-Val $rev.fields "System.ChangedDate"; $td = Get-Val $match.fields "System.ChangedDate"
        if ($sd -ne $td) { "src rev $($rev.rev)/tgt rev $($match.rev) ChangedDate source $sd target $td" }
        foreach ($f in $fields) {
            $sv = Get-Val $rev.fields $f; $tv = Get-Val $match.fields $f
            if ($sv -ne $tv) { "src rev $($rev.rev)/tgt rev $($match.rev) $f source '$sv' target '$tv'" }
        }
    }
    $r += New-E2EResult "$k src #$($s.$k) identity and state dates at every revision" (-not $diffs) $(if ($diffs) { $diffs -join "; " } else { "$($sr.Count) source revisions x $($fields.Count) fields equal" })
    if ($k -eq "i1") {
        $overwritten = @($tr | Where-Object { $_.rev -gt 2 -and $_.fields.'System.AssignedTo'.id -eq $runner.id })
        $r += New-E2EResult "i1 rule never replaced the ghost AssignedTo with the migration account after rev 2" ($overwritten.Count -eq 0) "$($overwritten.Count) target revision(s) assigned to the migration account"
    }
}
exit (Write-E2EResults $r $OutFile)
