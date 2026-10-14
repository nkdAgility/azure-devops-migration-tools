# queries + teams: #3219 (@CurrentIteration / @TeamAreas team context), shared queries without macros, team settings, capacity.
param([string]$Src = "amte2e-qt-src", [string]$Tgt = "amte2e-qt-tgt", [string]$Process = "amte2e-agile")
$ErrorActionPreference = "Stop"
Import-Module (Join-Path $PSScriptRoot "..\..\E2EAzDo.psm1") -Force
Import-Module (Join-Path $PSScriptRoot "..\..\E2ESeed.psm1") -Force

Set-E2EOrg nkdagility-adomt-target
New-E2EProject -Name $Tgt -Process $Process | Out-Null
Set-E2EOrg nkdagility-adomt-source
$p = New-E2EProject -Name $Src -Process $Process

$today = (Get-Date).Date
New-E2ENode $Src Areas "Squad A"
$s1 = New-E2ENode $Src Iterations "Sprint 1" -Start $today.AddDays(-5) -Finish $today.AddDays(8)
$s2 = New-E2ENode $Src Iterations "Sprint 2" -Start $today.AddDays(9) -Finish $today.AddDays(22)
$iters = (Invoke-E2ERest "$Src/_apis/wit/classificationnodes/Iterations?`$depth=2").children
$s1 = $iters | Where-Object name -eq "Sprint 1"; $s2 = $iters | Where-Object name -eq "Sprint 2"

$team = (Invoke-E2ERest "_apis/projects/$($p.id)/teams").value | Where-Object name -eq "Squad A"
if (-not $team) { Invoke-E2ERest "_apis/projects/$($p.id)/teams" -Method POST -Body @{ name = "Squad A"; description = "amte2e team" } | Out-Null }
# Re-read so the id used in the query WIQL below comes from the stored team.
$team = (Invoke-E2ERest "_apis/projects/$($p.id)/teams").value | Where-Object name -eq "Squad A"
if (-not $team.id) { throw "Team Squad A was not created." }
$defaultTeam = "$Src Team"
# A team created through REST has no backlog iteration, and every other team-settings write fails with TF400497 until it does.
Invoke-E2ERest "$Src/Squad%20A/_apis/work/teamsettings" -Method PATCH -Body @{ backlogIteration = (Invoke-E2ERest "$Src/_apis/wit/classificationnodes/Iterations").identifier } | Out-Null
foreach ($t in "Squad A", $defaultTeam) {
    $te = [Uri]::EscapeDataString($t)
    foreach ($it in $s1, $s2) { try { Invoke-E2ERest "$Src/$te/_apis/work/teamsettings/iterations" -Method POST -Body @{ id = $it.identifier } | Out-Null } catch { if ($_.ErrorDetails.Message -notmatch "already") { throw } } }
}
Invoke-E2ERest "$Src/Squad%20A/_apis/work/teamsettings/teamfieldvalues" -Method PATCH -Body @{ defaultValue = "$Src\Squad A"; values = @(@{ value = "$Src\Squad A"; includeChildren = $true }) } | Out-Null

# Capacity on the default team (its creator is a member).
$me = (Invoke-E2ERest "_apis/connectionData" -ApiVersion "7.1-preview").authenticatedUser
$dt = [Uri]::EscapeDataString($defaultTeam)
Invoke-E2ERest "$Src/$dt/_apis/work/teamsettings/iterations/$($s1.identifier)/capacities/$($me.id)" -Method PATCH -Body @{
    activities = @(@{ name = "Development"; capacityPerDay = 4 })
    daysOff = @(@{ start = $today.AddDays(1).ToString("o"); end = $today.AddDays(1).ToString("o") })
} | Out-Null

Invoke-E2ERest "$Src/_apis/wit/workitems/`$Task" -Method POST -ContentType "application/json-patch+json" -Body (ConvertTo-Json @(@{ op = "add"; path = "/fields/System.Title"; value = "qt task" }, @{ op = "add"; path = "/fields/System.IterationPath"; value = "$Src\Sprint 1" }, @{ op = "add"; path = "/fields/System.AreaPath"; value = "$Src\Squad A" })) | Out-Null

$folder = try { Invoke-E2ERest "$Src/_apis/wit/queries/Shared%20Queries/amte2e" } catch { Invoke-E2ERest "$Src/_apis/wit/queries/Shared%20Queries" -Method POST -Body @{ name = "amte2e"; isFolder = $true } }
$queries = [ordered]@{
    "plain open items"     = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.State] <> 'Closed' ORDER BY [System.Id]"
    "squad next iteration" = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.IterationPath] = @CurrentIteration('[$Src]\Squad A <id:$($team.id)>') + 1"
    "squad areas"          = "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.TeamProject] = @project AND [System.AreaPath] IN (@TeamAreas('[$Src]\Squad A <id:$($team.id)>'))"
}
foreach ($n in $queries.Keys) { try { Invoke-E2ERest "$Src/_apis/wit/queries/Shared%20Queries/amte2e" -Method POST -Body @{ name = $n; wiql = $queries[$n] } | Out-Null } catch { if ($_.ErrorDetails.Message -notmatch "already exists|TF237018") { throw } } }

$stored = (Invoke-E2ERest "$Src/_apis/wit/queries/Shared%20Queries/amte2e?`$depth=1&`$expand=wiql").children | ForEach-Object { "$($_.name): $($_.wiql)" }
Save-E2EState $PSScriptRoot @{ team = $team.id; sprint1 = $s1.identifier; sprint2 = $s2.identifier; me = $me.id }
"seeded queries/teams; stored source queries:"; $stored
