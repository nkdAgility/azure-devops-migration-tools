# After the done gates: hands the PR to the merge queue, but only when every recorded gate confidence meets the
# repo's merge threshold (mergeMinConfidence in .specify/memory/workflow.json; default minConfidence, then 85).
# Below it, auto-merge is turned off and the step fails, so the PR waits for Martin. Idempotent.
$ErrorActionPreference = 'Stop'
$min = 85
if (Test-Path .specify/memory/workflow.json) {
    $c = Get-Content .specify/memory/workflow.json -Raw | ConvertFrom-Json
    if ($c.PSObject.Properties['minConfidence'] -and $c.minConfidence) { $min = [int]$c.minConfidence }
    if ($c.PSObject.Properties['mergeMinConfidence'] -and $c.mergeMinConfidence) { $min = [int]$c.mergeMinConfidence }
}
$files = @(Get-ChildItem .specify/reviews -Filter '*-confidence.md' -ErrorAction SilentlyContinue)
if (-not $files) { gh pr merge --disable-auto 2>$null; Write-Error "no gate confidence is recorded in .specify/reviews/; auto-merge is off."; exit 1 }
foreach ($f in $files) {
    $m = [regex]::Match((Get-Content $f.FullName -Raw), '(?im)^Confidence:\s*(\d{1,3})\s*%')
    if (-not $m.Success) { gh pr merge --disable-auto 2>$null; Write-Error "$($f.Name) has no 'Confidence: N%' line; auto-merge is off."; exit 1 }
    if ([int]$m.Groups[1].Value -lt $min) { gh pr merge --disable-auto 2>$null; Write-Error "$($f.Name): confidence $($m.Groups[1].Value)% is below this repo's merge threshold of $min%; auto-merge is off."; exit 1 }
}
gh pr merge --auto --squash
exit $LASTEXITCODE