#Requires -Version 7.0
# Writes the multi-user configs for one side (main or branch). MatchUsersByEmail is on; the mapping file maps the ghost to the
# real user and the second ghost to a target that does not resolve. Generated config-*.json files are git-ignored.
#   users-continue   StopIfMappingTargetsDoNotResolve false: the run migrates W1-W3 and warns about W3's target   -> exit 0
#   users-stop       StopIfMappingTargetsDoNotResolve true: the run stops before migrating, naming W3's mapping  -> exit 1
param([Parameter(Mandatory)][ValidateSet("main", "branch")][string]$Side, [string]$Src = "amte2e-3272-users-src")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EConfig.psm1") -Force
$map = (Resolve-Path (Join-Path $PSScriptRoot "map-users.json")).Path
foreach ($r in @(@{ Name = "users-continue"; Stop = $false }, @{ Name = "users-stop"; Stop = $true })) {
    New-E2EConfig -Dir $PSScriptRoot -Name "config-$Side-$($r.Name)" -Src $Src -Tgt "amte2e-3272-users-tgt-$Side" `
        -Processors @(New-E2EWorkItemProcessor @{ FilterWorkItemsThatAlreadyExistInTarget = $true }) -Tools @{
        TfsUserMappingTool = [ordered]@{
            Enabled = $true; UserMappingFile = $map; MatchUsersByEmail = $true
            StopIfMappingTargetsDoNotResolve = $r.Stop; ValidateMappingTargets = "All"
        }
    }
}
