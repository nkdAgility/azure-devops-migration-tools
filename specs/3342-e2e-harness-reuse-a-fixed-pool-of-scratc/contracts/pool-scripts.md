# Contract: scripts

All run with `pwsh` 7+. All use `SupportsShouldProcess` where they change anything.

## `tools/e2e/Initialize-E2EPool.ps1` (adopt, one-off)

```text
Initialize-E2EPool.ps1 [-Organisations <string[]>] [-Size <int>] [-WhatIf]
```

- For each org: list live projects; eligible = name `amte2e-*` or `amt<issue>-*` and not already `amte2e-pool-NN`, no lease team, no change in the last 24 hours (project, newest work item, newest push).
- Skips, with a reason per project: `migrationSource1`, `migrationTarget1` ("used by automated tests"), a name outside the guard, "leased", "changed in the last 24 hours".
- Renames eligible projects to `amte2e-pool-NN`, lowest free numbers first, up to `-Size` (default: all eligible). Creates no project and deletes nothing.
- Output: one line per rename (`renamed <org>/<old> -> <org>/amte2e-pool-NN`) or skip; then `pool in <org>: <n> projects, live <before> -> <after>`.
- Exit 0 when the live count is unchanged in every org; 1 otherwise or on any failure. A second run prints no renames and exits 0.

## `tools/e2e/Test-E2EPool.ps1` (live checks for the Outcome checks)

```text
Test-E2EPool.ps1 -Case Cycle | Race | Lease | Scenario [-Trials <int> = 10] [-Scenario <name>] [-Runs <int> = 2]
```

| Case | Orgs | Proves |
|---|---|---|
| `Cycle` | both | take returns an empty inventory on the requested process, renamed; release returns `amte2e-pool-NN` on `amte2e-agile` with no lease team |
| `Race` | target | two takes started at once as separate processes never return the same project, over `-Trials` |
| `Lease` | target | an expired lease and a dead local claimant's lease are taken over; a live lease is left alone; a half-filled project is cleared on take |
| `Scenario` | both | `-Scenario` runs `-Runs` times through the pool and passes; live, deleted, process and `Custom.amte2e*` counts are equal before and after |

Output: `PASS <case>: <detail>` or `FAIL <case>: <detail>` per check. Exit 0 only if every check passes. Leaves every project it took released; it takes projects only from the pool.

## `tools/e2e/Remove-E2EObjects.ps1` (changed)

```text
Remove-E2EObjects.ps1 [-Organisations <string[]>] [-Prefix <string>] [-All] [-ClearPool] [-WhatIf]
```

| Run | Projects | Pool | Processes | Fields |
|---|---|---|---|---|
| `-Prefix` / `-All` | not deleted (the project loop is gone) | pool projects of that prefix are released, so processes can be deleted | as before | as before |
| `-ClearPool` | not deleted | every leased pool project is cleared and released, after listing each claimant | not touched | not touched |

`-All` and `-Prefix` never delete a pool project, whatever the prefix. `-WhatIf` lists the releases ("would release …"). Existing refusals and exit codes are unchanged. Recycled (already deleted) scratch projects are still destroyed where the service allows.

## `.specify/memory/proof.ps1` (changed)

- Sets `AMTE2E_POOL_OWNER=<proof pid>:<run id>` before running any seed, so every take in the run carries the proof's identity.
- Runs the scenario loop inside `try { ... } finally { Clear-E2EPool -Owner <run> }`, so a failed or interrupted run releases what it holds.
- Between scenarios, renews the leases of the run.
- Output changes: the line `scratch projects and processes left for clean-up: ...` is removed. In its place: `pool projects used: <org>/amte2e-pool-NN, ...` and `released: <org>/amte2e-pool-NN, ...` (each used slot with `cleared`). A used slot that could not be released is listed under `NOT RELEASED` and fails the proof.
- The queue and lock stay.
