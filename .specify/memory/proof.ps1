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
# The run id ends in six random hex digits, and a run id already in use is refused at the start, so two runs do not
# share names. Deletion does not rely on that: releasePool deletes only the projects this invocation recorded making.
param([string]$RunId = "$(Get-Date -Format 'MMddHHmm')$([Guid]::NewGuid().ToString('N').Substring(0, 6))", [string]$Item, [switch]$All, [string]$Base = 'origin/main',
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
if ($Only) {
    # A name asked for that cannot run (mistyped, or without a seed, config and assert) fails here, before anything runs,
    # so a run never reports success while silently skipping a scenario it was asked for.
    $unknown = @($Only | Where-Object { $runnable -notcontains $_ })
    if ($unknown) { Write-Error "-Only names scenarios that cannot run (no folder with seed.ps1, assert.ps1 and config.json): $($unknown -join ', '). Runnable: $($runnable -join ', ')"; exit 1 }
    $runAll = $false; $why = @{}; foreach ($n in $Only) { $why[$n] = 'asked for (-Only)' }
}
if ($runAll) { "== running all $($runnable.Count) runnable scenarios: $reasonAll" } else { "== running $($why.Count) scenario(s):"; $why.Keys | Sort-Object | ForEach-Object { "   $_ ($($why[$_]))" } }

# Proofs in different worktrees run side by side; two in the same worktree would overwrite each other's scenario
# state files (tools/e2e/scenarios/*/state.local.json), so a second one here is refused at once (Martin, #3351).
# The file is held open for the whole run and the OS lets go of it when this process ends, however it ends.
$worktreeKey = [BitConverter]::ToString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes((Get-Location).Path.ToLowerInvariant()))).Replace('-', '').Substring(0, 16)
$runningPath = Join-Path ([IO.Path]::GetTempPath()) "amte2e-proof-$worktreeKey.running"
try { $running = [IO.File]::Open($runningPath, 'OpenOrCreate', 'ReadWrite', 'None') }
catch [IO.IOException] { Write-Error "another proof is already running in this worktree ($((Get-Location).Path)); wait for it to finish, or run this one from another worktree."; exit 1 }

# One timestamped line per step, so a long run shows where it is.
$progress = { param([string]$Step) "[{0:HH:mm:ss}] {1}" -f (Get-Date), $Step }
# A seed or assert takes and reads pool projects in its own process. If Azure DevOps stayed unavailable there, its
# log ends with the pool's one line; the next scenario would fail the same way, so the proof stops with that line.
$stopIfUnavailable = { param([string]$Log)
    # Every form of the pool's line (a read that kept failing, a write that is not sent again) ends like this.
    $line = @(Select-String -Path $Log -Pattern ' returned .*Every lease is kept\.' -ErrorAction SilentlyContinue | Select-Object -Last 1)
    # The exception type lives in the module (Import-Module does not export classes), so it is thrown there.
    if ($line) { & (Get-Module E2EAzDo) { param($m) throw [E2EUnavailableException]::new($m) } ($line[0].Line -replace '^.*?(\S+ \S+ in \S+ returned)', '$1').Trim() }
}
$unavailable = $false
$failed = @(); $notCovered = @(); $scratch = @(); $processes = @(); $used = @(); $released = @(); $notReleased = @()
# Projects come from the scratch project pool (#3342). Every seed this run starts takes them as this proof, so the
# proof can give back everything the run holds, after each scenario and again in the finally below.
$poolRun = "proof-$Item-$RunId"
$env:AMTE2E_POOL_OWNER = "${PID}:$poolRun"
$env:AMTE2E_POOL_ITEM = $Item
Import-Module (Join-Path $PWD 'tools/e2e/E2EAzDo.psm1') -Force -WarningAction SilentlyContinue
$poolOrgs = @('nkdagility-adomt-source', 'nkdagility-adomt-target')
# Azure DevOps stayed unavailable (#3351): the pool's one line is the whole report, after the finally below has run.
trap { if ($_.Exception.GetType().Name -eq 'E2EUnavailableException') { "STOPPED: $($_.Exception.Message)"; exit 1 }; break }
# What this invocation creates outside the pool is recorded here (by pid as well as run id), so it deletes only that.
$env:AMTE2E_CREATED_LOG = Join-Path ([IO.Path]::GetTempPath()) "amte2e-created-$Item-$RunId-$PID.log"
# A run id already in use (passed again with -RunId, or drawn twice) is refused, so no two runs share a name.
$inUse = @($(foreach ($org in $poolOrgs) { @(Get-E2EProjects -Org $org -Prefix "amte2e-$Item-p$RunId-").name; @(Get-E2EProcesses -Org $org -Prefix "amte2e-$Item-p$RunId-").name }) | Where-Object { $_ })
if ($inUse) { Write-Error "run id $RunId is already in use (found: $($inUse -join ', ')); give another -RunId"; exit 1 }
# Renews this run's leases before each long step (migration, assertions), so they do not expire mid-scenario.
$renewPool = { foreach ($org in $poolOrgs) { foreach ($h in @(Get-E2EPoolProject -Org $org -Owner $poolRun)) { Set-E2EPoolLease $h.Id $org -Renew } } }
$releasePool = {
    # wicore-fresh's brand-new target project is deleted, the one project a run makes in the target org. Only the
    # projects this invocation recorded as it made them are deleted, checked by id, never any found by name alone.
    $freshProjects = @(if (Test-Path $env:AMTE2E_CREATED_LOG) { Get-Content $env:AMTE2E_CREATED_LOG | Where-Object { $_ -like 'nkdagility-adomt-target|*' } | Sort-Object -Unique })
    foreach ($line in $freshProjects) {
        $fOrg, $fId, $fName = $line.Split('|', 3)
        try {
            # Deleted by the recorded id, never by a fresh lookup of the name, which another run could have reused.
            if (Remove-E2EDisposableProject $fName -Org $fOrg -Id $fId) { "deleted fresh project $fOrg/$fName" }
            $rest = @(Get-Content $env:AMTE2E_CREATED_LOG | Where-Object { $_ -ne $line }); Set-Content $env:AMTE2E_CREATED_LOG $rest
        }
        catch {
            # Azure DevOps unavailable: stop here, so nothing else is given back and every lease is kept (#3351).
            if ($_.Exception.GetType().Name -eq 'E2EUnavailableException') { throw }
            $script:notReleased += "NOT DELETED fresh project $fOrg/$fName`: $($_.Exception.Message)"
        }
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
    & $progress "$($d.Name): seed"
    pwsh -NoProfile -File $seed @seedArgs *>&1 | Tee-Object -FilePath (Join-Path $work "$($d.Name).log") -Append
    if ($LASTEXITCODE -ne 0) { & $stopIfUnavailable (Join-Path $work "$($d.Name).log"); $failed += "$($d.Name) (seed)"; & $progress "$($d.Name): seed FAILED"; continue }
    & $renewPool
    & $progress "$($d.Name): migration"
    pwsh -NoProfile -File tools/e2e/Invoke-MigrationE2E.ps1 -Config $cfgFile *>&1 | Tee-Object -FilePath (Join-Path $work "$($d.Name).log") -Append
    if ($LASTEXITCODE -ne 0) { $failed += "$($d.Name) (migration)"; & $progress "$($d.Name): migration FAILED"; continue }
    & $renewPool
    & $progress "$($d.Name): assertions"
    $assertArgs = & $argsFor $assert @{ OutFile = (Join-Path $work "$($d.Name).assert.md") }
    pwsh -NoProfile -File $assert @assertArgs *>&1 | Tee-Object -FilePath (Join-Path $work "$($d.Name).log") -Append
    if ($LASTEXITCODE -ne 0) { & $stopIfUnavailable (Join-Path $work "$($d.Name).log"); $failed += "$($d.Name) (assertions)"; & $progress "$($d.Name): assertions FAILED" } else { & $progress "$($d.Name): passed" }
}
} catch {
    # Azure DevOps stayed unavailable: nothing is given back, so every lease this run holds is kept (#3351). Giving
    # back one org's projects while the other is down would release some leases and not others. Once this process
    # has exited its leases read as Orphaned, and Remove-E2EObjects.ps1 -Prefix amte2e-<item>- gives them back.
    if ($_.Exception.GetType().Name -eq 'E2EUnavailableException') { $unavailable = $true }
    throw
} finally {
    if ($unavailable) { & $progress "Azure DevOps unavailable: pool projects kept leased; give them back later with tools/e2e/Remove-E2EObjects.ps1 -Prefix amte2e-$Item-" }
    else {
        # Give back every pool project this run holds, whatever else happened above. Each is cleared and renamed back.
        & $progress "giving pool projects back"
        & $releasePool
        & $progress "done"
    }
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
