# Output Done, docs reconciled: every system the change touches has its {specRoot}/{system}/ (default docs/specs/)
# updated to describe what this item added (named on the item under "## Docs updated", with a phrase the doc must
# contain), or is marked unchanged with a reason that was in the spec Martin approved ("Docs unchanged: <system>:
# <reason>"); and no discrepancy is left open. The item's folder is removed later, before the PR is marked ready.
param([string]$Base = 'main')
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
Import-Module (Join-Path $PSScriptRoot '..\workflow\Workflow.psm1') -Force
$wkr = Get-ItemRecordPath '.' $id
$wkrText = if (Test-Path $wkr) { Get-Content $wkr -Raw } else { '' }
# The spec Martin approved (a feature spec or a bug assessment), when his approval still stands for it.
$approvedSpec = ''
$normRecord = $wkrText -replace "`r`n", "`n"
if (-not (Get-SpecApprovalProblem $normRecord '.')) {
    $ver = [regex]::Matches($normRecord, '(?m)^Spec version:[ \t]*(?<p>\S+)[ \t]+sha256') | Select-Object -Last 1
    $specFile = if ($ver) { Resolve-SpecPath '.' $ver.Groups['p'].Value } else { $null }
    if ($specFile) { $approvedSpec = (Get-Content $specFile -Raw) -replace "`r`n", "`n" }
}
$failures = [Collections.Generic.List[string]]::new()
$root = 'docs/specs'
$cfgPath = '.specify/memory/workflow.json'
if (Test-Path $cfgPath) { $c = Get-Content $cfgPath -Raw | ConvertFrom-Json; if ($c.PSObject.Properties['specRoot'] -and $c.specRoot) { $root = $c.specRoot.TrimEnd('/') } }
$changed = foreach ($d in @(@('--name-only', "$Base...HEAD"), @('--name-only'), @('--name-only', '--cached'))) {
    $out = @(git diff @d)
    if ($LASTEXITCODE -ne 0) { Write-Error "git diff $($d -join ' ') failed; the changed files are not known."; exit 1 }
    $out
}
$changed = @($changed | Where-Object { $_ } | Sort-Object -Unique)

if (-not (Test-Path "$root/systems.md")) { $failures.Add("$root/systems.md is missing: the repo has not listed its systems") }
else {
    $systems = [regex]::Matches((Get-Content "$root/systems.md" -Raw), '(?m)^\s*-\s*([^:]+):\s*(.+)$') | ForEach-Object {
        @{ name = $_.Groups[1].Value.Trim(); paths = @($_.Groups[2].Value -split '[,\s]+' | Where-Object { $_ } | ForEach-Object { $_.Trim('`') }) }
    }
    foreach ($s in $systems) {
        $touched = $changed | Where-Object { $f = $_; ($s.paths | Where-Object { $p = $_; if ($p.EndsWith('/')) { $f -like "$p*" } else { $f -eq $p -or $f -like "$p/*" } }) -and $f -notlike "$root/*" }
        if (-not $touched) { continue }
        # A waiver counts only if it was in the spec Martin approved: either in the approved spec itself (the file his
        # approval is bound to, while that approval stands), or on the record before his last "Spec agreed:".
        $waiverPattern = "(?m)^\s*(?:-\s*)?Docs unchanged:\s*$([regex]::Escape($s.name))\s*:\s*\S"
        if ($approvedSpec -and [regex]::IsMatch($approvedSpec, $waiverPattern)) { continue }
        $waiver = [regex]::Matches($wkrText, $waiverPattern) | Select-Object -Last 1
        $agreed = [regex]::Matches($wkrText, '(?m)^Spec agreed:') | Select-Object -Last 1
        if ($waiver -and $agreed -and $waiver.Index -lt $agreed.Index) { continue }
        if ($waiver) { $failures.Add("the 'Docs unchanged: $($s.name)' reason was added after Martin agreed the spec; it must be in the spec he approves"); continue }
        # The docs must say what this item added: a line under "## Docs updated" names the doc and a phrase from it,
        # "- <system>: ``<doc path>``: <phrase>"; the doc must be changed in this change and contain the phrase.
        $lines = [regex]::Matches("$(Get-Section ($wkrText -replace "`r`n", "`n") 'Docs updated')", "(?m)^\s*-\s*$([regex]::Escape($s.name))\s*:\s*``(?<p>[^``]+)``\s*:\s*(?<ph>\S.*?)\s*$")
        $ok = $false
        foreach ($l in $lines) {
            $p = $l.Groups['p'].Value.Replace('\', '/').TrimStart('/')
            if ($p -notlike "$root/$($s.name)/*") { $failures.Add("'Docs updated' for $($s.name) names $p, which is not under $root/$($s.name)/"); continue }
            if ($changed -notcontains $p) { $failures.Add("'Docs updated' for $($s.name) names $p, which this change does not change"); continue }
            if (-not (Test-Path $p) -or -not ((Get-Content $p -Raw) -match [regex]::Escape($l.Groups['ph'].Value))) { $failures.Add("$p does not contain ""$($l.Groups['ph'].Value)"", the phrase 'Docs updated' names for $($s.name)"); continue }
            $ok = $true
        }
        if (-not $ok -and -not $lines.Count) { $failures.Add("system '$($s.name)' changed: update $root/$($s.name)/ to describe what this item added, and name it on the item under '## Docs updated' as '- $($s.name): ``<doc path>``: <phrase from the doc>' (or put 'Docs unchanged: $($s.name): <reason>' in the spec Martin approves)") }
    }
}
foreach ($d in @(Get-ChildItem specs -Recurse -Filter discrepancies.md -ErrorAction SilentlyContinue)) {
    $open = Select-String $d.FullName -Pattern '(?i)\b(open|unresolved)\b' -SimpleMatch:$false
    if ($open) { $failures.Add("$($d.FullName): discrepancies are still open") }
}
# The item's folder (specs/<issue>-<name>/) is kept until the outcome checks have passed; pr-ready.ps1 requires it
# deleted, once reconciled into $root/, before the pull request is marked ready.

if ($failures.Count) { $failures | ForEach-Object { Write-Error $_ -ErrorAction Continue }; exit 1 }
"docs reconciled"
exit 0