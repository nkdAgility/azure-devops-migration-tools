# After the done gate: hands the PR to the merge queue. Idempotent.
$ErrorActionPreference = 'Stop'
gh pr merge --auto --squash
exit $LASTEXITCODE