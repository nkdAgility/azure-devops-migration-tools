# Adoption: imports a backlog kept in files as issues on the board. Each Markdown file under -Path is one item:
# its front matter gives `title` (default: the first heading, then the file name) and `type` (Product Backlog Item,
# Bug or Feedback; default Product Backlog Item); the rest is its body.
# Without -Apply it is a dry run: it lists each item with its type and state and writes the list to
# .specify/adopt/backlog.json for Martin to approve. With -Apply it creates each listed item as an issue of its type
# in the entry state (Proposed), linked to its source file, but only if the files have not changed since the dry run.
param([Parameter(Mandatory)][string]$Path, [switch]$Apply)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$types = 'Product Backlog Item', 'Bug', 'Feedback'
$cfg = Get-WorkflowConfig
$entry = Get-EntryState $cfg
$list = foreach ($f in Get-ChildItem $Path -Recurse -File -Filter *.md | Sort-Object FullName) {
    $text = Get-Content $f.FullName -Raw
    $fm = Get-FrontMatter $text
    $body = ($text -replace '(?s)\A---\r?\n.*?\r?\n---\r?\n', '').Trim()
    $title = if ($fm -and $fm['title']) { "$($fm['title'])" } elseif ($body -match '(?m)^#\s+(.+)$') { $Matches[1].Trim() } else { $f.BaseName }
    $type = if ($fm -and $fm['type']) { "$($fm['type'])" } else { 'Product Backlog Item' }
    $rel = (Resolve-Path -Relative $f.FullName).Replace('\', '/').TrimStart('.', '/')
    [ordered]@{ source = $rel; title = $title; type = $type; state = $entry; hash = (Get-FileHash $f.FullName).Hash; problem = $(if ($type -notin $types) { "type '$type' is not one of: $($types -join ', ')" }) }
}
$list = @($list)
$plan = '.specify/adopt/backlog.json'
if (-not $Apply) {
    New-Item -ItemType Directory -Force (Split-Path $plan) | Out-Null
    $list | ConvertTo-Json -Depth 4 -AsArray | Set-Content $plan -Encoding utf8
    foreach ($i in $list) { "{0} | {1} | {2} | {3}{4}" -f $i.source, $i.type, $i.state, $i.title, $(if ($i.problem) { "  <- $($i.problem)" }) }
    "$($list.Count) item(s). Show Martin this list; once he approves it, run again with -Apply."
    if (@($list | Where-Object problem).Count) { exit 1 }
    exit 0
}
if (-not (Test-Path $plan)) { Write-Error "no dry run: run without -Apply first and have Martin approve the list."; exit 1 }
$approved = @(Get-Content $plan -Raw | ConvertFrom-Json)
$now = ($list | ForEach-Object { "$($_.source)|$($_.hash)|$($_.type)|$($_.title)" }) -join "`n"
$then = ($approved | ForEach-Object { "$($_.source)|$($_.hash)|$($_.type)|$($_.title)" }) -join "`n"
if ($now -ne $then) { Write-Error "the files have changed since the dry run; run the dry run again and have Martin approve the new list."; exit 1 }
if (@($list | Where-Object problem).Count) { Write-Error "the list still has problems; fix them and run the dry run again."; exit 1 }
$nwo = if ($cfg.board['repo']) { $cfg.board.repo } else { (gh repo view --json nameWithOwner | ConvertFrom-Json).nameWithOwner }
$ad = Get-BoardAdapter $cfg
foreach ($i in $list) {
    $body = (Get-Content $i.source -Raw) -replace '(?s)\A---\r?\n.*?\r?\n---\r?\n', ''
    $body = "$($body.Trim())`n`nImported from: ``$($i.source)``"
    $n = gh api "repos/$nwo/issues" -f title="$($i.title)" -f body="$body" -f type="$($i.type)" --jq .number
    if ($LASTEXITCODE -ne 0 -or -not $n) { Write-Error "creating the issue for $($i.source) failed."; exit 1 }
    & $ad.SetState (& $ad.GetItem $n) $i.state
    "#$n $($i.type): $($i.title) ($($i.source))"
}
exit 0
