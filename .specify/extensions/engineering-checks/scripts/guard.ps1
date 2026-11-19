# Runs before each tool use. Reads the tool call as JSON on stdin.
# Exits 2 to block (the harness blocks only on exit 2), 0 to allow.
$ErrorActionPreference = 'Stop'
try {
    $raw = [Console]::In.ReadToEnd()
    $call = $raw | ConvertFrom-Json
} catch {
    [Console]::Error.WriteLine('guard: could not read the tool call; blocking.')
    exit 2
}
$inputs = $call.tool_input
$text = @($inputs.command, $inputs.file_path, $inputs.path) -join ' '

# Each command in a chain is checked on its own; git global options (git -C dir push) may come before push.
$pushBlocked = foreach ($seg in ($text -split '[|;&\r\n]+')) {
    $m = [regex]::Match($seg, '\bgit(\s+-\S+(\s+(?!push\b)[^\s-]\S*)?)*\s+push\b(?<rest>.*)$')
    if (-not $m.Success) { continue }
    $pa = @($m.Groups['rest'].Value.Trim() -split '\s+' | Where-Object { $_ } | ForEach-Object { $_.Trim('''"') })
    $force = $pa | Where-Object { $_ -match '^(--force(-with-lease)?(=.*)?|-[a-zA-Z]*f[a-zA-Z]*|\+.+)$' }
    $toMain = $pa | Where-Object { $_ -match '^(\+?(refs/heads/)?(main|master)|.*:(refs/heads/)?(main|master))$' }
    if ($force -or $toMain) { $true }
}
if ($pushBlocked) {
    [Console]::Error.WriteLine('guard: pushing to main or force-pushing is blocked. Merge through a PR and the merge queue.')
    exit 2
}
if ($text -match 'gh\s+issue\s+edit\b.*--(add|remove)-label\s+[''"]?state:' -or $text -match 'gh\s+project\s+item-edit\b' -or $text -match 'setIssueStatus|setIssueFieldValue|updateIssueFieldValue') {
    [Console]::Error.WriteLine('guard: board state is changed only through move-state.ps1, which checks the policy first.')
    exit 2
}
if ($call.tool_name -match 'Write|Edit' -and $text -match '\.specify[\\/]workflows[\\/]runs[\\/]') {
    [Console]::Error.WriteLine('guard: run state is written only by the workflow.')
    exit 2
}

# Draft until ready: a pull request is marked ready only through pr-ready.ps1, is always opened as a draft,
# and is marked draft before any edit on its branch.
$isShell = $call.tool_name -match '^(Bash|PowerShell)$'
$cmd = "$($inputs.command)"
if ($isShell) {
    $segs = $cmd -split '[|;&\r\n]+'
    if (($segs | Where-Object { $_ -match '\bgh\s+pr\s+ready\b' -and $_ -notmatch '--undo\b' }) -or $cmd -match 'markPullRequestReadyForReview') {
        [Console]::Error.WriteLine('guard: a pull request is marked ready only through pr-ready.ps1, which checks it is complete first.')
        exit 2
    }
    if ($segs | Where-Object { $_ -match '\bgh\s+pr\s+create\b' -and $_ -notmatch '(--draft|-d)\b' }) {
        [Console]::Error.WriteLine('guard: a pull request is opened as a draft (gh pr create --draft) and marked ready through pr-ready.ps1.')
        exit 2
    }
}
$writePattern = '(?i)\b(Set-Content|Add-Content|Out-File|Copy-Item|Move-Item|Remove-Item|New-Item|Rename-Item|Clear-Content|tee|cp|mv|rm|touch|truncate)\b|\bsed\s+-i|(?<![0-9&=-])>(?!\s*\$?null\b|&)|\bgit\b(\s+-\S+(\s+[^\s-]\S*)?)*\s+(add|commit|mv|rm|checkout|switch|reset|restore|merge|rebase|cherry-pick|revert|stash|apply|am|pull)\b|\bdotnet\s+(format|new|add)\b|\bnpm\s+(install|i|ci)\b'
$writes = ($call.tool_name -match '^(Write|Edit|MultiEdit|NotebookEdit)$') -or ($isShell -and $cmd -match $writePattern)
if ($writes) {
    $dir = if ($inputs.file_path) { Split-Path ([string]$inputs.file_path) -Parent } elseif ($inputs.notebook_path) { Split-Path ([string]$inputs.notebook_path) -Parent } else { "$($call.cwd)" }
    while ($dir -and -not (Test-Path $dir)) { $dir = Split-Path $dir -Parent }
    if (-not $dir) { exit 0 }
    git -C $dir rev-parse --is-inside-work-tree 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) { exit 0 }
    Push-Location $dir
    try {
        $out = gh pr view --json number,isDraft,state 2>&1
        if ($LASTEXITCODE -ne 0) {
            # No pull request, or no GitHub remote at all: nothing to mark draft.
            if ("$out" -match '(?i)no (open )?pull requests? found|no git remotes|none of the git remotes|not a git repository') { exit 0 }
            [Console]::Error.WriteLine("guard: could not check whether this branch has a pull request ready for review ($out); blocking the edit.")
            exit 2
        }
        $pr = "$out" | ConvertFrom-Json
        if ($pr.state -eq 'OPEN' -and -not $pr.isDraft) {
            gh pr ready $pr.number --undo 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                [Console]::Error.WriteLine("guard: PR #$($pr.number) is ready for review and could not be marked draft; blocking the edit.")
                exit 2
            }
            [Console]::Error.WriteLine("guard: PR #$($pr.number) marked draft before this edit; mark it ready only through pr-ready.ps1.")
        }
    } finally { Pop-Location }
}
exit 0