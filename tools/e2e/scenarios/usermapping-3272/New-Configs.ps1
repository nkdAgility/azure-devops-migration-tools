#Requires -Version 7.0
# Writes the usermapping-3272 configs for one side (main or branch). Mapping file paths must be absolute (the harness runs the
# tool from a temporary folder). The generated config-*.json files are git-ignored.
#   good-all                 mapping resolves; ValidateMappingTargets All                            -> exit 0
#   good-skipusers           as good-all with SkipValidateAllUsersExistOrAreMapped                   -> exit 0
#   badunref-all             an unreferenced mapping target does not resolve; All                    -> run stops
#   badunref-all-nostop      as badunref-all with StopIfMappingTargetsDoNotResolve false             -> exit 0, warning
#   badunref-referenced      as badunref-all with Referenced: the bad mapping is not checked         -> exit 0
#   badref-referenced        the referenced mapping target does not resolve; Referenced              -> run stops
#   badunref-skip            as badunref-all with Skip                                               -> exit 0, warning
param([Parameter(Mandatory)][ValidateSet("main", "branch")][string]$Side, [string]$Src = "amte2e-3272-src")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EConfig.psm1") -Force
$file = { param($n) (Resolve-Path (Join-Path $PSScriptRoot $n)).Path }
$runs = @(
    @{ Name = "good-all"; File = "map-good.json"; Scope = "All" }
    @{ Name = "good-skipusers"; File = "map-good.json"; Scope = "All"; SkipUsers = $true }
    @{ Name = "badunref-all"; File = "map-bad-unreferenced.json"; Scope = "All" }
    @{ Name = "badunref-all-nostop"; File = "map-bad-unreferenced.json"; Scope = "All"; NoStop = $true }
    @{ Name = "badunref-referenced"; File = "map-bad-unreferenced.json"; Scope = "Referenced" }
    @{ Name = "badref-referenced"; File = "map-bad-referenced.json"; Scope = "Referenced" }
    @{ Name = "badunref-skip"; File = "map-bad-unreferenced.json"; Scope = "Skip" }
)
foreach ($r in $runs) {
    New-E2EConfig -Dir $PSScriptRoot -Name "config-$Side-$($r.Name)" -Src $Src -Tgt "amte2e-3272-tgt-$Side" `
        -Processors @(New-E2EWorkItemProcessor @{ FilterWorkItemsThatAlreadyExistInTarget = $true }) -Tools @{
        TfsUserMappingTool = [ordered]@{
            Enabled = $true; UserMappingFile = (& $file $r.File); MatchUsersByEmail = $false
            SkipValidateAllUsersExistOrAreMapped = [bool]$r.SkipUsers; StopIfMappingTargetsDoNotResolve = -not $r.NoStop
            ValidateMappingTargets = $r.Scope
        }
    }
}
