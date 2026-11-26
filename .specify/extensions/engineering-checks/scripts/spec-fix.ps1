# A small fix to the spec Martin approved keeps his approval: his own answers written in, or a typing mistake
# corrected. Nothing that changes what will be built. Records "Spec fix: <path> sha256 <old> -> <new>: <reason>" on
# the item, after checking that his approval stands for the version before the fix and that the fix is small: at most
# 12 changed lines and 2000 changed characters, measured against that version as committed in git.
# Usage: spec-fix.ps1 -Reason "wrote in Martin's answers to Q-5 and Q-7"
param([Parameter(Mandatory)][string]$Reason, [int]$MaxLines = 12, [int]$MaxChars = 2000)
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$rec = Get-ItemRecordPath '.' $id
$text = (Get-Content $rec -Raw) -replace "`r`n", "`n"
if ($Reason.Trim().Length -lt 8) { Write-Error 'Say what was fixed.'; exit 1 }
$agreed = [regex]::Matches($text, '(?m)^Spec agreed:') | Select-Object -Last 1
if (-not $agreed) { Write-Error "#$id has no approval to keep: get Martin's approval of the spec."; exit 1 }
$ver = [regex]::Matches($text, '(?m)^Spec version:[ \t]*(?<p>\S+)[ \t]+sha256[ \t]+(?<h>[0-9a-f]{64})[ \t]*$') | Where-Object { $_.Index -gt $agreed.Index } | Select-Object -First 1
if (-not $ver) { Write-Error "#$id's approval is not bound to a version yet (spec-agreed.ps1)."; exit 1 }
$file = Resolve-SpecPath '.' $ver.Groups['p'].Value
if (-not $file) { Write-Error "The approved spec $($ver.Groups['p'].Value) is missing."; exit 1 }
$rel = [IO.Path]::GetRelativePath((Get-Location), $file).Replace('\', '/')
$new = Get-TextHash $file
# The versions the approval already covers: the bound one and every fix recorded since.
$accepted = [Collections.Generic.List[string]]::new(); $accepted.Add($ver.Groups['h'].Value)
foreach ($fx in [regex]::Matches($text, '(?m)^Spec fix:[ \t]*\S+[ \t]+sha256[ \t]+(?<o>[0-9a-f]{64})[ \t]*->[ \t]*(?<n>[0-9a-f]{64}):') | Where-Object { $_.Index -gt $agreed.Index }) { if ($accepted.Contains($fx.Groups['o'].Value)) { $accepted.Add($fx.Groups['n'].Value) } }
if ($accepted.Contains($new)) { "the spec is a version Martin's approval already covers; nothing to record"; exit 0 }
$from = $accepted[-1]
# The text of that version, from git: any commit of this file, or of the path it was approved at, with that fingerprint.
$old = $null
foreach ($p in @($rel, $ver.Groups['p'].Value) | Select-Object -Unique) {
    foreach ($c in @(git rev-list --all -- $p 2>$null)) {
        $blob = git show "${c}:$p" 2>$null
        if ($LASTEXITCODE -ne 0) { continue }
        $t = (($blob -join "`n") + "`n")
        foreach ($cand in @($t, $t.TrimEnd("`n"))) {
            $h = ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($cand)))).ToLowerInvariant()
            if ($accepted.Contains($h)) { $old = $cand; $from = $h; break }
        }
        if ($old) { break }
    }
    if ($old) { break }
}
# In "artifacts" mode the approved text is in the version history of the spec's artifact on the issue.
if (-not $old -and (Get-ItemDocsMode '.') -eq 'artifacts') {
    $folder = Get-ItemFolder '.' $id
    $inFolder = if ($folder) { [IO.Path]::GetRelativePath($folder, $file).Replace('\', '/') } else { Split-Path $file -Leaf }
    foreach ($body in @(Get-ItemArtifactVersions '.' $id $inFolder)) {
        $t = ($body -replace "`r`n", "`n").TrimEnd("`n")
        foreach ($cand in @("$t`n", $t)) {
            $h = ([Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($cand)))).ToLowerInvariant()
            if ($accepted.Contains($h)) { $old = $cand; $from = $h; break }
        }
        if ($old) { break }
    }
}
if (-not $old) { Write-Error "The approved text of $rel is not in git or in its artifact's history, so the fix cannot be measured. Commit the approved version before fixing it, or get Martin's approval of the new version."; exit 1 }
$a = @($old -split "`n"); $b = @(((Get-Content $file -Raw) -replace "`r`n", "`n") -split "`n")
$diff = @(Compare-Object $a $b)
$chars = ($diff | ForEach-Object { "$($_.InputObject)".Length } | Measure-Object -Sum).Sum
if ($diff.Count -gt $MaxLines -or $chars -gt $MaxChars) { Write-Error "This is not a small fix: $($diff.Count) changed lines, $chars characters (at most $MaxLines and $MaxChars). Present the spec to Martin again."; exit 1 }
Add-Content $rec "Spec fix: $rel sha256 $from -> ${new}: $($Reason.Trim())"
"small fix recorded ($($diff.Count) lines, $chars characters); Martin's approval stands"
exit 0
