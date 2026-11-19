#Requires -Version 7.0
# Revision snapshots of every work item in a target project, to prove a re-run adds no revision it should not (#3299).
# A snapshot records, per work item, the number of revisions, the number of updates and the latest ChangedDate across all
# revisions, read over REST. Updates are counted too because a change to links only is an update without a new revision.
Import-Module (Join-Path $PSScriptRoot "E2EAzDo.psm1")

function Get-E2ERevisionSnapshot {
    param([Parameter(Mandatory)][string[]]$Project, [string]$Org = "nkdagility-adomt-target")
    $snapshot = @{}
    foreach ($p in $Project) {
        foreach ($wi in Get-E2EAllWorkItems $p $Org) {
            $revisions = @(Get-E2ERevisions $wi.id $Org)
            $updates = @((Invoke-E2ERest "_apis/wit/workitems/$($wi.id)/updates" -Org $Org).value)
            $snapshot[[int]$wi.id] = [pscustomobject]@{
                Id         = [int]$wi.id
                Project    = $p
                Revisions  = $revisions.Count
                Updates    = $updates.Count
                MaxChanged = ($revisions | ForEach-Object { [datetime]$_.fields.'System.ChangedDate' } | Measure-Object -Maximum).Maximum
            }
        }
    }
    $snapshot
}

<#
Compares two snapshots item by item. -Expected maps a target id to the exact number of updates it must gain; every other
item must gain no update and no revision and keep its latest ChangedDate. No item may gain more revisions than updates.
-Either lists ids of which exactly one gains one update: the two target items that hold the same source item, when only the
run decides which of them a link points to.
Returns one row per item and sets Pass on the summary row.
#>
function Compare-E2ERevisionSnapshot {
    param([Parameter(Mandatory)][hashtable]$Before, [Parameter(Mandatory)][hashtable]$After, [hashtable]$Expected = @{}, [int[]]$Either = @())
    $rows = foreach ($id in ($Before.Keys + $After.Keys | Sort-Object -Unique)) {
        $b = $Before[$id]; $a = $After[$id]
        $gained = if ($a -and $b) { $a.Revisions - $b.Revisions } elseif ($a) { $a.Revisions } else { -1 }
        $updatesGained = if ($a -and $b) { $a.Updates - $b.Updates } elseif ($a) { $a.Updates } else { -1 }
        $want = if ($Expected.ContainsKey($id)) { [int]$Expected[$id] } else { 0 }
        $ok = if ($Either -contains $id) { $updatesGained -in 0, 1 } else { $updatesGained -eq $want }
        if ($gained -gt $updatesGained -or $gained -lt 0) { $ok = $false }
        if ($ok -and $updatesGained -eq 0 -and $a -and $b -and $a.MaxChanged -ne $b.MaxChanged) { $ok = $false }
        [pscustomobject]@{
            Id = $id; Project = ($a ?? $b).Project
            RevisionsBefore = $b.Revisions; RevisionsAfter = $a.Revisions; Gained = $gained
            UpdatesBefore = $b.Updates; UpdatesAfter = $a.Updates; UpdatesGained = $updatesGained
            Expected = $(if ($Either -contains $id) { "0 or 1 (one of $($Either -join '/'))" } else { $want })
            ChangedBefore = $b.MaxChanged; ChangedAfter = $a.MaxChanged; Ok = $ok
        }
    }
    if ($Either.Count -gt 0) {
        $eitherGained = @($rows | Where-Object { $Either -contains $_.Id -and $_.UpdatesGained -eq 1 }).Count
        if ($eitherGained -ne 1) { foreach ($r in $rows | Where-Object { $Either -contains $_.Id }) { $r.Ok = $false } }
    }
    $rows
}

function Format-E2ERevisionSummary {
    param([Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Rows, [Parameter(Mandatory)][string]$Label)
    $bad = @($Rows | Where-Object { -not $_.Ok })
    $unchanged = @($Rows | Where-Object { $_.UpdatesGained -eq 0 -and $_.Gained -eq 0 -and $_.ChangedBefore -eq $_.ChangedAfter }).Count
    $gainedRows = @($Rows | Where-Object { $_.UpdatesGained -gt 0 })
    $gainedText = ($gainedRows | ForEach-Object { "$($_.Id) +$($_.UpdatesGained) update(s), +$($_.Gained) revision(s) (expected $($_.Expected))" }) -join ", "
    [pscustomobject]@{
        Scenario = $Label; Items = $Rows.Count
        RevisionsBefore = ($Rows | Measure-Object RevisionsBefore -Sum).Sum; RevisionsAfter = ($Rows | Measure-Object RevisionsAfter -Sum).Sum
        UpdatesBefore = ($Rows | Measure-Object UpdatesBefore -Sum).Sum; UpdatesAfter = ($Rows | Measure-Object UpdatesAfter -Sum).Sum
        Unchanged = $unchanged; Gained = $(if ($gainedText) { $gainedText } else { "none" })
        Unexpected = $(if ($bad) { ($bad | ForEach-Object { "$($_.Id): revisions $($_.RevisionsBefore)->$($_.RevisionsAfter), updates $($_.UpdatesBefore)->$($_.UpdatesAfter), $($_.ChangedBefore.ToString('o'))->$($_.ChangedAfter.ToString('o'))" }) -join "; " } else { "none" })
        Pass = ($bad.Count -eq 0)
    }
}

Export-ModuleMember -Function Get-E2ERevisionSnapshot, Compare-E2ERevisionSnapshot, Format-E2ERevisionSummary
