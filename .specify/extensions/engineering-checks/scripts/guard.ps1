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
# A push to main is refused unless the repo's .specify/memory/workflow.json sets directToMain, for example
#   "directToMain": { "allowed": true, "only": "fix", "paths": ["system/**"], "never": ["specs/**"] }
# and then only a plain push of local main whose every commit is of that kind and touches only those paths.
function ConvertTo-PushGlob([string]$g) { '^' + ([regex]::Escape($g) -replace '\\\*\\\*', '.*' -replace '\\\*', '[^/]*' -replace '\\\?', '.') + '$' }
function Get-DirectToMainRefusal([string]$dir, [string[]]$pa) {
    $cfgPath = Join-Path $dir '.specify/memory/workflow.json'
    $root = git -C $dir rev-parse --show-toplevel 2>$null
    if ($root) { $cfgPath = Join-Path $root '.specify/memory/workflow.json' }
    $d = if (Test-Path $cfgPath) { (Get-Content $cfgPath -Raw | ConvertFrom-Json).directToMain } else { $null }
    if (-not $d -or -not $d.allowed) { return 'pushing to main is blocked. Merge through a PR and the merge queue.' }
    $flags = @($pa | Where-Object { $_ -like '-*' -and $_ -notin '-u', '--set-upstream', '-q', '--quiet', '-v', '--verbose' })
    $args2 = @($pa | Where-Object { $_ -notlike '-*' })
    if ($flags.Count -or $args2.Count -gt 2 -or ($args2 | Where-Object { $_ -match '[:+]' }) -or ($args2.Count -eq 2 -and $args2[1] -notin 'main', 'master')) {
        return 'push local main exactly as it is: no refspec, --all, --tags, --delete or other option.'
    }
    $branch = git -C $dir rev-parse --abbrev-ref HEAD 2>$null
    if ($branch -notin 'main', 'master') { return "only local main can be pushed to main (this is $branch)." }
    git -C $dir fetch -q origin $branch 2>$null
    $commits = @(git -C $dir rev-list "origin/$branch..HEAD" 2>$null)
    if ($LASTEXITCODE -ne 0) { return "the commits to push could not be listed (origin/$branch..HEAD)." }
    $kind = if ($d.only) { [string]$d.only } else { 'fix' }
    $paths = @($d.paths | ForEach-Object { ConvertTo-PushGlob $_ })
    $never = @($d.never | ForEach-Object { ConvertTo-PushGlob $_ })
    foreach ($c in $commits) {
        $subject = git -C $dir log -1 --format=%s $c
        if ($subject -notmatch "^$([regex]::Escape($kind))(\([^)]*\))?!?:") { return "commit $($c.Substring(0,8)) is not a $kind commit: $subject" }
        foreach ($f in @(git -C $dir diff-tree --no-commit-id --name-only -r --root $c)) {
            if ($never | Where-Object { $f -match $_ }) { return "commit $($c.Substring(0,8)) touches $f, which never goes to main directly." }
            if (-not ($paths | Where-Object { $f -match $_ })) { return "commit $($c.Substring(0,8)) touches $f, outside the paths that may go to main directly." }
        }
    }
    $null
}
foreach ($seg in ($text -split '[|;&\r\n]+')) {
    $m = [regex]::Match($seg, '\bgit(?<g>(\s+-\S+(\s+(?!push\b)[^\s-]\S*)?)*)\s+push\b(?<rest>.*)$')
    if (-not $m.Success) { continue }
    $pa = @($m.Groups['rest'].Value.Trim() -split '\s+' | Where-Object { $_ } | ForEach-Object { $_.Trim('''"') })
    $force = $pa | Where-Object { $_ -match '^(--force(-with-lease)?(=.*)?|-[a-zA-Z]*f[a-zA-Z]*|\+.+)$' }
    if ($force) { [Console]::Error.WriteLine('guard: force-pushing is blocked.'); exit 2 }
    $c = [regex]::Match($m.Groups['g'].Value, '-C\s+(?<d>"[^"]+"|''[^'']+''|\S+)')
    $dir = if ($c.Success) { $c.Groups['d'].Value.Trim('''"') } elseif ($call.cwd) { "$($call.cwd)" } else { (Get-Location).Path }
    $refs = @($pa | Where-Object { $_ -notlike '-*' })
    # Pushes to main: a refspec naming it, --all or --mirror, or a plain push (no refspec) from local main.
    $toMain = ($refs | Where-Object { $_ -match '^(\+?(refs/heads/)?(main|master)|.*:(refs/heads/)?(main|master))$' }) -or ($pa | Where-Object { $_ -in '--all', '--mirror' })
    if (-not $toMain -and $refs.Count -le 1) { $toMain = (git -C $dir rev-parse --abbrev-ref HEAD 2>$null) -in 'main', 'master' }
    if ($toMain) {
        $why = Get-DirectToMainRefusal $dir $pa
        if ($why) { [Console]::Error.WriteLine("guard: $why"); exit 2 }
    }
}
if ($text -match 'gh\s+issue\s+edit\b.*--(add|remove)-label\s+[''"]?state:' -or $text -match 'gh\s+project\s+item-edit\b' -or $text -match 'setIssueStatus|setIssueFieldValue|updateIssueFieldValue') {
    [Console]::Error.WriteLine('guard: board state is changed only through move-state.ps1, which checks the policy first.')
    exit 2
}
if ($call.tool_name -match 'Write|Edit' -and $text -match '\.specify[\\/]workflows[\\/]runs[\\/]') {
    [Console]::Error.WriteLine('guard: run state is written only by the workflow.')
    exit 2
}

# The worker's Purpose decides scope. The item's record (specs/<issue>-<name>/item.md) carries front matter:
#   ---
#   purpose: <the one outcome this worker delivers>
#   paths:
#     - src/MigrationTools/**
#   ---
# An edit to a repo file that matches none of the paths, or a build, test or end-to-end command when the paths
# include no product code (productPaths in .specify/memory/workflow.json), asks Martin. No current item, or a
# record without paths, means no opinion. The workflow's own files (specs/, .specify/items, reviews, bugs) are allowed.
function ConvertTo-GlobRegex([string]$Glob) {
    $g = $Glob.Trim().Replace('\', '/').TrimStart('/')
    # "folder/**" also matches the folder itself.
    if ($g.EndsWith('/**')) { return ((ConvertTo-GlobRegex $g.Substring(0, $g.Length - 3)).TrimEnd('$') + '(?:/.*)?$') }
    $sb = [Text.StringBuilder]::new('^'); $i = 0
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
    # The item's record: specs/<issue>-<name>/item.md (an older .specify/wkr/<issue>.md is still read).
    $folder = Get-ChildItem (Join-Path $Root 'specs') -Directory -Filter "$id-*" -ErrorAction SilentlyContinue | Select-Object -First 1
    $wkrRel = if ($folder) { "specs/$($folder.Name)/item.md" } else { ".specify/wkr/$id.md" }
    $wkr = Join-Path $Root $wkrRel
    if (-not (Test-Path $wkr)) { return $null }
    $lines = [IO.File]::ReadAllLines($wkr)
    $start = [array]::IndexOf(($lines | ForEach-Object { $_.Trim() }), '---')
    if ($start -lt 0) { return $null }
    $purpose = ''; $paths = @(); $inPaths = $false; $closed = $false
    for ($i = $start + 1; $i -lt $lines.Count; $i++) {
        $l = $lines[$i]
        if ($l.Trim() -eq '---') { $closed = $true; break }
        if ($l -match '^purpose:\s*(?<v>.+)$') { $purpose = $Matches['v'].Trim('"', "'"); $inPaths = $false; continue }
        if ($l -match '^paths:\s*$') { $inPaths = $true; continue }
        if ($inPaths) { if ($l -match '^\s*-\s*(?<v>.+?)\s*$') { $p = ($Matches['v'] -replace '\s+#.*$', '').Trim().Trim('"', "'"); if ($p) { $paths += $p }; continue }; if ($l -match '^\S') { $inPaths = $false } }
    }
    if (-not $closed -or -not $paths) { return $null }  # a front-matter block that is not closed is not a scope
    $product = @('src/')
    $cfg = Join-Path $Root '.specify/memory/workflow.json'
    $specRoot = 'docs/specs'
    if (Test-Path $cfg) {
        $c = Get-Content $cfg -Raw | ConvertFrom-Json
        if ($c.PSObject.Properties['productPaths'] -and $c.productPaths) { $product = @($c.productPaths) }
        if ($c.PSObject.Properties['specRoot'] -and $c.specRoot) { $specRoot = $c.specRoot.TrimEnd('/') }
    }
    # The docs of every system the item's paths touch ({specRoot}/{system}/): the item must update them, so they are
    # always within its scope. Systems come from {specRoot}/systems.md ("- <system>: <path> <path> ...").
    $docs = @()
    $sys = Join-Path $Root "$specRoot/systems.md"
    if (Test-Path $sys) {
        foreach ($m in [regex]::Matches((Get-Content $sys -Raw), '(?m)^\s*-\s*([^:]+):\s*(.+)$')) {
            $name = $m.Groups[1].Value.Trim()
            $sysPaths = @($m.Groups[2].Value -split '[,\s]+' | Where-Object { $_ } | ForEach-Object { $_.Trim('`') })
            if ($paths | Where-Object { $g = $_; $sysPaths | Where-Object { Test-GlobsOverlap $g $_ } }) { $docs += "$specRoot/$name/**" }
        }
    }
    @{ id = $id; wkr = $wkrRel; purpose = $purpose; paths = $paths; product = $product; docs = $docs }
}
function Write-Ask([string]$Reason) {
    @{ hookSpecificOutput = @{ hookEventName = 'PreToolUse'; permissionDecision = 'ask'; permissionDecisionReason = $Reason } } | ConvertTo-Json -Depth 5 -Compress
    exit 0
}
function Get-ShellWriteTargets([string]$Command) {
    # Best effort: the files a shell command writes. Redirections (> and >>), the path arguments of the
    # PowerShell file cmdlets, and the operands of cp, mv, rm, touch, tee and sed -i.
    $t = [Collections.Generic.List[string]]::new()
    foreach ($m in [regex]::Matches($Command, '(?<![0-9&=-])>>?\s*(?<q>[''"]?)(?<p>[^\s''"|;&>]+)\k<q>')) { if ($m.Groups['p'].Value -notmatch '^\$?null$|^&') { $t.Add($m.Groups['p'].Value) } }
    foreach ($seg in ($Command -split '[|;&\r\n]+')) {
        $toks = @([regex]::Matches($seg, '(?<q>[''"])(?<v>.*?)\k<q>|(?<v>[^\s''"]+)') | ForEach-Object { $_.Groups['v'].Value })
        for ($i = 0; $i -lt $toks.Count; $i++) {
            $cmdName = $toks[$i]
            if ($cmdName -match '^(?i)(Set-Content|Add-Content|Out-File|New-Item|Copy-Item|Move-Item|Remove-Item|Rename-Item|Clear-Content|cp|mv|rm|touch|tee|truncate)$') {
                # Only what the command writes: the destination of a copy or move; the path of every other cmdlet
                # (never the text being written, nor the source being read); every operand of rm, touch, tee, truncate.
                $named = $null; $positional = [Collections.Generic.List[string]]::new(); $takeNext = $null
                for ($j = $i + 1; $j -lt $toks.Count; $j++) {
                    $v = $toks[$j]
                    if ($takeNext) { if ($takeNext -eq 'target') { $named = $v }; $takeNext = $null; continue }
                    if ($cmdName -match '^(?i)(Copy-Item|Move-Item)$' -and $v -match '^(?i)-Destination$') { $takeNext = 'target'; continue }
                    if ($cmdName -notmatch '^(?i)(Copy-Item|Move-Item|cp|mv|rm|touch|tee|truncate)$' -and $v -match '^(?i)-(Path|LiteralPath|FilePath)$') { $takeNext = 'target'; continue }
                    if ($v -match '^-[A-Za-z]') { if ($v -match '^(?i)-(Value|Encoding|ItemType|Name|NewName|Filter|Include|Exclude|InputObject|Width|Path|LiteralPath)$') { $takeNext = 'skip' }; continue }
                    $positional.Add($v)
                }
                $cands = if ($named) { @($named) }
                         elseif ($cmdName -match '^(?i)(Copy-Item|Move-Item|cp|mv)$') { @($positional | Select-Object -Last 1) }
                         elseif ($cmdName -match '^(?i)(rm|touch|tee|truncate)$') { @($positional) }
                         else { @($positional | Select-Object -First 1) }
                foreach ($v in $cands) {
                    # Not a path: a variable, a here-string marker (@' or @"), or a token with no letters or digits in it.
                    if (-not $v -or $v -match '^\$' -or $v -match '^@|@$' -or $v -notmatch '[\w]' -or $v -match '[\r\n]') { continue }
                    $t.Add($v)
                }
            }
            if ($toks[$i] -eq 'sed' -and ($toks -contains '-i')) { $t.Add($toks[-1]) }
        }
    }
    , $t
}
# Draft until ready: a pull request is marked ready only through pr-ready.ps1, is always opened as a draft,
# and is marked draft before any edit on its branch. This runs before the scope check below, so an edit that is
# then approved outside the paths never lands while the pull request is ready.
$isShell = $call.tool_name -match '^(Bash|PowerShell)$'
$cmd = "$($inputs.command)"
$segs = @($cmd -split '[|;&\r\n]+')
if ($isShell) {
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
    # The branch being edited is the one the write lands in. A shell command that only writes files elsewhere (temp
    # files, say) leaves this branch's pull request alone; git, dotnet and npm commands change the repo they run in.
    $repoCmd = $isShell -and $cmd -match '(?i)\bgit\b|\bdotnet\s+(format|new|add)\b|\bnpm\s+(install|i|ci)\b|\bsed\s+-i'
    $shellTargets = if ($isShell -and -not $repoCmd) { @(Get-ShellWriteTargets $cmd | Where-Object { $_ }) } else { @() }
    $dir = if ($inputs.file_path) { Split-Path ([string]$inputs.file_path) -Parent } elseif ($inputs.notebook_path) { Split-Path ([string]$inputs.notebook_path) -Parent } else { "$($call.cwd)" }
    if ($shellTargets.Count) {
        $inside = @($shellTargets | ForEach-Object {
            $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($_)) { $_ } else { Join-Path "$($call.cwd)" $_ }))
            $d = Split-Path $full -Parent
            while ($d -and -not (Test-Path $d)) { $d = Split-Path $d -Parent }
            if ($d) { $top = git -C $d rev-parse --show-toplevel 2>$null; if ($LASTEXITCODE -eq 0 -and $top) { $d } }
        })
        $dir = if ($inside.Count) { $inside[0] } else { $null }
    }
    while ($dir -and -not (Test-Path $dir)) { $dir = Split-Path $dir -Parent }
    $inRepo = $false
    if ($dir) { git -C $dir rev-parse --is-inside-work-tree 2>$null | Out-Null; $inRepo = $LASTEXITCODE -eq 0 }
    # A file git ignores (an item's working copy kept as issue artifacts, say) is not a change to the branch.
    $editTarget = if ($inputs.file_path) { [string]$inputs.file_path } elseif ($inputs.notebook_path) { [string]$inputs.notebook_path } else { $null }
    if ($inRepo -and $editTarget -and -not $isShell) { git -C $dir check-ignore -q -- $editTarget 2>$null; if ($LASTEXITCODE -eq 0) { $inRepo = $false } }
    if ($inRepo) {
        Push-Location $dir
        try {
            $out = gh pr view --json number,isDraft,state 2>&1
            if ($LASTEXITCODE -ne 0) {
                # No pull request, or no GitHub remote at all: nothing to mark draft.
                if ("$out" -notmatch '(?i)no (open )?pull requests? found|no git remotes|none of the git remotes|not a git repository') {
                    [Console]::Error.WriteLine("guard: could not check whether this branch has a pull request ready for review ($out); blocking the edit.")
                    exit 2
                }
            } else {
                $pr = "$out" | ConvertFrom-Json
                if ($pr.state -eq 'OPEN' -and -not $pr.isDraft) {
                    gh pr ready $pr.number --undo 2>&1 | Out-Null
                    if ($LASTEXITCODE -ne 0) {
                        [Console]::Error.WriteLine("guard: PR #$($pr.number) is ready for review and could not be marked draft; blocking the edit.")
                        exit 2
                    }
                    [Console]::Error.WriteLine("guard: PR #$($pr.number) marked draft before this edit; mark it ready only through pr-ready.ps1.")
                }
            }
        } finally { Pop-Location }
    }
}

# The worker's Purpose decides scope (after the draft step).
function Test-GlobsOverlap([string]$Glob, [string]$Prefix) {
    # Whether a paths glob can match a file under a product path: the glob's literal start and the prefix agree as
    # far as both go (so **/*.cs overlaps src/, and docs/** does not).
    $g = $Glob.Replace('\', '/').TrimStart('/'); $p = $Prefix.Replace('\', '/').TrimStart('/')
    $lit = ($g -split '[*?\[]', 2)[0]
    $n = [Math]::Min($lit.Length, $p.Length)
    $lit.Substring(0, $n).Equals($p.Substring(0, $n), [StringComparison]::OrdinalIgnoreCase)
}
$scopeDir = if ($inputs.file_path) { Split-Path ([string]$inputs.file_path) -Parent } elseif ($inputs.notebook_path) { Split-Path ([string]$inputs.notebook_path) -Parent } else { "$($call.cwd)" }
while ($scopeDir -and -not (Test-Path $scopeDir)) { $scopeDir = Split-Path $scopeDir -Parent }
$scopeRoot = if ($scopeDir) { git -C $scopeDir rev-parse --show-toplevel 2>$null } else { $null }
if ($LASTEXITCODE -ne 0) { $scopeRoot = $null }
$scope = if ($scopeRoot) { Get-Scope $scopeRoot } else { $null }

# Product code is changed only for an item in Building, Validating or In review: an item gets there only through
# Refining, an approved spec and the Ready gate. No item, or an item in any other state, blocks the edit.
if ($scopeRoot -and (Test-Path (Join-Path $scopeRoot '.specify/memory/workflow.json'))) {
    $productTargets = @()
    $writeTargets = if ($call.tool_name -match '^(Write|Edit|MultiEdit|NotebookEdit)$') { @(if ($inputs.file_path) { [string]$inputs.file_path } elseif ($inputs.notebook_path) { [string]$inputs.notebook_path }) } elseif ($isShell) { @(Get-ShellWriteTargets $cmd) } else { @() }
    $wf = Get-Content (Join-Path $scopeRoot '.specify/memory/workflow.json') -Raw | ConvertFrom-Json
    $productPaths = if ($wf.PSObject.Properties['productPaths'] -and $wf.productPaths) { @($wf.productPaths) } else { @('src/') }
    $rootFull = [IO.Path]::GetFullPath($scopeRoot).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    foreach ($t in $writeTargets) {
        if (-not $t) { continue }
        $full = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($t)) { $t } else { Join-Path "$($call.cwd)" $t }))
        if (-not $full.StartsWith($rootFull, [StringComparison]::OrdinalIgnoreCase)) { continue }
        $rel = $full.Substring($rootFull.Length).Replace('\', '/')
        if ($productPaths | Where-Object { $p = $_.Replace('\', '/').TrimStart('/'); if ($p.EndsWith('/')) { $rel.StartsWith($p, [StringComparison]::OrdinalIgnoreCase) } else { $rel -ieq $p -or $rel.StartsWith("$p/", [StringComparison]::OrdinalIgnoreCase) } }) { $productTargets += $rel }
    }
    if ($productTargets) {
        $cur = Join-Path $scopeRoot '.specify/items/current.json'
        if (-not (Test-Path $cur)) {
            [Console]::Error.WriteLine("guard: $($productTargets[0]) is product code, and this branch has no item. Product code is changed only by the worker for an item in Building, after Refining, an approved spec and the Ready gate.")
            exit 2
        }
        $itemId = (Get-Content $cur -Raw | ConvertFrom-Json).number
        Import-Module (Join-Path $scopeRoot '.specify/extensions/engineering-checks/workflow/Workflow.psm1') -Force
        $wcfg = Get-WorkflowConfig -Root $scopeRoot
        $gItem = & (Get-BoardAdapter $wcfg $scopeRoot).GetItem $itemId
        $state = $gItem.state
        # The states that may change product code are the repo's own: each is flagged "editsCode" in its workflow.json.
        if (-not (Test-StateFlag $wcfg $state 'editsCode')) {
            $codeStates = @($wcfg.states | Where-Object { $_.ContainsKey('editsCode') -and $_.editsCode } | ForEach-Object { $_.name }) -join ', '
            [Console]::Error.WriteLine("guard: $($productTargets[0]) is product code, and #$itemId is in $state. Product code is changed only in $($codeStates): through Refining, Martin's approval of the spec, and the Ready gate.")
            exit 2
        }
        if (@($gItem.tags) -contains 'spec-reconcile-required') {
            [Console]::Error.WriteLine("guard: $($productTargets[0]) is product code, and #$itemId is tagged spec-reconcile-required. Revise the spec and get Martin's approval of the new version first; then clear the tag with set-blocked.ps1 -Clear.")
            exit 2
        }
    }
}

if ($scope) {
    $outside = "This is outside the Purpose of #$($scope.id): $($scope.purpose) (paths: $($scope.paths -join ', '))."
    $how = " If the agreed spec needs it, add it to paths: in $($scope.wkr) and ask Martin to approve; otherwise record it on the item as a proposed follow-up."
    $targets = @()
    if ($call.tool_name -match '^(Write|Edit|MultiEdit|NotebookEdit)$') { $targets = @(if ($inputs.file_path) { [string]$inputs.file_path } elseif ($inputs.notebook_path) { [string]$inputs.notebook_path }) }
    elseif ($isShell) { $targets = @(Get-ShellWriteTargets $cmd) }
    $rootFull = [IO.Path]::GetFullPath($scopeRoot).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    foreach ($target in $targets) {
        if (-not $target) { continue }
        $abs = if ([IO.Path]::IsPathRooted($target)) { $target } else { Join-Path "$($call.cwd)" $target }
        $full = [IO.Path]::GetFullPath($abs)
        if (-not $full.StartsWith($rootFull, [StringComparison]::OrdinalIgnoreCase)) { continue }
        $rel = $full.Substring($rootFull.Length).Replace('\', '/')
        # The workflow's own files are never outside an item's scope: its record, reviews, bug assessments,
        # idea assessments and working spec folders.
        $own = $rel -match '^/?(\.specify/(wkr|items|reviews|bugs|assessments)/|specs/)'
        # So are the docs of the systems its paths touch, which it must update.
        if (-not $own -and ($scope.docs | Where-Object { $rel -match (ConvertTo-GlobRegex $_) })) { $own = $true }
        if (-not $own -and -not ($scope.paths | Where-Object { $rel -match (ConvertTo-GlobRegex $_) })) { Write-Ask "$rel is not in the paths. $outside$how" }
    }
    $buildCmd = '(?i)\bdotnet\s+(build|test|run|publish|pack)\b|\bmsbuild\b|\bproof\.ps1\b|\bInvoke-MigrationE2E\.ps1\b|\bInvoke-Pester\b|\bnpm\s+(run|test)\b|\bBuild\.ps1\b'
    if ($isShell -and $cmd -match $buildCmd) {
        $touchesProduct = $scope.paths | Where-Object { $g = $_; $scope.product | Where-Object { Test-GlobsOverlap $g $_ } }
        if (-not $touchesProduct) { Write-Ask "A build, test or end-to-end run, but the paths include no product code ($($scope.product -join ', ')). $outside$how" }
    }
}
exit 0