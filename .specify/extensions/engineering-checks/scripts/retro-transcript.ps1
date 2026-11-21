# The retrospective's evidence: this worker's own chat, rendered as readable turns for the blind reviewer.
# Finds the transcript the agent keeps for this worktree (in a dot folder under the home directory:
# <agent folder>/projects/<worktree path, mangled>/<session>.jsonl), newest first, and writes
# .specify/reviews/retro-transcript.md: each user turn, each reply, and each tool call with the start of its result.
# -Session picks a transcript by id; -Path names one directly.
param([string]$Session, [string]$Path, [int]$ResultChars = 600)
$ErrorActionPreference = 'Stop'
$root = (git rev-parse --show-toplevel).Trim()
if (-not $Path) {
    $mangled = ([IO.Path]::GetFullPath($root)) -replace '[:\\/.]', '-'
    $dirs = @(Get-ChildItem $HOME -Directory -Force -Filter '.*' -ErrorAction SilentlyContinue | ForEach-Object { Join-Path $_.FullName "projects/$mangled" } | Where-Object { Test-Path $_ })
    if (-not $dirs) { Write-Error "No transcripts found for this worktree ($mangled)."; exit 1 }
    $file = if ($Session) { $dirs | ForEach-Object { Join-Path $_ "$Session.jsonl" } | Where-Object { Test-Path $_ } | Select-Object -First 1 }
            else { (Get-ChildItem $dirs -Filter *.jsonl | Sort-Object LastWriteTime | Select-Object -Last 1).FullName }
} else { $file = $Path }
if (-not $file -or -not (Test-Path $file)) { Write-Error "Transcript not found: $file"; exit 1 }

function Text($c) { if ($c -is [string]) { $c } else { (@($c) | Where-Object { $_.type -eq 'text' } | ForEach-Object text) -join "`n" } }
$out = [Collections.Generic.List[string]]::new()
$out.Add("# Transcript: $(Split-Path $file -Leaf)`n")
foreach ($line in Get-Content $file) {
    try { $e = $line | ConvertFrom-Json } catch { continue }
    if ($e.type -notin 'user', 'assistant' -or -not $e.message) { continue }
    $at = if ($e.timestamp) { " ($($e.timestamp))" } else { '' }
    foreach ($c in @($e.message.content)) {
        if ($c -is [string] -or $c.type -eq 'text') {
            $t = (Text $c).Trim(); if (-not $t) { continue }
            $out.Add("## $(if ($e.type -eq 'user') { 'User' } else { 'Worker' })$at`n$t`n")
        } elseif ($c.type -eq 'tool_use') {
            $arg = ($c.input | ConvertTo-Json -Compress -Depth 4); if ($arg.Length -gt 800) { $arg = $arg.Substring(0, 800) + ' ...' }
            $out.Add("### Tool call: $($c.name)$at`n``````n$arg`n``````n")
        } elseif ($c.type -eq 'tool_result') {
            $r = (Text $c.content).Trim(); if ($r.Length -gt $ResultChars) { $r = $r.Substring(0, $ResultChars) + ' ...' }
            $out.Add("### Tool result`n``````n$r`n``````n")
        }
    }
}
New-Item -ItemType Directory -Force (Join-Path $root '.specify/reviews') | Out-Null
$dest = Join-Path $root '.specify/reviews/retro-transcript.md'
$out -join "`n" | Set-Content $dest -Encoding utf8
"transcript written: $dest ($($out.Count) entries)"
exit 0
