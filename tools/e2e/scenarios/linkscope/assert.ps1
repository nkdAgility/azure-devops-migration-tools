#Requires -Version 7.0
# linkscope assertions: A and B are migrated, A keeps its Related link to B, and the links to X (unreadable, TF26198) and O
# (outside the query) are skipped. The run log must report both skips; the caller checks it and the exit code.
param([string]$OutFile, [Parameter(Mandatory)][string]$LogDirectory)
$ErrorActionPreference = "Stop"
$cfg = Get-Content (Join-Path $PSScriptRoot "config.json") -Raw | ConvertFrom-Json
# Project names come from the scenario config, so the seed, the migration and the assertions always agree.
$Src = $cfg.MigrationTools.Endpoints.Source.Project
$Tgt = $cfg.MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()

$a = Find-E2ETargetItem -Project $Tgt -SourceId $s.a
$b = Find-E2ETargetItem -Project $Tgt -SourceId $s.b
$r += New-E2EResult "A and B migrated" ($a -and $b) "A $($a.id) B $($b.id)"
$tgtItems = @(Get-E2EAllWorkItems $Tgt -Org nkdagility-adomt-target)
$rTarget = Find-E2ETargetItem -Project $Tgt -SourceId $s.r
$r += New-E2EResult "R migrated" ($null -ne $rTarget) "R $($rTarget.id)"
if ($rTarget) {
    $rRelated = @($rTarget.relations | Where-Object rel -eq "System.LinkTypes.Related")
    $r += New-E2EResult "R has no Related link (removed in the source)" ($rRelated.Count -eq 0) "$($rRelated.Count) Related link(s)"
}
$r += New-E2EResult "only A, B and R migrated" ($tgtItems.Count -eq 3) "$($tgtItems.Count) target items"
if ($a -and $b) {
    $related = @($a.relations | Where-Object rel -eq "System.LinkTypes.Related" | ForEach-Object { [int]($_.url -split '/')[-1] })
    $r += New-E2EResult "A links to B" ($related -contains [int]$b.id) "A related to: $($related -join ', ')"
    $r += New-E2EResult "A has no other Related link" ($related.Count -eq 1) "$($related.Count) Related link(s)"
}
$log = Get-ChildItem $LogDirectory -Recurse -Filter "migration2*.log" | Sort-Object LastWriteTime | Select-Object -Last 1
$text = if ($log) { Get-Content $log.FullName -Raw } else { "" }
$r += New-E2EResult "run log found" ($null -ne $log) "LogDirectory $LogDirectory"
$r += New-E2EResult "no NotImplementedException" ($text -notmatch "NotImplementedException") "log $($log.Name)"
$r += New-E2EResult "unreadable target X reported and skipped" ($text -match "\[SKIP\][^\r\n]*to $($s.x) not migrated") ""
$r += New-E2EResult "removal of the link to unreadable X reported and skipped" ($text -match "\[SKIP\][^\r\n]*to $($s.x) not removed from target") ""
$r += New-E2EResult "no stack frames in the log" ($text -notmatch "(?m)^\s+at ") ""
$r += New-E2EResult "out-of-query target O reported and skipped" ($text -match "\[SKIP\][^\r\n]*wiSourceR=$($s.o)[^\r\n]*has not been migrated") ""
exit (Write-E2EResults $r $OutFile)
