#Requires -Version 7.0
# linkscope assertions: A and B are migrated, A keeps its Related link to B, and the links to X (unreadable, TF26198) and O
# (outside the query) are skipped. The run log must report both skips; the caller checks it and the exit code.
param([string]$Src = "amte2e-3271-lnk-src", [string]$Tgt = "amte2e-3271-lnk-tgt", [string]$OutFile, [string]$LogDirectory)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()

$a = Find-E2ETargetItem -Project $Tgt -SourceId $s.a
$b = Find-E2ETargetItem -Project $Tgt -SourceId $s.b
$r += New-E2EResult "A and B migrated" ($a -and $b) "A $($a.id) B $($b.id)"
$tgtItems = @(Get-E2EAllWorkItems $Tgt -Org nkdagility-adomt-target)
$r += New-E2EResult "only A and B migrated" ($tgtItems.Count -eq 2) "$($tgtItems.Count) target items"
if ($a -and $b) {
    $related = @($a.relations | Where-Object rel -eq "System.LinkTypes.Related" | ForEach-Object { [int]($_.url -split '/')[-1] })
    $r += New-E2EResult "A links to B" ($related -contains [int]$b.id) "A related to: $($related -join ', ')"
    $r += New-E2EResult "A has no other Related link" ($related.Count -eq 1) "$($related.Count) Related link(s)"
}
if ($LogDirectory) {
    $log = Get-ChildItem $LogDirectory -Recurse -Filter "migration2*.log" | Sort-Object LastWriteTime | Select-Object -Last 1
    $text = if ($log) { Get-Content $log.FullName -Raw } else { "" }
    $r += New-E2EResult "no NotImplementedException" ($text -notmatch "NotImplementedException") "log $($log.Name)"
    $r += New-E2EResult "unreadable target X reported and skipped" ($text -match "\[SKIP\][^\r\n]*to $($s.x) not migrated") ""
    $r += New-E2EResult "out-of-query target O reported and skipped" ($text -match "\[SKIP\][^\r\n]*wiSourceR=$($s.o)[^\r\n]*has not been migrated") ""
}
exit (Write-E2EResults $r $OutFile)
