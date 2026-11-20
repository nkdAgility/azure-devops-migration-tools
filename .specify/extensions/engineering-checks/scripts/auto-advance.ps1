# Runs after each tool use. When a worker has just written its item's evidence (.specify/wkr/ or .specify/reviews/),
# it checks the item against the exit policy for its state and, if the policy passes, moves it on through
# move-state's own check. It never answers for Martin: only an item in a working state moves (wait states are pulled
# by the manager within the WIP limits, or wait for Martin), and a policy that runs a slow check (a build, the tests,
# the outcome checks) is left to the workflow step that runs it. It says nothing unless the item moved.
# It never blocks: any error is reported and the tool call stands.
$ErrorActionPreference = 'Stop'
try {
    $call = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $in = $call.tool_input
    $paths = @()
    if ($call.tool_name -match '^(Write|Edit|MultiEdit|NotebookEdit)$') { $paths = @("$($in.file_path)$($in.notebook_path)") }
    elseif ($call.tool_name -match '^(Bash|PowerShell)$') { $paths = @("$($in.command)") }
    if (-not ($paths | Where-Object { $_ -match '\.specify[\\/](wkr|reviews)[\\/]' })) { exit 0 }

    $dir = if ($in.file_path) { Split-Path ([string]$in.file_path) -Parent } else { "$($call.cwd)" }
    while ($dir -and -not (Test-Path $dir)) { $dir = Split-Path $dir -Parent }
    $root = git -C $dir rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $root) { exit 0 }
    $cur = Join-Path $root '.specify/items/current.json'
    if (-not (Test-Path $cur)) { exit 0 }
    $id = (Get-Content $cur -Raw | ConvertFrom-Json).number

    Import-Module (Join-Path $root '.specify/extensions/engineering-checks/workflow/Workflow.psm1') -Force
    $cfg = Get-WorkflowConfig -Root $root
    $ad = Get-BoardAdapter $cfg $root
    $item = & $ad.GetItem $id
    if (-not $item -or $item.tags -contains $cfg.tags.blocked) { exit 0 }
    if ((Get-StateDef $cfg $item.state).kind -ne 'work') { exit 0 }
    $order = Get-ItemOrder $cfg $item
    $i = $order.IndexOf($item.state)
    if ($i -lt 0 -or $i -ge $order.Count - 1) { exit 0 }
    $to = $order[$i + 1]

    $t = Test-Move -Config $cfg -Item $item -To $to -Adapter $ad -Root $root -AllItems @(& $ad.GetItems) -SkipScripts
    if (-not $t.ok -or @($t.skipped).Count) { exit 0 }
    $r = Move-WorkItem -Config $cfg -Id $id -To $to -Reason "its $($t.shift.name) policy passed" -Adapter $ad -Root $root
    if ($r.ok -and $r.moved) { "auto-advance: #$id moved from $($r.from) to $($r.to) ($($t.shift.name) policy passed). Rename your chat for the new state and do its work." }
} catch {
    [Console]::Error.WriteLine("auto-advance: could not check the item's policy ($_); nothing moved.")
}
exit 0
