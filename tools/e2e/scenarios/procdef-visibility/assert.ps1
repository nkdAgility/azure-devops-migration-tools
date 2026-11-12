# procdef-visibility assertions (#3242): the target process layout hides what the source hides.
param([string]$Process = "amt3242-vis", [string]$OutFile)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
$org = "nkdagility-adomt-target"
Set-E2EOrg $org
$procId = Get-E2EProcessId $Process $org
if (-not $procId) { throw "Process $Process does not exist in $org." }
$wits = (Invoke-E2ERest "_apis/work/processes/$procId/workitemtypes" -Org $org).value | Where-Object customization -ne "system"

function Get-Layout([string]$WitName) {
    $w = ($wits | Where-Object name -eq $WitName).referenceName
    Invoke-E2ERest "_apis/work/processes/$procId/workitemtypes/$w/layout" -Org $org
}
function Get-Control($Layout, [string]$Id) {
    foreach ($p in $Layout.pages) { foreach ($s in $p.sections) { foreach ($g in $s.groups) { foreach ($c in $g.controls) { if ($c.id -eq $Id) { return [pscustomobject]@{ Group = $g.label; Visible = $c.visible } } } } } }
}
function Get-Group($Layout, [string]$Label) {
    foreach ($p in $Layout.pages) { foreach ($s in $p.sections) { foreach ($g in $s.groups) { if ($g.label -eq $Label) { return $g } } } }
}

$r = @()
$story = Get-Layout "User Story"
foreach ($e in @(
        @{ Id = "Microsoft.VSTS.Common.Risk"; Visible = $false; Kind = "inherited control" },
        @{ Id = "Custom.amte2e3242Hidden"; Visible = $false; Kind = "custom control on inherited group" },
        @{ Id = "Custom.amte2e3242Secret"; Visible = $false; Kind = "custom control on custom group" },
        @{ Id = "Custom.amte2e3242Shown"; Visible = $true; Kind = "custom control on custom group" },
        @{ Id = "Microsoft.VSTS.Common.Priority"; Visible = $true; Kind = "inherited control" })) {
    $c = Get-Control $story $e.Id
    $r += New-E2EResult "User Story $($e.Kind) $($e.Id) visible=$($e.Visible)" ($c -and $c.Visible -eq $e.Visible) $(if ($c) { "in [$($c.Group)] visible=$($c.Visible)" } else { "missing" })
}
$g = Get-Group $story "amt3242 Custom"
$r += New-E2EResult "User Story custom group amt3242 Custom hidden" ($g -and -not $g.visible) $(if ($g) { "visible=$($g.visible)" } else { "missing" })
$sys = $story.systemControls | Where-Object id -eq "System.Reason"
$r += New-E2EResult "User Story system control System.Reason hidden" ($sys -and -not $sys.visible) $(if ($sys) { "visible=$($sys.visible)" } else { "missing" })
$sysArea = $story.systemControls | Where-Object id -eq "System.AreaPath"
$r += New-E2EResult "User Story system control System.AreaPath still visible" ($sysArea -and $sysArea.visible) "visible=$($sysArea.visible)"

$task = Get-Layout "Task"
$c = Get-Control $task "Microsoft.VSTS.Scheduling.RemainingWork"
$r += New-E2EResult "Task inherited control RemainingWork hidden" ($c -and -not $c.Visible) $(if ($c) { "visible=$($c.Visible)" } else { "missing" })

Write-E2EResults $r $OutFile
if ($r | Where-Object { -not $_.Pass }) { exit 1 }
