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
if ($LASTEXITCODE -ne 0) { Write-Error 'git fetch failed: the remote branch cannot be checked, so the PR is not marked ready.'; exit 1 }
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
        Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
        foreach ($d in (Get-SharedDrift $src (Get-Location).Path)) { $fail.Add("shared file differs from its source: $d (run nkda-speckit's scripts/sync-to-repo.ps1)") }
    }
}

# Specs are not kept (Martin): once the outcome checks have passed, the item's folder, specs/<issue>-<name>/, is
# reconciled into the docs and deleted in this pull request, before it is marked ready.
if (Test-Path .specify/items/current.json) {
    $curId = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
    $left = Get-ChildItem specs -Directory -Filter "$curId-*" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($left) { $fail.Add("specs/$($left.Name)/ is still here: reconcile what was decided and built into the docs, then delete the folder in this pull request") }
}
# No temporary Spec Kit or workflow file reaches main: a bug or idea assessment, a review file, or an older item record.
$temp = @(git ls-files -- .specify/bugs .specify/assessments .specify/reviews .specify/wkr 2>$null)
if ($temp) { $fail.Add("temporary workflow files are tracked on this branch: $($temp -join ', '). Fold what matters into the item's record or the docs, then remove them (git rm) in this pull request") }

if (-not $fail.Count) {
    $done = @('.specify/extensions/engineering-checks/scripts/done-checks.ps1', 'extensions/engineering-checks/scripts/done-checks.ps1') | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $done) { $fail.Add('done-checks.ps1 was not found') }
    else { pwsh -NoProfile -File $done; if ($LASTEXITCODE -ne 0) { $fail.Add('done-checks.ps1 failed') } }
}

if ($fail.Count) { $fail | ForEach-Object { Write-Error "not ready: $_" -ErrorAction Continue }; exit 1 }
# Only this branch's own PR is marked ready: the checks above were run on this branch.
$mine = gh pr view --json number 2>$null | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or -not $mine) { Write-Error 'this branch has no open pull request.'; exit 1 }
if ($Number -and $Number -ne $mine.number) { Write-Error "PR #$Number is not this branch's pull request (#$($mine.number)); only the branch that passed the checks can be marked ready."; exit 1 }
gh pr ready $mine.number
if ($LASTEXITCODE -ne 0) { exit 1 }
exit 0
