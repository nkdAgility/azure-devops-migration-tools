# Resolves the item reference (#123) from the run's text and records which item this run is for.
# The manager has already pulled the item into Specifying; intake does not change its state.
# The workflow passes the request as -Text '<text>'. On Windows the shell does not honour those single quotes, so text
# with spaces arrives as several arguments: the first binds to -Text and the rest are joined back on.
param([Parameter(Mandatory)][string]$Text, [Parameter(ValueFromRemainingArguments)][string[]]$Rest)
$ErrorActionPreference = 'Stop'
$Text = (@($Text) + @($Rest) -join ' ').Trim().Trim("'").Trim()
if ($Text -notmatch '#(\d+)') { Write-Error 'No item reference (#123) in the request. Create the issue first.'; exit 1 }
$id = $Matches[1]
New-Item -ItemType Directory -Force .specify/items, .specify/wkr | Out-Null
@{ number = [int]$id } | ConvertTo-Json | Set-Content .specify/items/current.json -Encoding utf8
$wkr = ".specify/wkr/$id.md"
if (-not (Test-Path $wkr)) { "# WKR-$id`n" | Set-Content $wkr -Encoding utf8 }
$run = Get-ChildItem .specify/workflows/runs -Directory -ErrorAction SilentlyContinue | Sort-Object LastWriteTime | Select-Object -Last 1
if ($run -and -not (Select-String $wkr -Pattern '^Run:' -Quiet)) { Add-Content $wkr "Run: $($run.Name)" }
"item #$id"
exit 0