# reviewlog (#3297): one work item per review category, plus filler items so a run can be killed part-way.
#  bug      - Bug; Bug is disabled in the target process, so it fails (failed). Re-enable Bug to get a resolved entry.
#  related  - Task with a Related link to the bug, which is never migrated (link-skipped).
#  attach   - Task with a 4 KB attachment; the config sets MaxAttachmentSize below that (attachment-not-migrated).
#  commit   - Task with a commit link to a repository the target does not have (link-left-on-source).
#  ghost    - Task assigned to "Ghost Reviewer", which matches two entries in the mapping file (user-not-mapped). The orgs have one
#             real user, so an identity missing from the target cannot be seeded; the ambiguous mapping is the case that can.
#  legacy   - User Story moved from a project whose process has Custom.amte2e3297Legacy (field-not-mapped).
#  long     - Task with a 200-character Integration Build; the config sets MaxStringLength to 150 (field-truncated). Only
#             String fields go through StringManipulatorTool, and the title is copied onto the shell before it, so it is not used.
#  (legacy also gets field-map-failed: the config has a RegexFieldMap with an invalid pattern for User Story.)
#  rerun-mismatch: after the first run, set State to Active on the target copy of "related" with bypassRules, then run
#             again; the re-run check reports that the target revision does not match the source.
#  fillers  - 12 Tasks, each with an oversized attachment and 4 revisions, so the kill test has entries to cut through.
param([int]$Fillers = 12)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EProcess.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
$src = "amte2e-3297-src"; $tgt = "amte2e-3297-tgt"; $old = "amte2e-3297-old"
$ghost = "Ghost Reviewer"

Set-E2EOrg nkdagility-adomt-target
$tp = New-E2EBaseProcess -Name "amte2e-3297-tgtproc" -Org nkdagility-adomt-target
Set-E2EWitDisabled $tp "Bug" $true
New-E2EProject -Name $tgt -Process "amte2e-3297-tgtproc" | Out-Null

Set-E2EOrg nkdagility-adomt-source
$legacyRef = New-E2EField "amte2e3297Legacy" -Org nkdagility-adomt-source
$op = New-E2EBaseProcess -Name "amte2e-3297-oldproc" -Org nkdagility-adomt-source
Add-E2EFieldToWit $op "User Story" $legacyRef
New-E2EBaseProcess -Name "amte2e-3297-srcproc" -Org nkdagility-adomt-source | Out-Null
New-E2EProject -Name $old -Process "amte2e-3297-oldproc" | Out-Null
New-E2EProject -Name $src -Process "amte2e-3297-srcproc" | Out-Null

$bug = Set-E2EWorkItem $src "Bug" -Fields ([ordered]@{ "System.Title" = "3297 bug (type disabled in target)" })
$related = Set-E2EWorkItem $src "Task" -Fields ([ordered]@{ "System.Title" = "3297 related to the bug" })
Set-E2EWorkItem -Id $related.id -Ops @(Add-E2ELinkOp "System.LinkTypes.Related" $bug.id "to bug") | Out-Null

$bytes = [byte[]]::new(4096); (New-Object Random 3297).NextBytes($bytes)
$attach = Set-E2EWorkItem $src "Task" -Fields ([ordered]@{ "System.Title" = "3297 oversized attachment" })
$att = New-E2EAttachment "big-3297.bin" $bytes
Set-E2EWorkItem -Id $attach.id -Ops @(Add-E2EAttachmentOp $att.url "too big") | Out-Null

$projId = (Invoke-E2ERest "_apis/projects/$src").id
$fakeRepo = "00000000-0000-0000-0000-000000003297"
$commit = Set-E2EWorkItem $src "Task" -Fields ([ordered]@{ "System.Title" = "3297 commit link to unknown repo" })
Set-E2EWorkItem -Id $commit.id -Ops @(@{ op = "add"; path = "/relations/-"; value = @{ rel = "ArtifactLink"; url = "vstfs:///Git/Commit/$projId%2F$fakeRepo%2F3297329732973297329732973297329732973297"; attributes = @{ name = "Fixed in Commit" } } }) | Out-Null

$ghostItem = Set-E2EWorkItem $src "Task" -Fields ([ordered]@{ "System.Title" = "3297 assigned to a ghost" })
Set-E2EWorkItem -Id $ghostItem.id -BypassRules -Fields ([ordered]@{ "System.AssignedTo" = $ghost }) | Out-Null

$legacy = Set-E2EWorkItem $old "User Story" -Fields ([ordered]@{ "System.Title" = "3297 legacy (moved)"; $legacyRef = "legacy-value" })
Set-E2EWorkItem -Id $legacy.id -Fields ([ordered]@{ "System.TeamProject" = $src; "System.AreaPath" = $src; "System.IterationPath" = $src }) | Out-Null

$long = Set-E2EWorkItem $src "Task" -Fields ([ordered]@{ "System.Title" = "3297 long value" })
Set-E2EWorkItem -Id $long.id -Fields ([ordered]@{ "Microsoft.VSTS.Build.IntegrationBuild" = "build-" + ("x" * 194) }) | Out-Null

$fill = @()
for ($i = 1; $i -le $Fillers; $i++) {
    $f = Set-E2EWorkItem $src "Task" -Fields ([ordered]@{ "System.Title" = "3297 filler $i" })
    $a = New-E2EAttachment "filler-$i.bin" $bytes
    Set-E2EWorkItem -Id $f.id -Ops @(Add-E2EAttachmentOp $a.url "filler") | Out-Null
    foreach ($r in 1..3) { Set-E2EWorkItem -Id $f.id -Fields ([ordered]@{ "System.History" = "filler $i edit $r" }) | Out-Null }
    $fill += $f.id
}

Save-E2EState $PSScriptRoot @{ bug = $bug.id; related = $related.id; attach = $attach.id; commit = $commit.id; ghost = $ghostItem.id; legacy = $legacy.id; long = $long.id; fillers = $fill; legacyRef = $legacyRef; targetProcess = $tp }
"seeded 3297: bug $($bug.id) related $($related.id) attach $($attach.id) commit $($commit.id) ghost $($ghostItem.id) legacy $($legacy.id) long $($long.id) fillers $($fill -join ',')"
