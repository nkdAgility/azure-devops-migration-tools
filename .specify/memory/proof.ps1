# The real-system proof for Output Done, on the test organisations (tools/e2e; see "Validating Changes End to
# End" in platform/specs/architecture.md). Needs `az login` (or AMT_TEST_PAT) for the test orgs.
#
# Every scenario this change needs (see below; -All for every one) that has a seed, a config and an assert is run in scratch projects of its own, never in the
# projects the automated tests use: seed the scratch projects, run the migration with the config re-pointed at
# them, then run the scenario's assertions. Fails if any seed, migration or assertion fails.
# Scenarios without an assert are listed as not covered. Projects come from the scratch project pool (#3342): each
# scenario's projects are given back (cleared and renamed back) before the next scenario and in a finally, and the
# pool slots used are listed at the end. Scratch processes are listed and not deleted here: Remove-E2EObjects.ps1 with
# a per-run prefix also deletes other scenarios' fields (#3278).
param([string]$RunId = (Get-Date -Format 'MMddHHmm'), [string]$Item, [switch]$All, [string]$Base = 'origin/main',
    # Run only these scenarios, whatever the change touches (used by tools/e2e/Test-E2EPool.ps1 -Case Scenario).
    [string[]]$Only)
$ErrorActionPreference = 'Stop'
# Every object a run creates is named amte2e-<item>-p<run>-..., so each worker can find and remove its own.
if (-not $Item -and (Test-Path .specify/items/current.json)) { $Item = "$((Get-Content .specify/items/current.json -Raw | ConvertFrom-Json).number)" }
if (-not $Item) { Write-Error 'no item: pass -Item <issue number>, or run from an item worktree'; exit 1 }

# Which scenarios this change needs: the smoke set, every scenario whose folder the change touches, and every
# scenario whose config names a processor, tool or field map that a changed file is named after (for example
# TfsWorkItemMigrationProcessor.cs). A changed product file that no scenario names, or a change to the harness
# itself (tools/e2e outside scenarios/), runs them all, since nothing says which it affects. -All runs them all.
$smoke = @('wicore')
$changed = @(git diff --name-only "$Base...HEAD"; git diff --name-only; git diff --name-only --cached) | Where-Object { $_ } | Sort-Object -Unique
if ($LASTEXITCODE -ne 0) { Write-Error "git diff against $Base failed; the change is not known"; exit 1 }
$wf = Get-Content .specify/memory/workflow.json -Raw | ConvertFrom-Json
$productPaths = @(if ($wf.PSObject.Properties['productPaths']) { $wf.productPaths } else { 'src/' }) | ForEach-Object { $_.Replace('\', '/').TrimStart('/') }
$product = @($changed | Where-Object { $p = $_; $productPaths | Where-Object { $p.StartsWith($_.TrimEnd('/') + '/', [StringComparison]::OrdinalIgnoreCase) -or $p -ieq $_.TrimEnd('/') } })
$namesOf = @{}
foreach ($d in Get-ChildItem tools/e2e/scenarios -Directory) {
    $txt = (Get-ChildItem $d.FullName -Filter *.json | ForEach-Object { Get-Content $_.FullName -Raw }) -join "`n"
    $namesOf[$d.Name] = @([regex]::Matches($txt, '"(\w+(Processor|Tool|Map|FieldMap|Enricher))"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
}
# Where each named processor or tool lives: the folders of the source files named after it.
$where = @{}
foreach ($src in (git ls-files 'src/*.cs')) { $n = [IO.Path]::GetFileNameWithoutExtension($src); if (-not $where[$n]) { $where[$n] = @() }; $where[$n] += (Split-Path $src -Parent).Replace('\', '/') }
$why = @{}
foreach ($n in $smoke) { $why[$n] = 'smoke' }
$runAll = [bool]$All
if ($All) { $reasonAll = '-All' }
foreach ($p in $product) {
    # Tests change no behaviour a migration shows: unit test projects and the harness's own Pester tests.
    if ($p -match '^src/[^/]*\.Tests/' -or $p -match '\.Tests\.ps1$') { continue }
    if ($p -match '^tools/e2e/scenarios/([^/]+)/') { $why[$Matches[1]] = "its folder changed ($p)"; continue }
    if ($p -like 'tools/e2e/*') { if ($p -match '\.(ps1|psm1|psd1|json)$') { $runAll = $true; $reasonAll = "the harness changed ($p)" }; continue }
    $stem = [IO.Path]::GetFileNameWithoutExtension($p)
    $hits = @($namesOf.Keys | Where-Object { $namesOf[$_] -contains $stem }); $how = "uses $stem"
    # Not named itself: the scenarios that use a processor or tool in the same folder, else in the same project.
    if (-not $hits) { $dir = (Split-Path $p -Parent).Replace('\', '/'); $hits = @($namesOf.Keys | Where-Object { @($namesOf[$_] | Where-Object { $where[$_] -contains $dir }).Count }); $how = "uses a processor or tool next to $stem" }
    if (-not $hits) { $proj = ($p -split '/')[0..1] -join '/'; $hits = @($namesOf.Keys | Where-Object { @($namesOf[$_] | Where-Object { @($where[$_] | Where-Object { $_ -eq $proj -or $_ -like "$proj/*" }).Count }).Count }); $how = "uses a processor or tool in $proj" }
    if (-not $hits) { $runAll = $true; $reasonAll = "$p is near no processor or tool a scenario uses"; continue }
    foreach ($h in $hits) { if (-not $why[$h]) { $why[$h] = $how } }
}
# Only scenarios with a seed, a config and an assert can run; the list says what will actually run.
$runnable = @(Get-ChildItem tools/e2e/scenarios -Directory | Where-Object { (Test-Path (Join-Path $_.FullName 'seed.ps1')) -and (Test-Path (Join-Path $_.FullName 'assert.ps1')) -and (Test-Path (Join-Path $_.FullName 'config.json')) } | ForEach-Object Name)
if (-not $runAll) { foreach ($k in @($why.Keys)) { if ($runnable -notcontains $k) { $why.Remove($k) } } }
if ($Only) { $runAll = $false; $why = @{}; foreach ($n in $Only) { $why[$n] = 'asked for (-Only)' } }
if ($runAll) { "== running all $($runnable.Count) runnable scenarios: $reasonAll" } else { "== running $($why.Count) scenario(s):"; $why.Keys | Sort-Object | ForEach-Object { "   $_ ($($why[$_]))" } }

# One proof at a time on the shared test orgs: concurrent runs collide there (VS800075, dropped connections).
# Waiting runs queue in the order they arrived and never give up: each takes a ticket, and the oldest ticket whose
# process is still alive goes next.
$queue = Join-Path ([IO.Path]::GetTempPath()) 'amte2e-proof-queue'
New-Item -ItemType Directory -Force $queue | Out-Null
$ticket = Join-Path $queue ('{0:D19}-{1}.ticket' -f [DateTime]::UtcNow.Ticks, $PID)
"#$Item run $RunId" | Set-Content $ticket
$lockPath = Join-Path ([IO.Path]::GetTempPath()) 'amte2e-proof.lock'
$lock = $null; $waited = 0
try {
    while (-not $lock) {
        $live = @(Get-ChildItem $queue -Filter *.ticket | Sort-Object Name | Where-Object {
            $tp = [int](($_.BaseName -split '-')[-1]); if (Get-Process -Id $tp -ErrorAction SilentlyContinue) { $true } else { Remove-Item $_.FullName -ErrorAction SilentlyContinue; $false } })
        if ($live.Count -and $live[0].FullName -eq $ticket) {
            try { $lock = [IO.File]::Open($lockPath, 'OpenOrCreate', 'ReadWrite', 'None') } catch [IO.IOException] { }
        }
        if (-not $lock) {
            if ($waited % 300 -eq 0) { "queued for the test orgs: position $([array]::IndexOf(@($live.FullName), $ticket) + 1) of $($live.Count), waited $([int]($waited / 60)) min" }
            Start-Sleep 10; $waited += 10
        }
    }
} finally { if (-not $lock) { Remove-Item $ticket -ErrorAction SilentlyContinue } }
try {
$failed = @(); $notCovered = @(); $scratch = @(); $processes = @(); $used = @(); $released = @(); $notReleased = @()
# Projects come from the scratch project pool (#3342). Every seed this run starts takes them as this proof, so the
# proof can give back everything the run holds, after each scenario and again in the finally below.
$poolRun = "proof-$Item-$RunId"
$env:AMTE2E_POOL_OWNER = "${PID}:$poolRun"
$env:AMTE2E_POOL_ITEM = $Item
Import-Module (Join-Path $PWD 'tools/e2e/E2EAzDo.psm1') -Force -WarningAction SilentlyContinue
$poolOrgs = @('nkdagility-adomt-source', 'nkdagility-adomt-target')
# Renews this run's leases before each long step (migration, assertions), so they do not expire mid-scenario.
$renewPool = { foreach ($org in $poolOrgs) { foreach ($h in @(Get-E2EPoolProject -Org $org -Owner $poolRun)) { Set-E2EPoolLease $h.Id $org -Renew } } }
$releasePool = {
    # wicore-fresh's brand-new target project is deleted, the one project a run makes in the target org.
    $freshProjects = try { @(Get-E2EProjects -Org 'nkdagility-adomt-target' -Prefix "amte2e-$Item-p$RunId-" | Where-Object { "$($_.name)" -match '-fresh$' }) } catch { $script:notReleased += "NOT LISTED fresh projects: $($_.Exception.Message)"; @() }
    foreach ($f in $freshProjects) {
        try { Remove-E2EDisposableProject $f.name -Org 'nkdagility-adomt-target'; "deleted fresh project nkdagility-adomt-target/$($f.name)" }
        catch { $script:notReleased += "NOT DELETED fresh project nkdagility-adomt-target/$($f.name): $($_.Exception.Message)" }
    }
    foreach ($org in $poolOrgs) {
        $held = @(Get-E2EPoolProject -Org $org -Owner $poolRun)
        foreach ($h in $held) { $script:used += "$org/$(Format-E2EPoolName $h.Slot)" }
        foreach ($line in @(Clear-E2EPool -Org $org -Owner $poolRun)) {
            $line
            if ($line -match '^released \S+ -> (\S+)') { $script:released += "$org/$($Matches[1])" } elseif ($line -like 'NOT RELEASED*') { $script:notReleased += $line }
        }
    }
}
try {
$work = Join-Path ([IO.Path]::GetTempPath()) "amtproof-$Item-$RunId"
New-Item -ItemType Directory -Force $work | Out-Null
foreach ($d in Get-ChildItem tools/e2e/scenarios -Directory) {
    $seed = Join-Path $d.FullName 'seed.ps1'; $assert = Join-Path $d.FullName 'assert.ps1'; $config = Join-Path $d.FullName 'config.json'
    if (-not $runAll -and -not $why.ContainsKey($d.Name)) { continue }
    # wicore-fresh makes a new target project, which the target org may have no room for: it runs only behind the flag.
    if ($d.Name -eq 'wicore-fresh' -and $env:AMTE2E_FRESH_PROJECTS -ne '1') { "wicore-fresh skipped: AMTE2E_FRESH_PROJECTS is not set"; continue }
    # The previous scenario's projects go back to the pool before the next one takes any.
    & $releasePool
    if (-not ((Test-Path $seed) -and (Test-Path $assert) -and (Test-Path $config))) { $notCovered += $d.Name; continue }
    # Every named resource the seed takes (projects, and processes such as Process or NoBugProcess) gets a scratch
    # name of this run, so nothing shared is seeded or migrated into. The config is re-pointed at the same names,
    # longest value first so that "x" is not replaced inside "x-src". Each script gets only the parameters it declares.
    # A process gets a scratch name only where the proof can stand one up: one the seed creates itself, or the shared
    # baseline (amte2e-agile), which Initialize-E2EEnvironment.ps1 creates here in both test orgs before seeding.
    # Any other process the seed expects to exist keeps its shared name, and the run says so.
    $seedText = Get-Content $seed -Raw
    $defaults = [ordered]@{}
    foreach ($m in [regex]::Matches($seedText, '\[string\]\$(\w+)\s*=\s*"([^"]+)"')) { $defaults[$m.Groups[1].Value] = $m.Groups[2].Value }
    if (-not ($defaults.Contains('Src') -and $defaults.Contains('Tgt'))) { $notCovered += $d.Name; continue }
    $baseline = 'amte2e-agile'
    $names = [ordered]@{}; $initialise = @()
    foreach ($k in @($defaults.Keys)) {
        $scratchName = "amte2e-$Item-p$RunId-$($d.Name)-$($k.ToLower())"
        if ($k -notmatch 'Process$') { $names[$k] = $scratchName; continue }
        if ($seedText -match "New-E2E\w*Process[^\n]*\`$$k\b") { $names[$k] = $scratchName }
        elseif ($defaults[$k] -eq $baseline) { $names[$k] = $scratchName; $initialise += $scratchName }
        else { "   $k keeps the shared process $($defaults[$k]): the seed expects it to exist and the proof cannot create it"; $defaults.Remove($k) }
    }
    $scratch += $names.Values
    $processes += @($names.Keys | Where-Object { $_ -match 'Process$' } | ForEach-Object { $names[$_] })
    # Projects come from the pool and are named by their slot in the summary; only scratch processes are named here.
    $procKeys = @($names.Keys | Where-Object { $_ -match 'Process$' })
    "== $($d.Name): $(@($names.Keys | Where-Object { $_ -notmatch 'Process$' }) -join ', ') from the pool$(if ($procKeys) { "; processes $(($procKeys | ForEach-Object { "$_=$($names[$_])" }) -join ', ')" })"
    $initFailed = $false
    foreach ($p in $initialise) {
        pwsh -NoProfile -File tools/e2e/Initialize-E2EEnvironment.ps1 -Process $p *>&1 | Tee-Object -FilePath (Join-Path $work "$($d.Name).log") -Append
        if ($LASTEXITCODE -ne 0) { $initFailed = $true }
    }
    if ($initFailed) { $failed += "$($d.Name) (creating its scratch process)"; continue }
    $cfgText = Get-Content $config -Raw
    foreach ($k in ($defaults.Keys | Sort-Object { $defaults[$_].Length } -Descending)) { $cfgText = $cfgText.Replace($defaults[$k], $names[$k]) }
    $cfgFile = Join-Path $work "$($d.Name).json"
    $cfgText | Set-Content $cfgFile -Encoding utf8
    $argsFor = { param($script, [hashtable]$extra = @{})
        $declared = @([regex]::Matches((Get-Content $script -Raw), '(?m)\[\w+\]\$(\w+)') | ForEach-Object { $_.Groups[1].Value })
        $a = @(); foreach ($k in $names.Keys) { if ($declared -contains $k) { $a += "-$k", $names[$k] } }
        foreach ($k in $extra.Keys) { if ($declared -contains $k) { $a += "-$k", $extra[$k] } }
        , $a }
    $seedArgs = & $argsFor $seed
    pwsh -NoProfile -File $seed @seedArgs *>&1 | Tee-Object -FilePath (Join-Path $work "$($d.Name).log") -Append
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (seed)"; continue }
    & $renewPool
    pwsh -NoProfile -File tools/e2e/Invoke-MigrationE2E.ps1 -Config $cfgFile *>&1 | Tee-Object -FilePath (Join-Path $work "$($d.Name).log") -Append
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (migration)"; continue }
    & $renewPool
    $assertArgs = & $argsFor $assert @{ OutFile = (Join-Path $work "$($d.Name).assert.md") }
    pwsh -NoProfile -File $assert @assertArgs *>&1 | Tee-Object -FilePath (Join-Path $work "$($d.Name).log") -Append
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (assertions)" }
}
} finally {
    # Give back every pool project this run holds, whatever happened above. Each is cleared and renamed back.
    & $releasePool
}
"not covered (no seed, config and assert with scratch-project parameters): $($notCovered -join ', ')"
"pool projects used: $((@($used | Sort-Object -Unique)) -join ', ')"
"released: $((@($released | Sort-Object -Unique) | ForEach-Object { "$_ (cleared)" }) -join ', ')"
"scratch processes (Remove-E2EObjects.ps1 deletes them): $($processes -join ', ')"
if ($notReleased) { "NOT RELEASED:"; $notReleased | ForEach-Object { "  $_" }; Write-Error "pool projects could not be given back"; exit 1 }
if (-not $scratch) { Write-Error "no scenario was run: the proof covers nothing"; exit 1 }
if ($failed) { Write-Error "end-to-end proof failed: $($failed -join ', '); each scenario's output is in $work\<scenario>.log"; exit 1 }
"all covered scenarios passed; results in $work"
exit 0
} finally { $lock.Dispose(); Remove-Item $ticket -ErrorAction SilentlyContinue }
