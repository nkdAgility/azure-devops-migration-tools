# Runs after each tool use. When a worker has just written its item's evidence (its folder under specs/, the older
# .specify/wkr/, or .specify/reviews/),
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
    if (-not ($paths | Where-Object { $_ -match '\.specify[\\/](wkr|reviews)[\\/]|(^|[\\/\s])specs[\\/]\d+-' })) { exit 0 }

    $dir = if ($in.file_path) { Split-Path ([string]$in.file_path) -Parent } else { "$($call.cwd)" }
    while ($dir -and -not (Test-Path $dir)) { $dir = Split-Path $dir -Parent }
    $root = git -C $dir rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0 -or -not $root) { exit 0 }
    $cur = Join-Path $root '.specify/items/current.json'
    if (-not (Test-Path $cur)) { exit 0 }
    $id = (Get-Content $cur -Raw | ConvertFrom-Json).number

    Import-Module (Join-Path $root '.specify/extensions/engineering-checks/workflow/Workflow.psm1') -Force
    $cfg = Get-WorkflowConfig -Root $root
    # In "artifacts" mode every change to the item's folder is published to its issue straight away.
    try { $pub = @(Publish-ItemDocs $root $id); if ($pub.Count) { "auto-advance: published to #${id}: $($pub -join ', ')" } }
    catch { [Console]::Error.WriteLine("auto-advance: could not publish #$id's documents to its issue ($_); they are only local until the next write.") }
    # The manager reads this item's record and activity from this worktree.
    Set-StoredWorktree $id ([IO.Path]::GetFullPath($root))
    $ad = Get-BoardAdapter $cfg $root
    $item = & $ad.GetItem $id
    if (-not $item -or $item.tags -contains $cfg.tags.blocked) { exit 0 }
    if ((Get-StateDef $cfg $item.state).kind -ne 'work') { exit 0 }
    $order = Get-ItemOrder $cfg $item
    $i = $order.IndexOf($item.state)
    if ($i -lt 0 -or $i -ge $order.Count - 1) { exit 0 }
    $to = $order[$i + 1]

    $t = Test-Move -Config $cfg -Item $item -To $to -Adapter $ad -Root $root -AllItems @(& $ad.GetItems) -SkipScripts
    # What still stands between the item and its next state, other than a full WIP slot (the manager pulls when one frees).
    $missing = @($t.failures | Where-Object { $_ -notmatch ' is full \(' })
    if ($missing.Count) {
        # Tell the worker what is still missing, once per change: the same list is not repeated on every write.
        $seenFile = Join-Path $root ".specify/items/auto-advance-$id.txt"
        $now = "$($item.state)>$to`n" + ($missing -join "`n")
        $before = if (Test-Path $seenFile) { Get-Content $seenFile -Raw } else { '' }
        if ($now.Trim() -ne "$before".Trim()) {
            Set-Content $seenFile $now -Encoding utf8
            "auto-advance: #$id is not yet ready to leave $($item.state) for $to. Still missing:"
            $missing | ForEach-Object { "- $_" }
            "Do each of these you can, now and without asking; ask Martin only for one that needs his approval or decision."
        }
        exit 0
    }
    if (@($t.skipped).Count) { exit 0 }
    $r = Move-WorkItem -Config $cfg -Id $id -To $to -Reason "its $($t.shift.name) policy passed" -Adapter $ad -Root $root
    if ($r.ok -and $r.moved) { "auto-advance: #$id moved from $($r.from) to $($r.to) ($($t.shift.name) policy passed). Rename your chat for the new state and do its work." }
} catch {
    [Console]::Error.WriteLine("auto-advance: could not check the item's policy ($_); nothing moved.")
}
exit 0
