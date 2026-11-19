#Requires -Version 7.0
# longrun-3305: builds targets the way an affected run left them (#3305), to prove a re-run corrects them with no delete.
# For each of the first $Count source User Stories, a target work item is created over REST with bypass rules, one
# revision per source revision, each dated and attributed as the source revision with its State and state-stamped values,
# except Microsoft.VSTS.Common.StateChangeDate, which is saved empty. The reflected work item id points at the source, so
# the migration finds it as already migrated.
param([string]$Src = "amt3305-long-src", [string]$Tgt = "amt3305-fix-tgt", [string]$Process = "amte2e-agile", [int]$Count = 10)
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
$copied = "System.State", "System.Reason", "Microsoft.VSTS.Common.ActivatedDate", "Microsoft.VSTS.Common.ResolvedDate", "Microsoft.VSTS.Common.ClosedDate",
    "Microsoft.VSTS.Common.ActivatedBy", "Microsoft.VSTS.Common.ResolvedBy", "Microsoft.VSTS.Common.ClosedBy"
function V($v) { if ($null -eq $v) { "" } elseif ($v.PSObject.Properties["uniqueName"]) { "$($v.displayName) <$($v.uniqueName)>" } elseif ($v -is [datetime]) { $v.ToUniversalTime().ToString("o") } else { "$v" } }

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
$stories = @(Get-E2EAllWorkItems $Src -Org nkdagility-adomt-source | Where-Object { $_.fields.'System.WorkItemType' -eq "User Story" } | Sort-Object { [int]$_.id } | Select-Object -First $Count)
$made = @()
foreach ($s in $stories) {
    $revs = @(Get-E2ERevisions $s.id -Org nkdagility-adomt-source)
    $id = $null
    foreach ($r in $revs) {
        $f = [ordered]@{ "System.ChangedDate" = (V $r.fields.'System.ChangedDate'); "System.ChangedBy" = (V $r.fields.'System.ChangedBy') }
        if (-not $id) {
            $f["System.Title"] = $r.fields.'System.Title'
            $f["System.CreatedDate"] = (V $r.fields.'System.CreatedDate')
            $f["System.CreatedBy"] = (V $r.fields.'System.CreatedBy')
            $f["Custom.ReflectedWorkItemId"] = "https://dev.azure.com/nkdagility-adomt-source/$Src/_workitems/edit/$($s.id)"
        }
        foreach ($k in $copied) { $f[$k] = V $r.fields.$k }
        # The fault: saved empty although the source revision has a value.
        $f["Microsoft.VSTS.Common.StateChangeDate"] = ""
        if ($id) { Set-E2EWorkItem -Id $id -Fields $f -BypassRules -Org nkdagility-adomt-target | Out-Null }
        else { $id = (Set-E2EWorkItem $Tgt "User Story" -Fields $f -BypassRules -Org nkdagility-adomt-target).id }
    }
    $made += [pscustomobject]@{ source = $s.id; target = $id; revisions = $revs.Count }
}
$stateDir = Join-Path $PSScriptRoot $Tgt
New-Item -ItemType Directory -Force $stateDir | Out-Null
Save-E2EState $stateDir @{ items = $made }
"built $($made.Count) targets in $Tgt with an empty State Change Date on every revision: " + (($made | ForEach-Object { "$($_.source)->$($_.target)" }) -join ", ")
