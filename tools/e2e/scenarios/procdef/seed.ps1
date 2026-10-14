# procdef: ProcessDefinitionProcessor with an inherited process whose User Story is customised (so its "Stories" behaviour is
# inherited), carrying a field with '#' in its name (#3208, #2611). Reproduces #3224/#3086 and #3211 on the code they were found on.
#   source org: field Custom.amte2eTicket#, process amte2e-procdef (Agile-derived, field on User Story), project amte2e-procdef-src
#   target org: project amte2e-procdef-tgt on amte2e-agile; the amte2e-procdef process must NOT exist there before the run.
param([string]$Src = "amte2e-procdef-src", [string]$Tgt = "amte2e-procdef-tgt", [string]$Process = "amte2e-procdef")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EProcess.psm1") -Force
$srcOrg = "nkdagility-adomt-source"; $tgtOrg = "nkdagility-adomt-target"

Set-E2EOrg $tgtOrg
New-E2EProject -Name $Tgt -Process "amte2e-agile" -Org $tgtOrg | Out-Null
if (Get-E2EProcessId $Process $tgtOrg) { Write-Warning "Process $Process already exists in $tgtOrg; the run will take the update path instead of create. Run Remove-E2EObjects.ps1 for a clean start." }

Set-E2EOrg $srcOrg
$ref = New-E2EField "amte2eTicket#" -Org $srcOrg
$procId = New-E2EBaseProcess -Name $Process -Wits @("User Story", "Bug") -Org $srcOrg
Add-E2EFieldToWit $procId "User Story" $ref
New-E2EProject -Name $Src -Process $Process -Org $srcOrg | Out-Null

$behaviours = (Invoke-E2ERest "_apis/work/processes/$procId/behaviors" -Org $srcOrg).value | ForEach-Object { "$($_.name)[$($_.customization)]" }
"seeded procdef: field $ref on User Story in $Process; behaviours: $($behaviours -join ', ')"
