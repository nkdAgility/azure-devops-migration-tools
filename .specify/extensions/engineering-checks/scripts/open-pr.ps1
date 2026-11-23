# Opens a PR naming the item and attaches the completion report. Idempotent. State is moved by the policy, not here.
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$title = (gh issue view $id --json title | ConvertFrom-Json).title
$body = "Closes #$id`n`n"
if (Test-Path .specify/reviews/completion-report.md) { $body += Get-Content .specify/reviews/completion-report.md -Raw }
# The item's branch on the remote is topic/<item folder> (topic/<id>-<short name>), whatever the local branch is called.
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$folder = Get-ItemFolder '.' $id
$head = if ($folder) { "topic/$(Split-Path $folder -Leaf)" } else { "topic/$id" }
git push -u origin "HEAD:refs/heads/$head"
if ($LASTEXITCODE -ne 0) { Write-Error "could not push the branch to $head."; exit 1 }
# A pull request already open from the topic branch, or from this local branch (opened before the topic branch was used).
$existing = gh pr view $head --json number,body,isDraft 2>$null | ConvertFrom-Json
if (-not $existing) { $existing = gh pr view --json number,body,isDraft 2>$null | ConvertFrom-Json }
if (-not $existing) { gh pr create --draft --head $head --title $title --body $body; if ($LASTEXITCODE -ne 0) { exit 1 } }
elseif ("$($existing.body)" -notmatch "(?i)\b(close[sd]?|fix(e[sd])?|resolve[sd]?)\s*:?\s+#$id\b") { Write-Error "PR #$($existing.number) on this branch does not close #$id; it was left unchanged."; exit 1 }
else {
    # A ready PR is made a draft before it changes; if that fails, it is left unchanged.
    if (-not $existing.isDraft) { gh pr ready $existing.number --undo | Out-Null; if ($LASTEXITCODE -ne 0) { Write-Error "PR #$($existing.number) could not be marked draft; it was left unchanged."; exit 1 } }
    gh pr edit $existing.number --body $body | Out-Null; if ($LASTEXITCODE -ne 0) { exit 1 }
}
# Opened as a draft; marked ready only once it is complete.
pwsh -NoProfile -File (Join-Path $PSScriptRoot 'pr-ready.ps1')
if ($LASTEXITCODE -ne 0) { Write-Error 'the PR stays a draft: it is not complete (see above).'; exit 1 }
exit 0