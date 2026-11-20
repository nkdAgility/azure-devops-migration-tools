# Fresh full build, full test suite, then the repository's own real-system proof, for a change to the product.
# The repository names its commands in .specify/memory/done-checks.json:
#   { "build": "dotnet build", "test": "dotnet test", "proof": "pwsh ./proof.ps1" }
# Product code is `productPaths` in .specify/memory/workflow.json (default src/). A change that touches none of it
# (process or docs files only) has no product to build, test or prove: those steps are skipped, and the change's
# own scripts are checked instead (every .ps1 and .psm1 it changes must parse).
param([string]$Base = 'origin/main')
$ErrorActionPreference = 'Stop'
$cfgPath = '.specify/memory/done-checks.json'
if (-not (Test-Path $cfgPath)) { Write-Error "Missing ${cfgPath}: the repository must name its build, test and proof commands."; exit 1 }
$cfg = Get-Content $cfgPath -Raw | ConvertFrom-Json

$product = @('src/')
$wf = '.specify/memory/workflow.json'
if (Test-Path $wf) { $w = Get-Content $wf -Raw | ConvertFrom-Json; if ($w.PSObject.Properties['productPaths'] -and $w.productPaths) { $product = @($w.productPaths) } }
$changed = foreach ($d in @(@('--name-only', "$Base...HEAD"), @('--name-only'), @('--name-only', '--cached'))) {
    $out = @(git diff @d)
    if ($LASTEXITCODE -ne 0) { Write-Error "git diff $($d -join ' ') failed; the changed files are not known."; exit 1 }
    $out
}
$changed = @(@($changed) + @(git ls-files --others --exclude-standard) | Where-Object { $_ } | Sort-Object -Unique)
$touchesProduct = $changed | Where-Object { $f = $_; $product | Where-Object { $p = $_.Replace('\', '/').TrimStart('/'); if ($p.EndsWith('/')) { $f.StartsWith($p) } else { $f -eq $p -or $f.StartsWith("$p/") } } }

if (-not $touchesProduct) {
    "== no product code changed ($($product -join ', ')): build, test and proof are not run"
    $bad = foreach ($f in $changed | Where-Object { $_ -match '\.psm?1$' -and (Test-Path $_) }) {
        $errs = $null; [void][Management.Automation.Language.Parser]::ParseFile((Resolve-Path $f).Path, [ref]$null, [ref]$errs)
        if ($errs) { "${f}: $($errs[0].Message)" }
    }
    if ($bad) { $bad | ForEach-Object { Write-Error "does not parse: $_" -ErrorAction Continue }; exit 1 }
    "== changed scripts parse"
    exit 0
}

foreach ($k in 'build','test','proof') {
    if (-not $cfg.$k) { Write-Error "done-checks.json has no '$k' command."; exit 1 }
    "== ${k}: $($cfg.$k)"
    pwsh -NoProfile -Command $cfg.$k
    if ($LASTEXITCODE -ne 0) { Write-Error "$k failed."; exit 1 }
}
exit 0
