# Adoption step 1: sets the workflow up on the repo's board. Safe to run again.
# Writes .specify/memory/workflow.json from the default if the repo has none, checks the board has a Status option for every state (state labels are never used), creates the tags,
# and reports any missing work item types (creating types needs an org admin, so it is reported, not done).
param([string]$Repo)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$cfgPath = '.specify/memory/workflow.json'
if (-not (Test-Path $cfgPath)) {
    New-Item -ItemType Directory -Force (Split-Path $cfgPath) | Out-Null
    $c = Get-Content (Join-Path $PSScriptRoot '..\workflow\workflow.default.json') -Raw | ConvertFrom-Json -AsHashtable
    if ($Repo) { $c.board.repo = $Repo }
    $c | ConvertTo-Json -Depth 10 | Set-Content $cfgPath -Encoding utf8
    "wrote $cfgPath (check the order, Package and WIP limits)"
}
$cfg = Get-WorkflowConfig
$nwo = if ($cfg.board['repo']) { $cfg.board.repo } else { (gh repo view --json nameWithOwner | ConvertFrom-Json).nameWithOwner }
$r = if ($cfg.board['repo']) { @('-R', $cfg.board.repo) } else { @() }
$ad = Get-BoardAdapter $cfg
$statuses = @(& $ad.GetStatuses)
$needStatus = if ($cfg.board['stateField']) { @($cfg.board.categoryStatus.Values) } else { @($cfg.states | ForEach-Object { & $ad.ToStatus $_.name }) }
$missingStatus = @($needStatus | Where-Object { $_ -notin $statuses } | Select-Object -Unique)
if ($cfg.board['stateField']) {
    $opts = @(gh api graphql -f query="{repository(owner:`"$($nwo.Split('/')[0])`",name:`"$($nwo.Split('/')[1])`"){issueFields(first:30){nodes{... on IssueFieldSingleSelect{name options{name}}}}}}" --jq ".data.repository.issueFields.nodes[] | select(.name==`"$($cfg.board.stateField)`") | .options[].name")
    $missingOptions = @($cfg.states.name | Where-Object { $_ -notin $opts })
    if (-not $opts) { "the issue field '$($cfg.board.stateField)' is missing or is not single-select; add it in Organization settings > Planning > Issue fields"; exit 1 }
    if ($missingOptions) { "'$($cfg.board.stateField)' is missing options: $($missingOptions -join ', ')"; exit 1 }
}
foreach ($t in @($cfg.tags.expedite, $cfg.tags.blocked, 'class-c-approved', 'evidence-waived')) { gh label create $t @r --force | Out-Null }
"tags set; Status options missing: $(if ($missingStatus) { $missingStatus -join ', ' } else { 'none' })"
$nwo = if ($cfg.board['repo']) { $cfg.board.repo } else { (gh repo view --json nameWithOwner | ConvertFrom-Json).nameWithOwner }
$owner, $name = $nwo -split '/'
$have = @(gh api graphql -f query="{repository(owner:`"$owner`",name:`"$name`"){issueTypes(first:50){nodes{name}}}}" --jq '.data.repository.issueTypes.nodes[].name' 2>$null)
$want = @('Product Backlog Item', 'Bug', 'Feedback') + $(if ($cfg.usesPackage) { 'Package' } else { @() })
$missing = @($want | Where-Object { $_ -notin $have })
New-Item -ItemType Directory -Force .specify/adopt | Out-Null
@{ missingTypes = $missing; otherTypes = @($have | Where-Object { $_ -notin $want }); missingStatuses = $missingStatus } | ConvertTo-Json | Set-Content .specify/adopt/board.json -Encoding utf8
if ($missing) { "missing work item types (an org admin must add them): $($missing -join ', ')" }
if ($missingStatus -or $missing) { exit 1 }
exit 0