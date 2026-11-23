#Requires -Version 7.0
# stringmanip-3258 assertions. Without -User, checks the run with config.json (the built-in default); with -User, the
# run with config-user.json (a no-op manipulator, which replaces the default). The caller checks that the run exited 0.
param([string]$OutFile, [switch]$User)
$ErrorActionPreference = "Stop"
$Config = if ($User) { "config-user.json" } else { "config.json" }
$Tgt = (Get-Content (Join-Path $PSScriptRoot $Config) -Raw | ConvertFrom-Json).MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()
$l = Find-E2ETargetItem -Project $Tgt -SourceId $s.letters
$r += New-E2EResult "letters Bug migrated" ($null -ne $l) "target $($l.id)"
if ($l) {
    $r += New-E2EResult "Title kept every letter, emoji and U+FE0F" ($l.fields.'System.Title' -ceq "in-scope 3258 $($s.text)") "got '$($l.fields.'System.Title')'"
    $r += New-E2EResult "Found In kept every letter, emoji and U+FE0F" ($l.fields.'Microsoft.VSTS.Build.FoundIn' -ceq $s.text) "got '$($l.fields.'Microsoft.VSTS.Build.FoundIn')'"
}
if (-not $User) {
    $n = Find-E2ETargetItem -Project $Tgt -SourceId $s.nonchar
    $r += New-E2EResult "nonchar Bug migrated" ($null -ne $n) "target $($n.id)"
    if ($n) { $r += New-E2EResult "U+FFFE removed from Found In" ($n.fields.'Microsoft.VSTS.Build.FoundIn' -ceq "XY") "got '$($n.fields.'Microsoft.VSTS.Build.FoundIn')'" }
}
exit (Write-E2EResults $r $OutFile)
