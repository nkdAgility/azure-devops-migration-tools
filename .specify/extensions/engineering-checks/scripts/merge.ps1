# After the done gates: hands the PR to the merge queue, but only when every recorded gate confidence meets the
# repo's merge threshold (mergeMinConfidence in .specify/memory/workflow.json; default minConfidence, then 85),
# the automated reviewers have reviewed the latest push (or 10 minutes have passed since it with no review),
# and no review thread is still open. Below the threshold, auto-merge is turned off and the step fails, so the PR
# waits for Martin. The merge method is the repo's mergeMethod (squash, the default; merge; or rebase). Idempotent.
param([int]$ReviewWaitMinutes = 10, [int]$PollSeconds = 30)
$ErrorActionPreference = 'Stop'
$min = 85; $method = 'squash'
if (Test-Path .specify/memory/workflow.json) {
    $c = Get-Content .specify/memory/workflow.json -Raw | ConvertFrom-Json
    if ($c.PSObject.Properties['minConfidence'] -and $c.minConfidence) { $min = [int]$c.minConfidence }
    if ($c.PSObject.Properties['mergeMinConfidence'] -and $c.mergeMinConfidence) { $min = [int]$c.mergeMinConfidence }
    if ($c.PSObject.Properties['mergeMethod'] -and $c.mergeMethod) { $method = "$($c.mergeMethod)".ToLower() }
}
if ($method -notin 'squash', 'merge', 'rebase') { Write-Error "mergeMethod '$method' is not squash, merge or rebase."; exit 1 }
$files = @(Get-ChildItem .specify/reviews -Filter '*-confidence.md' -ErrorAction SilentlyContinue)
if (-not $files) { gh pr merge --disable-auto 2>$null; Write-Error "no gate confidence is recorded in .specify/reviews/; auto-merge is off."; exit 1 }
foreach ($f in $files) {
    $m = [regex]::Match((Get-Content $f.FullName -Raw), '(?im)^Confidence:\s*(\d{1,3})\s*%')
    if (-not $m.Success) { gh pr merge --disable-auto 2>$null; Write-Error "$($f.Name) has no 'Confidence: N%' line; auto-merge is off."; exit 1 }
    if ([int]$m.Groups[1].Value -lt $min) { gh pr merge --disable-auto 2>$null; Write-Error "$($f.Name): confidence $($m.Groups[1].Value)% is below this repo's merge threshold of $min%; auto-merge is off."; exit 1 }
}

# Wait for the automated reviewers: a review by a bot after the latest push, or ReviewWaitMinutes since that push.
$pr = gh pr view --json number,url,commits | ConvertFrom-Json
$pushed = [datetime](@($pr.commits)[-1].committedDate)
while ($true) {
    $reviews = @((gh pr view --json reviews | ConvertFrom-Json).reviews)
    $botReviewed = @($reviews | Where-Object { "$($_.author.login)" -match '\[bot\]$|^copilot' -and [datetime]$_.submittedAt -ge $pushed }).Count -gt 0
    if ($botReviewed -or ([datetime]::UtcNow - $pushed.ToUniversalTime()).TotalMinutes -ge $ReviewWaitMinutes) { break }
    Start-Sleep -Seconds $PollSeconds
}

# Every finding fixed: no review thread left unresolved.
$owner, $name = (gh repo view --json nameWithOwner -q .nameWithOwner) -split '/'
$q = 'query($o:String!,$n:String!,$p:Int!){repository(owner:$o,name:$n){pullRequest(number:$p){reviewThreads(first:100){nodes{isResolved path}}}}}'
$threads = @((gh api graphql -f query=$q -f o=$owner -f n=$name -F p=$pr.number | ConvertFrom-Json).data.repository.pullRequest.reviewThreads.nodes)
$open = @($threads | Where-Object { -not $_.isResolved })
if ($open.Count) { gh pr merge --disable-auto 2>$null; Write-Error "$($open.Count) review thread(s) still open ($((@($open | ForEach-Object path) | Select-Object -Unique) -join ', ')): fix each finding, reply with the commit and resolve it; auto-merge is off."; exit 1 }

gh pr merge --auto "--$method"
exit $LASTEXITCODE
