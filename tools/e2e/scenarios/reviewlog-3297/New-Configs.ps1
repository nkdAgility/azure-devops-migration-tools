# Writes the reviewlog (#3297) config. The mapping file path must be absolute because the harness runs the tool from a
# temporary folder. The generated config.json is git-ignored.
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EConfig.psm1") -Force
New-E2EConfig -Dir $PSScriptRoot -Src "amte2e-3297-src" -Tgt "amte2e-3297-tgt" -Tools @{
    TfsAttachmentTool = @{ Enabled = $true; ExportBasePath = "c:\temp\amte2e-attachments"; MaxAttachmentSize = 1000 }
    TfsUserMappingTool = [ordered]@{ Enabled = $true; UserMappingFile = (Resolve-Path (Join-Path $PSScriptRoot "usermapping-ambiguous.json")).Path; IdentityFieldsToCheck = @("System.AssignedTo"); MatchUsersByEmail = $false; StopIfMappingTargetsDoNotResolve = $false }
    TfsGitRepositoryTool = @{ Enabled = $true; Mappings = @{} }
    TfsWorkItemTypeValidatorTool = @{ Enabled = $true; ExcludeWorkItemTypes = @("Bug") }
    StringManipulatorTool = @{ Enabled = $true; MaxStringLength = 150; Manipulators = @() }
    FieldMappingTool = @{ Enabled = $true; FieldMaps = @(@{ FieldMapType = "RegexFieldMap"; ApplyTo = @("User Story"); sourceField = "System.Title"; targetField = "System.Description"; pattern = "(["; replacement = "" }) }
}
$path = Join-Path $PSScriptRoot "config.json"
$cfg = Get-Content $path -Raw | ConvertFrom-Json
foreach ($n in "Areas", "Iterations") {
    $cfg.MigrationTools.CommonTools.TfsNodeStructureTool.$n.Mappings += [pscustomobject]@{ Match = "^amte2e-3297-old([\\]?.*)$"; Replacement = "amte2e-3297-tgt`$1" }
}
$cfg | ConvertTo-Json -Depth 30 | Set-Content $path
