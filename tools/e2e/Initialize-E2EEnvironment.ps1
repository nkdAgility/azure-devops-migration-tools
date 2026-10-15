<#
.SYNOPSIS
    Creates the shared inherited process the scenario seeders use (amte2e-agile, with Custom.ReflectedWorkItemId on every Agile
    work item type) in both test organisations. Safe to re-run. Run it before any seed.ps1, including after Remove-E2EObjects.ps1.
#>
param([string[]]$Organisations = @("nkdagility-adomt-source", "nkdagility-adomt-target"), [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "E2EProcess.psm1") -Force
foreach ($org in $Organisations) {
    Set-E2EOrg $org
    $id = New-E2EBaseProcess -Name $Process -Org $org
    "$org : $Process ($id) ready"
}
