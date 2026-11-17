# tags: #3264 (TagMappingMap renames tags, then TagFilterMap keeps only approved tags). Tags are set over several revisions,
# so every replayed revision must be renamed and filtered, not just the last one.
param([string]$Src = "amte2e-3264-src", [string]$Tgt = "amte2e-3264-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
New-E2EProject -Name $Src -Process $Process | Out-Null

function Step([int]$Id, [System.Collections.IDictionary]$Fields) {
    Start-Sleep -Seconds 2
    Set-E2EWorkItem -Id $Id -Fields $Fields | Out-Null
}

# T1: an approved tag, an unapproved tag, and a tag renamed to an approved name.
$t1 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "tags T1"; "System.Tags" = "Approved; Random; Bug-Bash" })
# T2: a pattern-approved tag, an excluded tag that also matches the pattern, and a wrong-case approved tag.
$t2 = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "tags T2"; "System.Tags" = "Team-A; Team-Legacy; customer" })
# T3: an unapproved tag in the first revision, an approved tag added later.
$t3 = Set-E2EWorkItem $Src "User Story" -Fields ([ordered]@{ "System.Title" = "tags T3"; "System.Tags" = "tmp-x" })
Step $t3.id ([ordered]@{ "System.Tags" = "tmp-x; Release" })
# T4: a regex rename with a group.
$t4 = Set-E2EWorkItem $Src "Bug" -Fields ([ordered]@{ "System.Title" = "tags T4"; "System.Tags" = "Sprint 12; Other" })

Save-E2EState $PSScriptRoot @{ t1 = $t1.id; t2 = $t2.id; t3 = $t3.id; t4 = $t4.id }
"seeded tags: T1 $($t1.id) T2 $($t2.id) T3 $($t3.id) T4 $($t4.id)"
