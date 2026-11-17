# tags assertions: each target item's final tags, and no target revision ever carried a tag the filter should drop.
param([string]$Src = "amte2e-3264-src", [string]$Tgt = "amte2e-3264-tgt", [string]$OutFile)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$s = Read-E2EState $PSScriptRoot

function Tags([string]$Value) { @(($Value -split ';') | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Sort-Object) }

$expected = [ordered]@{ t1 = @("Approved", "BugBash"); t2 = @("Team-A"); t3 = @("Release"); t4 = @("S12") }
$allowed = @("Approved", "BugBash", "Team-A", "Release", "S12")
$r = @()
foreach ($k in $expected.Keys) {
    $wi = Find-E2ETargetItem -Project $Tgt -SourceId $s.$k
    if (-not $wi) { $r += New-E2EResult "$k migrated" $false "no target item for source $($s.$k)"; continue }
    $actual = Tags $wi.fields.'System.Tags'
    $want = @($expected[$k] | Sort-Object)
    $r += New-E2EResult "$k final tags" (($actual -join ';') -ceq ($want -join ';')) "target $($wi.id): '$($actual -join '; ')' expected '$($want -join '; ')'"
    $revs = @(Get-E2ERevisions $wi.id -Org nkdagility-adomt-target)
    if ($k -eq "t3") {
        # T3's first source revision has only the dropped tag, so the replayed history must show no tags before Release.
        $sequence = @($revs | ForEach-Object { (Tags $_.fields.'System.Tags') -join ';' } | Get-Unique)
        $r += New-E2EResult "t3 history replayed (no tags, then Release)" (($sequence -join ' > ') -ceq ' > Release') "tag sequence: '$($sequence -join "' > '")'"
    }
    $bad = foreach ($rev in $revs) {
        Tags $rev.fields.'System.Tags' | Where-Object { $allowed -cnotcontains $_ } | ForEach-Object { "rev $($rev.rev): $_" }
    }
    $r += New-E2EResult "$k no revision has a dropped tag" (@($bad).Count -eq 0) ($(if ($bad) { $bad -join ', ' } else { 'none' }))
}
$tgtItems = @(Get-E2EAllWorkItems $Tgt -Org nkdagility-adomt-target)
$r += New-E2EResult "target item count (no duplicates)" ($tgtItems.Count -eq 4) "$($tgtItems.Count) items"
exit (Write-E2EResults $r $OutFile)
