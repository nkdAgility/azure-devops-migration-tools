# Writes the users scenario configs for this checkout. TfsUserMappingTool opens UserMappingFile as given, and the harness runs the
# tool from a temporary folder, so the mapping file paths must be absolute paths into the current checkout. The generated
# config*.json files are git-ignored.
#   config.json                     dictionary file, IdentityFieldsToCheck = AssignedTo + decoy prefix      -> amte2e-users-tgt
#   config-identitymapdata.json     IdentityMapData file, same fields                                        -> amte2e-users2-tgt
#   config-control-emptymap.json    empty mapping file (ResolvedBy change must survive)                      -> amte2e-users3-tgt
#   config-control-assignedonly.json  IdentityFieldsToCheck = AssignedTo only                                -> amte2e-users4-tgt
#   config-control-padded6.json     AssignedTo x6, overriding every shipped default index                    -> amte2e-users5-tgt
param([string]$Src = "amte2e-users-src")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EConfig.psm1") -Force
$file = { param($n) (Resolve-Path (Join-Path $PSScriptRoot $n)).Path }
$decoy = @("System.AssignedTo", "Microsoft.VSTS.Common.Resolved")
$runs = @(
    @{ Name = "config"; Tgt = "amte2e-users-tgt"; File = "usermapping-dictionary.json"; Fields = $decoy; Imd = $false }
    @{ Name = "config-identitymapdata"; Tgt = "amte2e-users2-tgt"; File = "usermapping-identitymapdata.json"; Fields = $decoy; Imd = $true }
    @{ Name = "config-control-emptymap"; Tgt = "amte2e-users3-tgt"; File = "usermapping-empty.json"; Fields = $decoy; Imd = $false }
    @{ Name = "config-control-assignedonly"; Tgt = "amte2e-users4-tgt"; File = "usermapping-dictionary.json"; Fields = @("System.AssignedTo"); Imd = $false }
    @{ Name = "config-control-padded6"; Tgt = "amte2e-users5-tgt"; File = "usermapping-dictionary.json"; Fields = @("System.AssignedTo") * 6; Imd = $false }
)
foreach ($r in $runs) {
    New-E2EConfig -Dir $PSScriptRoot -Name $r.Name -Src $Src -Tgt $r.Tgt -Tools @{
        TfsUserMappingTool = [ordered]@{ Enabled = $true; UserMappingFile = (& $file $r.File); IdentityFieldsToCheck = $r.Fields; UseIdentityMapDataFormat = $r.Imd; MatchUsersByEmail = $false }
        TfsWorkItemEmbededLinkTool = @{ Enabled = $true }
    }
}
