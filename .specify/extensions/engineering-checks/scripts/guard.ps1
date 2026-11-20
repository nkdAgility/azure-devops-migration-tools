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
# Fails closed: any error in the guard blocks the call.
trap { [Console]::Error.WriteLine("guard: error ($_); blocking."); exit 2 }
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

# The worker's Purpose decides scope. The item's WKR file (.specify/wkr/<n>.md) carries front matter:
#   ---
#   purpose: <the one outcome this worker delivers>
#   paths:
#     - src/MigrationTools/**
#   ---
# An edit to a repo file that matches none of the paths, or a build, test or end-to-end command when the paths
# include no product code (productPaths in .specify/memory/workflow.json), asks Martin. No current item, or a
# WKR file without paths, means no opinion. The workflow's own files (.specify/wkr, items, reviews) are allowed.
function ConvertTo-GlobRegex([string]$Glob) {
    $g = $Glob.Trim().Replace('\', '/').TrimStart('/'); $sb = [Text.StringBuilder]::new('^'); $i = 0
    while ($i -lt $g.Length) {
        $c = $g[$i]
        if ($c -eq '*') {
            if ($i + 1 -lt $g.Length -and $g[$i + 1] -eq '*') {
                if ($i + 2 -lt $g.Length -and $g[$i + 2] -eq '/') { [void]$sb.Append('(?:.*/)?'); $i += 3; continue }
                [void]$sb.Append('.*'); $i += 2; continue
            }
            [void]$sb.Append('[^/]*')
        } elseif ($c -eq '?') { [void]$sb.Append('[^/]') } else { [void]$sb.Append([regex]::Escape([string]$c)) }
        $i++
    }
    [void]$sb.Append('$'); $sb.ToString()
}
function Get-Scope([string]$Root) {
    $cur = Join-Path $Root '.specify/items/current.json'
    if (-not (Test-Path $cur)) { return $null }
    $id = (Get-Content $cur -Raw | ConvertFrom-Json).number
    $wkrRel = ".specify/wkr/$id.md"; $wkr = Join-Path $Root $wkrRel
    if (-not (Test-Path $wkr)) { return $null }
    $lines = [IO.File]::ReadAllLines($wkr)
    $start = [array]::IndexOf(($lines | ForEach-Object { $_.Trim() }), '---')
    if ($start -lt 0) { return $null }
    $purpose = ''; $paths = @(); $inPaths = $false
    for ($i = $start + 1; $i -lt $lines.Count; $i++) {
        $l = $lines[$i]
        if ($l.Trim() -eq '---') { break }
        if ($l -match '^purpose:\s*(?<v>.+)$') { $purpose = $Matches['v'].Trim('"', "'"); $inPaths = $false; continue }
        if ($l -match '^paths:\s*$') { $inPaths = $true; continue }
        if ($inPaths) { if ($l -match '^\s*-\s*(?<v>.+?)\s*$') { $paths += $Matches['v'].Trim('"', "'"); continue }; if ($l -match '^\S') { $inPaths = $false } }
    }
    if (-not $paths) { return $null }
    $product = @('src/')
    $cfg = Join-Path $Root '.specify/memory/workflow.json'
    if (Test-Path $cfg) { $c = Get-Content $cfg -Raw | ConvertFrom-Json; if ($c.PSObject.Properties['productPaths'] -and $c.productPaths) { $product = @($c.productPaths) } }
    @{ id = $id; wkr = $wkrRel; purpose = $purpose; paths = $paths; product = $product }
}
function Write-Ask([string]$Reason) {
    @{ hookSpecificOutput = @{ hookEventName = 'PreToolUse'; permissionDecision = 'ask'; permissionDecisionReason = $Reason } } | ConvertTo-Json -Depth 5 -Compress
    exit 0
}
$scopeDir = if ($inputs.file_path) { Split-Path ([string]$inputs.file_path) -Parent } elseif ($inputs.notebook_path) { Split-Path ([string]$inputs.notebook_path) -Parent } else { "$($call.cwd)" }
while ($scopeDir -and -not (Test-Path $scopeDir)) { $scopeDir = Split-Path $scopeDir -Parent }
$scopeRoot = if ($scopeDir) { git -C $scopeDir rev-parse --show-toplevel 2>$null } else { $null }
if ($LASTEXITCODE -ne 0) { $scopeRoot = $null }
$scope = if ($scopeRoot) { Get-Scope $scopeRoot } else { $null }
if ($scope) {
    $outside = "This is outside the Purpose of #$($scope.id): $($scope.purpose) (paths: $($scope.paths -join ', '))."
    $how = " If the agreed spec needs it, add it to paths: in $($scope.wkr) and ask Martin to approve; otherwise record it on the item as a proposed follow-up."
    $target = if ($inputs.file_path) { [string]$inputs.file_path } elseif ($inputs.notebook_path) { [string]$inputs.notebook_path } else { '' }
    if ($target -and $call.tool_name -match '^(Write|Edit|MultiEdit|NotebookEdit)$') {
        $full = [IO.Path]::GetFullPath($target); $rootFull = [IO.Path]::GetFullPath($scopeRoot).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
        if ($full.StartsWith($rootFull, [StringComparison]::OrdinalIgnoreCase)) {
            $rel = $full.Substring($rootFull.Length).Replace('\', '/')
            $own = $rel -match '^\.specify/(wkr|items|reviews)/'
            if (-not $own -and -not ($scope.paths | Where-Object { $rel -match (ConvertTo-GlobRegex $_) })) { Write-Ask "$rel is not in the paths. $outside$how" }
        }
    }
    $buildCmd = '(?i)\bdotnet\s+(build|test|run|publish|pack)\b|\bmsbuild\b|\bproof\.ps1\b|\bInvoke-MigrationE2E\.ps1\b|\bInvoke-Pester\b|\bnpm\s+(run|test)\b|\bBuild\.ps1\b'
    if ($call.tool_name -match '^(Bash|PowerShell)$' -and "$($inputs.command)" -match $buildCmd) {
        $touchesProduct = $scope.paths | Where-Object { $p = $_.Replace('\', '/').TrimStart('/'); $scope.product | Where-Object { $p.StartsWith($_.Replace('\', '/').TrimStart('/'), [StringComparison]::OrdinalIgnoreCase) } }
        if (-not $touchesProduct) { Write-Ask "A build, test or end-to-end run, but the paths include no product code ($($scope.product -join ', ')). $outside$how" }
    }
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