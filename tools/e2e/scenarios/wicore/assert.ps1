# wi-core assertions: per-item fidelity, link topology, nodes, and no duplicates.
param([string]$Src = "amte2e-wicore-src", [string]$Tgt = "amte2e-wicore-tgt", [string]$OutFile)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAssert.psm1") -Force
$s = Read-E2EState $PSScriptRoot
$r = @()
foreach ($k in "epic", "feature", "story", "bug", "task") { $r += Test-E2EItemFidelity -SourceId $s.$k -SrcProject $Src -TgtProject $Tgt }

$tgtItems = @(Get-E2EAllWorkItems $Tgt -Org nkdagility-adomt-target)
$r += New-E2EResult "target item count (no duplicates)" ($tgtItems.Count -eq 5) "$($tgtItems.Count) items"
$titles = $tgtItems | Group-Object { $_.fields.'System.Title' } | Where-Object Count -gt 1
$r += New-E2EResult "no duplicate titles" (-not $titles) $(if ($titles) { ($titles.Name) -join ", " } else { "unique" })

# Link topology in the target, translated back to source ids through the reflected id.
$map = @{}; foreach ($i in $tgtItems) { if ($i.fields.'Custom.ReflectedWorkItemId' -match '/(\d+)$') { $map[[int]$i.id] = [int]$matches[1] } }
$edges = foreach ($i in $tgtItems) { foreach ($rel in @($i.relations)) { if ($rel.url -match '/workItems/(\d+)$') { "$($map[[int]$i.id])-$($rel.rel)->$($map[[int]$matches[1]])" } } }
$expected = @(
    "$($s.feature)-System.LinkTypes.Hierarchy-Reverse->$($s.epic)", "$($s.epic)-System.LinkTypes.Hierarchy-Forward->$($s.feature)",
    "$($s.story)-System.LinkTypes.Hierarchy-Reverse->$($s.feature)", "$($s.task)-System.LinkTypes.Hierarchy-Reverse->$($s.story)",
    "$($s.story)-System.LinkTypes.Related->$($s.bug)", "$($s.bug)-System.LinkTypes.Related->$($s.story)")
$missing = $expected | Where-Object { $edges -notcontains $_ }
$r += New-E2EResult "link topology (parent/child, related)" (-not $missing) $(if ($missing) { "missing: $($missing -join ', ')" } else { "$(@($edges).Count) link ends, all expected present" })

$story = Find-E2ETargetItem $Tgt $s.story -Org nkdagility-adomt-target
$att = @($story.relations | Where-Object rel -eq AttachedFile)
$r += New-E2EResult "story attachment present" ($att.Count -eq 1 -and $att[0].attributes.name -eq "e2e-note.txt") "$($att.Count) attachment(s): $($att.attributes.name -join ',')"

$nodes = Invoke-E2ERest "$Tgt/_apis/wit/classificationnodes?`$depth=5" -Org nkdagility-adomt-target
$paths = $nodes.value | ForEach-Object { $_; $_.children; $_.children.children } | Where-Object { $_ } | ForEach-Object path
foreach ($p in "\$Tgt\Area\Team A", "\$Tgt\Area\Team A\Sub", "\$Tgt\Iteration\Sprint 1", "\$Tgt\Iteration\Sprint 2") {
    $r += New-E2EResult "node $p" ($paths -contains $p) $(if ($paths -contains $p) { "exists" } else { "missing" })
}
$sp1 = $nodes.value | Where-Object structureType -eq iteration | ForEach-Object children | Where-Object name -eq "Sprint 1"
$r += New-E2EResult "iteration dates copied" ($sp1.attributes.startDate -and ([datetime]$sp1.attributes.startDate).Date -eq [datetime]"2026-09-01") "Sprint 1 start '$($sp1.attributes.startDate)'"

exit (Write-E2EResults $r $OutFile)
