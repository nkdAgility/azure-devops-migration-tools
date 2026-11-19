# Output Done, docs reconciled: every system the change touches has its docs/specs/{system}/ updated, or is
# marked unchanged with a reason ("Docs unchanged: <system>: <reason>" on the item); no discrepancy is left
# open; and the working spec folder is deleted in the same change. Fails the step otherwise.
param([string]$Base = 'main')
$ErrorActionPreference = 'Stop'
$id = (Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number
$wkr = ".specify/wkr/$id.md"
$wkrText = if (Test-Path $wkr) { Get-Content $wkr -Raw } else { '' }
$failures = [Collections.Generic.List[string]]::new()
$changed = @(git diff --name-only "$Base...HEAD") + @(git diff --name-only) + @(git diff --name-only --cached) | Where-Object { $_ } | Sort-Object -Unique

if (-not (Test-Path docs/specs/systems.md)) { $failures.Add('docs/specs/systems.md is missing: the repo has not listed its systems') }
else {
    $systems = [regex]::Matches((Get-Content docs/specs/systems.md -Raw), '(?m)^\s*-\s*([^:]+):\s*(.+)$') | ForEach-Object {
        @{ name = $_.Groups[1].Value.Trim(); paths = @($_.Groups[2].Value -split '[,\s]+' | Where-Object { $_ } | ForEach-Object { $_.Trim('`') }) }
    }
    foreach ($s in $systems) {
        $touched = $changed | Where-Object { $f = $_; ($s.paths | Where-Object { $f -like "$($_.TrimEnd('/'))*" }) -and $f -notlike 'docs/*' }
        if (-not $touched) { continue }
        $docsChanged = $changed | Where-Object { $_ -like "docs/specs/$($s.name)/*" }
        $waived = [regex]::IsMatch($wkrText, "(?m)^Docs unchanged:\s*$([regex]::Escape($s.name))\s*:\s*\S")
        if (-not $docsChanged -and -not $waived) { $failures.Add("system '$($s.name)' changed but docs/specs/$($s.name)/ was not updated, and no 'Docs unchanged: $($s.name): <reason>' line is on the item") }
    }
}
foreach ($d in @(Get-ChildItem specs -Recurse -Filter discrepancies.md -ErrorAction SilentlyContinue)) {
    $open = Select-String $d.FullName -Pattern '(?i)\b(open|unresolved)\b' -SimpleMatch:$false
    if ($open) { $failures.Add("$($d.FullName): discrepancies are still open") }
}
$specDirs = @(Get-ChildItem specs -Directory -ErrorAction SilentlyContinue)
if ($specDirs) { $failures.Add("the working spec is still in specs/ ($($specDirs.Name -join ', ')); delete it once reconciled into docs/specs/") }

if ($failures.Count) { $failures | ForEach-Object { Write-Error $_ -ErrorAction Continue }; exit 1 }
"docs reconciled"
exit 0