# Resolves the item reference (#123) from the run's text and records which item this run is for.
# The manager has already pulled the item into Specifying; intake does not change its state.
param([Parameter(Mandatory)][string]$Text)
$ErrorActionPreference = 'Stop'
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