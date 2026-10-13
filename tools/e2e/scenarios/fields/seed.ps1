# fields: #3186/#3202 history-only fields, #3151 validator uses the source WIT for field maps, #3209 reflected id before field maps.
#  legacy   - User Story created in amte2e-fields-old (whose process has Custom.amte2eLegacy) and then moved into the source
#             project, whose process does not have the field: its value only exists in revision history (#3173 case).
#  issue    - Issue with Custom.amte2eIssueOnly (only on Issue in the source process). The config maps Issue -> Task and
#             has a FieldtoFieldMap with ApplyTo ["Issue"] for the field, so validation must pass (#3151).
#  reflect  - Task with one revision; a TargetToTarget map copies Custom.ReflectedWorkItemId to Integration Build (#3209).
param()
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EProcess.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$org = "nkdagility-adomt-source"

Set-E2EOrg "nkdagility-adomt-target"
New-E2EProject -Name "amte2e-fields-tgt" -Process "amte2e-agile" | Out-Null

Set-E2EOrg $org
$legacyRef = New-E2EField "amte2eLegacy" -Org $org
$issueRef = New-E2EField "amte2eIssueOnly" -Org $org
$oldProc = New-E2EBaseProcess -Name "amte2e-fields-old" -Org $org
Add-E2EFieldToWit $oldProc "User Story" $legacyRef
$srcProc = New-E2EBaseProcess -Name "amte2e-fields-srcproc" -Org $org
Add-E2EFieldToWit $srcProc "Issue" $issueRef
New-E2EProject -Name "amte2e-fields-old" -Process "amte2e-fields-old" | Out-Null
New-E2EProject -Name "amte2e-fields-src" -Process "amte2e-fields-srcproc" | Out-Null

$legacy = Set-E2EWorkItem "amte2e-fields-old" "User Story" -Fields ([ordered]@{ "System.Title" = "fields legacy (moved)"; $legacyRef = "legacy-value" })
Set-E2EWorkItem -Id $legacy.id -Fields ([ordered]@{ "System.History" = "edited in old project" }) | Out-Null
Set-E2EWorkItem -Id $legacy.id -Fields ([ordered]@{ "System.TeamProject" = "amte2e-fields-src"; "System.AreaPath" = "amte2e-fields-src"; "System.IterationPath" = "amte2e-fields-src" }) | Out-Null
Set-E2EWorkItem -Id $legacy.id -Fields ([ordered]@{ "System.History" = "edited after move" }) | Out-Null

$issue = Set-E2EWorkItem "amte2e-fields-src" "Issue" -Fields ([ordered]@{ "System.Title" = "fields issue"; $issueRef = "issue-value" })
$reflect = Set-E2EWorkItem "amte2e-fields-src" "Task" -Fields ([ordered]@{ "System.Title" = "fields reflect (single revision)" })

Save-E2EState $PSScriptRoot @{ legacy = $legacy.id; issue = $issue.id; reflect = $reflect.id; legacyRef = $legacyRef; issueRef = $issueRef }
"seeded fields: legacy $($legacy.id) ($legacyRef), issue $($issue.id) ($issueRef), reflect $($reflect.id)"
