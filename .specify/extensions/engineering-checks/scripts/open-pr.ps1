# Opens a PR naming the item and attaches the completion report. Idempotent. State is moved by the policy, not here.
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$title = (gh issue view $id --json title | ConvertFrom-Json).title
$body = "Closes #$id`n`n"
if (Test-Path .specify/reviews/completion-report.md) { $body += Get-Content .specify/reviews/completion-report.md -Raw }
$existing = gh pr view --json number 2>$null | ConvertFrom-Json
if (-not $existing) { gh pr create --title $title --body $body; if ($LASTEXITCODE -ne 0) { exit 1 } }
exit 0