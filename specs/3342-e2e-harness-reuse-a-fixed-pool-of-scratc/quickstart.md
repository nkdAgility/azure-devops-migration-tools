# Quickstart: prove the pool on the test orgs

Prerequisites: `pwsh` 7+, `az login` as an identity with access to `nkdagility-adomt-source` and `nkdagility-adomt-target` (no PAT needed; never print one), the repo built (`MigrationTools.ConsoleFull`), and `Initialize-E2EEnvironment.ps1` already run so `amte2e-agile` exists. Run from the repo root. Contracts: [pool-module.md](contracts/pool-module.md), [pool-scripts.md](contracts/pool-scripts.md). Entities: [data-model.md](data-model.md).

Each step lists its pass condition. Each matches an Outcome check in [item.md](item.md).

## 0. Baseline

Record, per org: live project count, deleted project count, process count, `Custom.amte2e*` field count. Step 4 compares against these. (`Test-E2EPool.ps1 -Case Scenario` records and compares them itself.)

## 1. Adopt

```powershell
pwsh -NoProfile -File tools/e2e/Initialize-E2EPool.ps1 -WhatIf
pwsh -NoProfile -File tools/e2e/Initialize-E2EPool.ps1
pwsh -NoProfile -File tools/e2e/Initialize-E2EPool.ps1
```

Pass: all exit 0. The first lists renames and skips and changes nothing. The second renames eligible projects, creates none, skips `migrationSource1`/`migrationTarget1` and anything leased or changed in the last 24 hours, and prints an unchanged live count. The third prints no renames.

## 2. One take and release

```powershell
pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Cycle
```

Pass: exit 0, every case `PASS` in both orgs. The taken project is empty by inventory, on the requested process and renamed; after release it is `amte2e-pool-NN` on `amte2e-agile` with no `amte2e-pool-lease` team.

## 3. Concurrency and leases

```powershell
pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Race -Trials 10
pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Lease
```

Pass: exit 0. Race: 10 of 10 trials returned distinct projects. Lease: an expired lease and a dead local claimant's lease are taken over, a live lease is left alone, a half-filled project is cleared on take.

## 4. A scenario through the pool, twice

```powershell
pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Scenario -Scenario wicore -Runs 2
```

Pass: exit 0. `wicore` passes both times, and live, deleted, process and field counts equal the baseline.

## 5. The proof and the failure paths

Run from an item worktree so `.specify/items/current.json` names the item:

```powershell
pwsh -NoProfile -File .specify/memory/proof.ps1 -Item 3342
```

Pass: exit 0; the output has `pool projects used:` and `released:`, and no `left for clean-up` line. Failure paths, each of which must leave every project released:

- Stop a proof part-way (Ctrl+C in a seed). Pass: `Get-E2EPoolProject -All` shows no lease of the run, or one that the next take clears.
- Run `linkscope` before and after the change. Pass: its assertions pass both times and no project is deleted.
- With every pool project leased, call `New-E2EProject`. Pass: it fails naming the org and each lease's claimant and expiry, and creates no project.
- Ask for a name outside the guard (`migrationTarget1`). Pass: refused by `Assert-E2EName`; nothing touched.

Report the output and exit codes for each step in the pull request, run on `main` and on the change as `CLAUDE.md` asks (on `main`, steps 2 to 4 do not exist, so run step 5 and the live counts).

## 6. Cleanup and unit tests

```powershell
pwsh -NoProfile -File tools/e2e/Remove-E2EObjects.ps1 -All -WhatIf
pwsh -NoProfile -Command "Invoke-Pester tools/e2e -CI"
git diff --quiet origin/main -- src
```

Pass: the first lists no pool project for deletion and exits 0. Pester exits 0 (`-All` and a per-issue prefix never delete a pool project; `-ClearPool` releases every lease). The last prints nothing and exits 0.

## 7. Docs

```powershell
./GenerateDocs.ps1
```

Pass: `platform/specs/e2e-harness/cleanup.md`, `platform/specs/e2e-harness/scenarios.md` and the "Validating Changes End to End" section of `platform/specs/architecture.md` describe the pool, the lease, take and release, and the not-poolable list. Remove anything created only for the run (scratch processes the proof made; the pool projects stay).
