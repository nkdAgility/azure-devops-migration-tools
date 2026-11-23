#Requires -Version 7.0
# stringmanip-3258 assertions. -Config names the scenario config the run used; the caller checks the exit code
# (0 for config.json, 1 for config-user.json).
param([string]$OutFile, [string]$Config = "config.json")
$ErrorActionPreference = "Stop"
$Tgt = (Get-Content (Join-Path $PSScriptRoot $Config) -Raw | ConvertFrom-Json).MigrationTools.Endpoints.Target.Project
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()
$l = Find-E2ETargetItem -Project $Tgt -SourceId $s.letters
$n = Find-E2ETargetItem -Project $Tgt -SourceId $s.nonchar
$r += New-E2EResult "letters Bug migrated" ($null -ne $l) "target $($l.id)"
if ($Config -eq "config.json") {
    if ($l) {
        $r += New-E2EResult "Title kept every letter and emoji" ($l.fields.'System.Title' -ceq "in-scope 3258 $($s.text)") "got '$($l.fields.'System.Title')'"
        $r += New-E2EResult "Found In kept every letter and emoji" ($l.fields.'Microsoft.VSTS.Build.FoundIn' -ceq $s.text) "got '$($l.fields.'Microsoft.VSTS.Build.FoundIn')'"
    }
    $r += New-E2EResult "nonchar Bug migrated" ($null -ne $n) "target $($n.id)"
    if ($n) { $r += New-E2EResult "U+FFFE removed from Found In" ($n.fields.'Microsoft.VSTS.Build.FoundIn' -ceq "AB") "got '$($n.fields.'Microsoft.VSTS.Build.FoundIn')'" }
} else {
    if ($l) {
        $r += New-E2EResult "user manipulator ran on Title" ($l.fields.'System.Title' -ceq ("in-scope 3258 " + ($s.text -creplace 'W', ''))) "got '$($l.fields.'System.Title')'"
        $r += New-E2EResult "user manipulator ran on Found In" ($l.fields.'Microsoft.VSTS.Build.FoundIn' -ceq ($s.text -creplace 'W', '')) "got '$($l.fields.'Microsoft.VSTS.Build.FoundIn')'"
    }
    $r += New-E2EResult "nonchar Bug not migrated (the default was replaced)" ($null -eq $n) "target $($n.id)"
}
exit (Write-E2EResults $r $OutFile)
