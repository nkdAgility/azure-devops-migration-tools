#Requires -Version 7.0
# rerun-3326 assertions (#3326), after the first migration:
#  1. every current state-stamped value on the target equals the source;
#  2. removes the commit links (T1, T2, S2, T4) and the parent link (S1) on the target, and stamps T4's ClosedDate in a
#     revision shaped like a re-run's catch-up save (author kept, previous date + 1 s), as main's repair save did;
#  3. re-runs: exit 0, and every current state-stamped value equals the source again (T4 corrected);
#  4. re-runs again: exit 0, no new target revision, nothing updated (the unresolved links are not repaired every run);
#  5. stamps T5's ClosedDate in an ordinary revision (not a catch-up save) and re-runs: the item fails with "stamped
#     instead of the source values", the run exits non-zero, and the log has no stack trace.
# Migrations use the built tool through Invoke-MigrationE2E.ps1, with this scenario's config pointed at -Src/-Tgt.
# -BuildOutput runs another build (for example main's), as Invoke-MigrationE2E.ps1 takes it.
param([string]$Src = "amt3326-rsrc", [string]$Tgt = "amt3326-rtgt", [string]$OutFile, [string]$BuildOutput)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
$so = "nkdagility-adomt-source"; $to = "nkdagility-adomt-target"
$s = Read-E2EState $PSScriptRoot
$fields = "Microsoft.VSTS.Common.ClosedDate", "Microsoft.VSTS.Common.ClosedBy", "Microsoft.VSTS.Common.ResolvedDate", "Microsoft.VSTS.Common.ResolvedBy",
    "Microsoft.VSTS.Common.ActivatedDate", "Microsoft.VSTS.Common.ActivatedBy", "Microsoft.VSTS.Common.StateChangeDate", "System.State"
function V($wi, [string]$f) {
    $v = $wi.fields.$f
    if ($null -eq $v) { return "" }
    if ($v.PSObject.Properties["uniqueName"]) { return "$($v.uniqueName)" }
    if ($f -like "*Date") { return ([datetime]$v).ToUniversalTime().ToString("o") }
    "$v"
}
$targets = [ordered]@{}
foreach ($p in $s.PSObject.Properties) { $targets[$p.Name] = (Find-E2ETargetItem $Tgt $p.Value).id }
function Compare-Current {
    $diff = @()
    foreach ($k in $targets.Keys) {
        if (-not $targets[$k]) { $diff += "$k not in target"; continue }
        $sw = Invoke-E2ERest "_apis/wit/workitems/$($s.$k)" -Org $so
        $tw = Invoke-E2ERest "_apis/wit/workitems/$($targets[$k])" -Org $to
        foreach ($f in $fields) { $a = V $sw $f; $b = V $tw $f; if ($a -ne $b) { $diff += "$k ($($s.$k) -> $($targets[$k])) $($f -replace '^.*\.',''): source '$a' target '$b'" } }
    }
    , $diff
}
function Get-RevCounts { $c = [ordered]@{}; foreach ($k in $targets.Keys) { if ($targets[$k]) { $c[$k] = @(Get-E2ERevisions $targets[$k] -Org $to).Count } }; $c }
$cfg = Join-Path ([IO.Path]::GetTempPath()) "amte2e-3326-$Tgt.json"
(Get-Content (Join-Path $PSScriptRoot "config.json") -Raw).Replace("amt3326-rsrc", $Src).Replace("amt3326-rtgt", $Tgt) | Set-Content $cfg -Encoding utf8
$harness = Join-Path $PSScriptRoot "..\..\Invoke-MigrationE2E.ps1"
function Invoke-Rerun([string]$Name) {
    $dir = Join-Path ([IO.Path]::GetTempPath()) "amte2e-3326-$Tgt-$Name"
    $build = if ($BuildOutput) { @("-BuildOutput", $BuildOutput) } else { @() }
    $out = @(pwsh -NoProfile -File $harness -Config $cfg -LogDirectory $dir @build 2>&1 | ForEach-Object { "$_" })
    $code = $LASTEXITCODE
    $log = @(Get-ChildItem $dir -Recurse -File -Filter *.log | Get-Content)
    [pscustomobject]@{ Exit = $code; Summary = ($out | Where-Object { $_ -match 'Work items:' } | Select-Object -Last 1); Log = $log }
}
function Remove-Relation([int]$Id, [scriptblock]$Match, [System.Collections.IDictionary]$Fields = @{}) {
    $wi = Invoke-E2ERest "_apis/wit/workitems/$Id`?`$expand=relations" -Org $to
    $i = 0; $idx = -1
    foreach ($r in $wi.relations) { if (& $Match $r) { $idx = $i }; $i++ }
    if ($idx -lt 0) { throw "no matching relation on target $Id" }
    Set-E2EWorkItem -Id $Id -Org $to -Fields $Fields -Ops @(@{ op = "remove"; path = "/relations/$idx" }) -BypassRules:($Fields.Count -gt 0) | Out-Null
}
$r = @()
$first = Compare-Current
$r += New-E2EResult "first run: every current state-stamped value equals the source" ($first.Count -eq 0) $(if ($first) { $first -join "; " } else { "$($targets.Count) items" })

Set-E2EOrg $to
$artifact = { param($x) $x.rel -eq "ArtifactLink" }
foreach ($k in "T1", "T2", "S2") { Remove-Relation $targets[$k] $artifact }
Remove-Relation $targets.S1 { param($x) $x.rel -eq "System.LinkTypes.Hierarchy-Reverse" }
# T4: what main's repair save left as the latest revision: dated previous + 1 s by the previous author, ClosedDate
# stamped to the save time. The same revision removes the link, so this run has a repair to make.
$t4 = Invoke-E2ERest "_apis/wit/workitems/$($targets.T4)" -Org $to
$t4Date = ([datetime]$t4.fields.'System.ChangedDate').ToUniversalTime()
Remove-Relation $targets.T4 $artifact ([ordered]@{
    "System.ChangedDate" = $t4Date.AddSeconds(1).ToString("o"); "System.ChangedBy" = $t4.fields.'System.ChangedBy'.uniqueName
    "Microsoft.VSTS.Common.ClosedDate" = (Get-Date).ToUniversalTime().ToString("o") })
$broken = Compare-Current
$r += New-E2EResult "the fixture has a wrongly stamped target before the re-run" ($broken.Count -ge 1) ($broken -join "; ")

$a = Invoke-Rerun "rerun1"
$after = Compare-Current
$r += New-E2EResult "re-run after removing links exits 0" ($a.Exit -eq 0) "exit $($a.Exit); $($a.Summary)"
$r += New-E2EResult "after the repair, every current state-stamped value equals the source (T4 corrected)" ($after.Count -eq 0) $(if ($after) { $after -join "; " } else { "$($targets.Count) items" })

$before = Get-RevCounts
$b = Invoke-Rerun "rerun2"
$afterB = Get-RevCounts
$new = @($before.Keys | Where-Object { $afterB[$_] -ne $before[$_] } | ForEach-Object { "$_ $($before[$_]) -> $($afterB[$_])" })
$r += New-E2EResult "a further re-run exits 0" ($b.Exit -eq 0) "exit $($b.Exit); $($b.Summary)"
$r += New-E2EResult "a further re-run saves no revision and updates nothing" ($new.Count -eq 0 -and $b.Summary -match ' 0 updated') $(if ($new) { $new -join "; " } else { "$($b.Summary)" })

# T5: a ClosedDate that is not the source's, in an ordinary revision (3 s after the previous one, not 1 s).
$t5 = Invoke-E2ERest "_apis/wit/workitems/$($targets.T5)" -Org $to
Set-E2EWorkItem -Id $targets.T5 -Org $to -BypassRules -Fields ([ordered]@{
    "System.ChangedDate" = ([datetime]$t5.fields.'System.ChangedDate').ToUniversalTime().AddSeconds(3).ToString("o")
    "Microsoft.VSTS.Common.ClosedDate" = (Get-Date).ToUniversalTime().ToString("o") }) | Out-Null
$c = Invoke-Rerun "rerun3"
$failLine = @($c.Log | Where-Object { $_ -match "Source work item $($s.T5) .*stamped instead of the source values" })
$frames = @($c.Log | Where-Object { $_ -match '^\s+at \S+\(' })
$r += New-E2EResult "a genuinely wrong target fails: the run exits non-zero" ($c.Exit -ne 0) "exit $($c.Exit); $($c.Summary)"
$r += New-E2EResult "the wrong target is named with 'stamped instead of the source values'" ($failLine.Count -gt 0) $(if ($failLine) { ($failLine[0] -replace '^.*Reason: ', '').Substring(0, [Math]::Min(300, ($failLine[0] -replace '^.*Reason: ', '').Length)) } else { "no such line in the log" })
$r += New-E2EResult "no stack trace in the logs" ($frames.Count -eq 0) "$($frames.Count) stack frames"
Remove-Item $cfg -ErrorAction SilentlyContinue
exit (Write-E2EResults $r $OutFile)
