# Assertion helpers. Each check returns a result object; Write-E2EResults prints them and returns the failure count.
Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1")
Import-Module (Join-Path $PSScriptRoot "E2ESeed.psm1")

$script:SrcOrg = "nkdagility-adomt-source"
$script:TgtOrg = "nkdagility-adomt-target"

function New-E2EResult([string]$Check, [bool]$Pass, [string]$Evidence) { [pscustomobject]@{ Check = $Check; Pass = $Pass; Evidence = $Evidence } }

function Write-E2EResults([object[]]$Results, [string]$OutFile) {
    $Results | ForEach-Object { "{0} {1} -- {2}" -f $(if ($_.Pass) { "PASS" } else { "FAIL" }), $_.Check, $_.Evidence }
    if ($OutFile) { $Results | ConvertTo-Json -Depth 5 | Set-Content $OutFile }
    @($Results | Where-Object { -not $_.Pass }).Count
}

# Counts of each artifact (attachments, non-attachment links) per revision, keyed by revision index.
function Get-E2ERevisionShape([object[]]$Revs) {
    foreach ($r in $Revs) {
        $rel = @($r.relations)
        [pscustomobject]@{
            Rev = $r.rev; State = $r.fields.'System.State'; ChangedBy = $r.fields.'System.ChangedBy'.uniqueName
            Attachments = @($rel | Where-Object rel -eq "AttachedFile").Count
            Links = @($rel | Where-Object { $_.rel -ne "AttachedFile" }).Count
            Area = $r.fields.'System.AreaPath'; Iteration = $r.fields.'System.IterationPath'
        }
    }
}

# Pairs a source item with its target and compares revisions one for one.
function Test-E2EItemFidelity {
    param([int]$SourceId, [string]$TgtProject, [string]$SrcProject, [switch]$ExpectRevisionMatch)
    $out = @()
    $t = Find-E2ETargetItem $TgtProject $SourceId -Org $script:TgtOrg
    if (-not $t) { return New-E2EResult "src #$SourceId migrated" $false "no target item reflects it" }
    $sRevs = @(Get-E2ERevisions $SourceId -Org $script:SrcOrg)
    $tRevs = @(Get-E2ERevisions $t.id -Org $script:TgtOrg)
    # Date fix-up saves (#3180/#3205) are extra revisions that keep the date of the revision they fix (to the second).
    # Collapse them, then the target's (ChangedDate, ChangedBy, State) sequence must equal the source's.
    $key = { param($r) "{0:yyyy-MM-dd HH:mm:ss}|{1}|{2}" -f ([datetime]$r.fields.'System.ChangedDate'), $r.fields.'System.ChangedBy'.uniqueName, $r.fields.'System.State' }
    # Seeded revisions can share a second too, so both sides are collapsed the same way.
    $sSeq = @(); foreach ($k in @($sRevs | ForEach-Object { & $key $_ })) { if (-not ($sSeq.Count -and $sSeq[-1] -eq $k)) { $sSeq += $k } }
    $tAll = @($tRevs | ForEach-Object { & $key $_ })
    $tSeq = @(); foreach ($k in $tAll) { if (-not ($tSeq.Count -and $tSeq[-1] -eq $k)) { $tSeq += $k } }
    $fixups = $tRevs.Count - $sRevs.Count
    $seqOk = ($sSeq -join "`n") -eq ($tSeq -join "`n")
    $diff = if (-not $seqOk) { (Compare-Object $sSeq $tSeq | ForEach-Object { "$($_.SideIndicator) $($_.InputObject)" }) -join "; " } else { "" }
    $out += New-E2EResult "src #$SourceId -> tgt #$($t.id) revision sequence (date|by|state)" $seqOk $(if ($seqOk) { "$($sSeq.Count) source revisions matched; target $($tRevs.Count) incl. $fixups same-date fix-up save(s)" } else { $diff })
    $last = [datetime]$sRevs[-1].fields.'System.ChangedDate'
    $late = @($tRevs | Where-Object { ([datetime]$_.fields.'System.ChangedDate') -gt $last.AddSeconds(1) })
    $out += New-E2EResult "src #$SourceId no revision dated after source history (#3196)" ($late.Count -eq 0) $(if ($late) { ($late | ForEach-Object { "rev $($_.rev) $($_.fields.'System.ChangedDate') by $($_.fields.'System.ChangedBy'.uniqueName)" }) -join "; " } else { "last target rev <= source last ($last)" })
    $mig = @($tRevs | Where-Object { $_.fields.'System.ChangedBy'.displayName -match '^Migration$' })
    $out += New-E2EResult "src #$SourceId no 'Migration' revisions" ($mig.Count -eq 0) "$($mig.Count)"
    # Relation timing (#3196): a relation's authorizedDate is the wall-clock AuthorizedDate of the revision that added it
    # (the /updates view shows back-dated adds at rev 1). Resolve it to that revision's ChangedDate and compare with the source.
    $sw = Invoke-E2ERest "_apis/wit/workitems/$SourceId`?`$expand=relations" -Org $script:SrcOrg
    $relKey = { param($rel) if ($rel.rel -eq "AttachedFile") { "att:" + $rel.attributes.name } else { "$($rel.rel):$($rel.url)" } }
    $sRel = @($sw.relations | Where-Object { $_.rel -in "AttachedFile", "Hyperlink" } | ForEach-Object { "{0}@{1:yyyy-MM-dd HH:mm:ss}" -f (& $relKey $_), ([datetime]$_.attributes.authorizedDate) } | Sort-Object)
    $authToChanged = @{}; $changed = $null
    foreach ($u in (Invoke-E2ERest "_apis/wit/workitems/$($t.id)/updates" -Org $script:TgtOrg).value) {
        if ($u.fields.'System.ChangedDate') { $changed = [datetime]$u.fields.'System.ChangedDate'.newValue }
        if ($u.fields.'System.AuthorizedDate') { $authToChanged[([datetime]$u.fields.'System.AuthorizedDate'.newValue).ToString("o")] = $changed }
    }
    $tRel = @($t.relations | Where-Object { $_.rel -in "AttachedFile", "Hyperlink" } | ForEach-Object {
        $a = ([datetime]$_.attributes.authorizedDate).ToString("o")
        $d = if ($authToChanged.ContainsKey($a)) { "{0:yyyy-MM-dd HH:mm:ss}" -f $authToChanged[$a] } else { "unmatched($a)" }
        "{0}@{1}" -f ((& $relKey $_) -replace '\[originalId:\d+\]', ''), $d } | Sort-Object)
    if ($sRel.Count -or $tRel.Count) {
        $ok = ($sRel -join ",") -eq ($tRel -join ",")
        $out += New-E2EResult "src #$SourceId attachment/hyperlink authorizedDate matches source revision (#3196)" $ok "source [$($sRel -join ', ')] target [$($tRel -join ', ')]"
    }
    $foreign = @($t.relations | Where-Object { $_.url -match '/_apis/wit/workItems/' -and $_.url -notmatch "dev.azure.com/$($script:TgtOrg)/" })
    $out += New-E2EResult "src #$SourceId work item links point at target org" ($foreign.Count -eq 0) "$($foreign.Count) foreign"
    $sl = $sRevs[-1].fields; $tl = $tRevs[-1].fields
    foreach ($f in "System.State", "System.Title", "Microsoft.VSTS.Common.ClosedDate", "Microsoft.VSTS.Common.ResolvedDate", "Microsoft.VSTS.Common.ActivatedDate", "System.CreatedDate") {
        $sv = "$($sl.$f)"; $tv = "$($tl.$f)"
        if ($sv -or $tv) { $out += New-E2EResult "src #$SourceId $f" ($sv -eq $tv) "source '$sv', target '$tv'" }
    }
    foreach ($f in "System.AssignedTo", "System.CreatedBy", "Microsoft.VSTS.Common.ResolvedBy", "Microsoft.VSTS.Common.ClosedBy", "Microsoft.VSTS.Common.ActivatedBy") {
        $sv = "$($sl.$f.uniqueName)"; $tv = "$($tl.$f.uniqueName)"
        if ($sv -or $tv) { $out += New-E2EResult "src #$SourceId $f" ($sv -eq $tv) "source '$sv', target '$tv'" }
    }
    $sa = ($sl.'System.AreaPath' -replace "^$SrcProject", ""); $ta = ($tl.'System.AreaPath' -replace "^$TgtProject", "")
    $si = ($sl.'System.IterationPath' -replace "^$SrcProject", ""); $ti = ($tl.'System.IterationPath' -replace "^$TgtProject", "")
    $out += New-E2EResult "src #$SourceId area/iteration" ($sa -eq $ta -and $si -eq $ti) "area '$sa'/'$ta', iteration '$si'/'$ti'"
    $out
}

Export-ModuleMember -Function *-E2E*
