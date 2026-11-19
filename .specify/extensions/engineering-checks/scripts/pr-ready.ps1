# The only way a pull request is marked ready (the guard refuses `gh pr ready` everywhere else).
# It checks, in this order, and marks the PR ready only if all pass:
#   1. the working tree is clean (nothing uncommitted or untracked);
#   2. the branch has a remote branch and matches it (nothing unpushed, nothing to pull);
#   3. the shared copies under .specify/extensions/ match their source (nkda-speckit's extensions/);
#   4. done-checks.ps1 passes.
# The source is NKDA_SPECKIT_SOURCE, or `sharedSource` in .specify/memory/workflow.json, relative to the
# repo's main checkout. In the source repo itself there are no copies to compare.
param([int]$Number)
$ErrorActionPreference = 'Stop'
$fail = [Collections.Generic.List[string]]::new()

$dirty = @(git status --porcelain)
if ($LASTEXITCODE -ne 0) { Write-Error 'git status failed; the working tree is not known.'; exit 1 }
if ($dirty) { $fail.Add("the working tree has uncommitted changes: $(($dirty | ForEach-Object { $_.Substring(3) }) -join ', ')") }

git fetch -q 2>$null
$up = git rev-parse --abbrev-ref --symbolic-full-name '@{u}' 2>$null
if ($LASTEXITCODE -ne 0 -or -not $up) { $fail.Add('the branch has no remote branch: push it first') }
else {
    $head = git rev-parse HEAD; $remote = git rev-parse '@{u}'
    if ($head -ne $remote) {
        $ahead = git rev-list --count '@{u}..HEAD'; $behind = git rev-list --count 'HEAD..@{u}'
        $fail.Add("the branch does not match $up ($ahead unpushed, $behind not pulled)")
    }
}

$isSource = Test-Path 'extensions/engineering-checks/extension.yml'
if (-not $isSource) {
    $src = $env:NKDA_SPECKIT_SOURCE
    if (-not $src -and (Test-Path .specify/memory/workflow.json)) {
        $c = Get-Content .specify/memory/workflow.json -Raw | ConvertFrom-Json
        if ($c.PSObject.Properties['sharedSource'] -and $c.sharedSource) {
            $main = Split-Path (Resolve-Path (git rev-parse --git-common-dir)).Path -Parent
            $src = if ([IO.Path]::IsPathRooted($c.sharedSource)) { $c.sharedSource } else { Join-Path $main $c.sharedSource }
        }
    }
    if (-not $src -or -not (Test-Path (Join-Path $src 'extensions'))) { $fail.Add('the shared copies cannot be checked: set NKDA_SPECKIT_SOURCE or sharedSource in .specify/memory/workflow.json') }
    else {
        $srcExt = (Resolve-Path (Join-Path $src 'extensions')).Path
        foreach ($f in Get-ChildItem $srcExt -Recurse -File) {
            $rel = $f.FullName.Substring($srcExt.Length).TrimStart('\', '/')
            $copy = Join-Path '.specify/extensions' $rel
            if (-not (Test-Path $copy)) { $fail.Add("shared file not copied in: .specify/extensions/$rel"); continue }
            $a = (Get-Content $f.FullName -Raw) -replace "`r`n", "`n"; $b = (Get-Content $copy -Raw) -replace "`r`n", "`n"
            if ($a -ne $b) { $fail.Add("shared file differs from its source: .specify/extensions/$rel") }
        }
    }
}

if (-not $fail.Count) {
    $done = @('.specify/extensions/engineering-checks/scripts/done-checks.ps1', 'extensions/engineering-checks/scripts/done-checks.ps1') | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $done) { $fail.Add('done-checks.ps1 was not found') }
    else { pwsh -NoProfile -File $done; if ($LASTEXITCODE -ne 0) { $fail.Add('done-checks.ps1 failed') } }
}

if ($fail.Count) { $fail | ForEach-Object { Write-Error "not ready: $_" -ErrorAction Continue }; exit 1 }
$args2 = @('pr', 'ready'); if ($Number) { $args2 += "$Number" }
gh @args2
if ($LASTEXITCODE -ne 0) { exit 1 }
exit 0
