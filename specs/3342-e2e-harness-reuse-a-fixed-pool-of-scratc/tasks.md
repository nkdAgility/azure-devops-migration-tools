# Tasks: e2e-harness reuses a fixed pool of scratch projects (#3342)

**Input**: spec.md, plan.md, research.md (R1-R12), data-model.md, contracts/pool-module.md, contracts/pool-scripts.md.
**Tests**: requested by the item's Outcome checks (Pester with REST mocked, and live `Test-E2EPool.ps1`).

## Phase 1: Setup

- [ ] T001 Create `tools/e2e/E2EPool.psm1` with the module header (PowerShell 7+ `#Requires -Version 7`), constants `PoolPrefix = "amte2e-pool-"`, `LeaseTeam = "amte2e-pool-lease"`, `LeaseHours = 2`, `BaseProcess = "amte2e-agile"`, and import of `E2EAzDo.psm1` helpers it calls back into (`Invoke-E2ERest`, `Assert-E2EName`, `Wait-E2EOperation`, `Get-E2EProcessId`)

## Phase 2: Foundational (lease and owner, blocks every story)

- [ ] T002 Implement owner identity in `tools/e2e/E2EPool.psm1`: `$env:AMTE2E_POOL_OWNER` (`<pid>:<run>`), else a GUID run id with no pid; token `machine|pid|run` (data-model "Lease team")
- [ ] T003 Implement `ConvertTo-E2EPoolLease`/`ConvertFrom-E2EPoolLease` for the one-line description `slot=NN;machine=;pid=;run=;item=;claimed=;expires=` (UTC ISO 8601) in `tools/e2e/E2EPool.psm1`
- [ ] T004 Implement `Get-E2EPoolLease -Project` and lease state (Free, Held, Expired, Orphaned: "machine is this machine and pid is set and not running") in `tools/e2e/E2EPool.psm1`
- [ ] T005 Implement `Set-E2EPoolLease -Renew` (expires = now + 2 h; throws "lease lost" if the team's token is not this run's) and `Assert-E2EPoolLeaseHeld` (the re-read before each destructive step) in `tools/e2e/E2EPool.psm1`
- [ ] T006 [P] Pester tests for T002-T005 (parse, states, renew, lost lease) in `tools/e2e/E2EPool.Tests.ps1`

## Phase 3: US1 + US2 - take and release (P1) MVP

**Independent test**: `Test-E2EPool.ps1 -Case Cycle` on both orgs.

- [ ] T007 [US1] Implement `Get-E2EPoolInventory` and `Test-E2EPoolClear` from REST surfaces in data-model "Inventory" (work items and recycle bin, teams, child nodes, queries, tags, repos and repo recycle bin, build and release definitions, variable groups, service connections, environments, wikis, test plans, test runs, dashboards) in `tools/e2e/E2EPool.psm1`
- [ ] T008 [US1] Implement `Clear-E2EPoolProject` in research R3 order (work items destroyed; test plans; pipelines, variable groups, service connections, environments, wikis, dashboards except default; queries; teams except default and lease; child nodes; tags; repos replaced by one empty repo named after the project and the old ones purged) in `tools/e2e/E2EPool.psm1`
- [ ] T009 [US1] Implement `Switch-E2EPoolProcess` (`POST {project}/_apis/wit/projectprocessmigration` 7.1-preview.1, poll, verify `System.ProcessTemplateType`) and `Rename-E2EPoolProject` (`PATCH _apis/projects/{id}`, poll, refuse an existing name) in `tools/e2e/E2EPool.psm1`
- [ ] T010 [US1] Implement `Get-E2EPoolProject [-Owner] [-All]` and `Request-E2EPoolProject -Name -Process` (claim by lease team, takeover by id per R2, clear, verify, switch, rename, renew; release lease on failure; "No free pool project in <org>. Leases: ..." when none) in `tools/e2e/E2EPool.psm1`
- [ ] T011 [US2] Implement `Release-E2EPoolProject` and `Clear-E2EPool -Owner|-All` per R7 (clear, switch to `amte2e-agile`, rename to `amte2e-pool-NN` from `slot`, delete lease last; keep lease on failure; collect errors) in `tools/e2e/E2EPool.psm1`
- [ ] T012 [US1] Make `New-E2EProject` call `Request-E2EPoolProject` (same name already leased by this run is returned) and `Remove-E2EProject` call `Release-E2EPoolProject` in `tools/e2e/E2EAzDo.psm1`; signatures unchanged; export the pool functions
- [ ] T013 [P] [US1] Pester tests for take and release with REST mocked: no project created, guard applied, exhausted pool message (Q-7 `Exhausted`), lease deleted on failed take in `tools/e2e/E2EPool.Tests.ps1`
- [ ] T014 [US1] `Test-E2EPool.ps1 -Case Cycle` in `tools/e2e/Test-E2EPool.ps1` (PASS/FAIL lines, exit 0 only if all pass)

## Phase 4: US3 - two proofs never share (P1)

- [ ] T015 [US3] `Test-E2EPool.ps1 -Case Race -Trials N` (two `pwsh` takers started at once) and `-Case Lease` (expired, orphaned, held, half-filled) in `tools/e2e/Test-E2EPool.ps1`

## Phase 5: US4 - adopt (P2)

- [ ] T016 [US4] `tools/e2e/Initialize-E2EPool.ps1` per contracts/pool-scripts.md (eligible: guard, not pool, no lease, no change in 24 h by project, newest work item and newest push; skips with reason; lowest free NN; `-WhatIf`; exit 0 when live count unchanged)
- [ ] T017 [P] [US4] Pester tests for adopt eligibility in `tools/e2e/E2EPool.Tests.ps1`

## Phase 6: US5 - cleanup and proof (P2)

- [ ] T018 [US5] `tools/e2e/Remove-E2EObjects.ps1`: remove the project delete loop, release pool projects of the prefix, add `-ClearPool`, keep recycle-bin, process and field steps
- [ ] T019 [P] [US5] Update `tools/e2e/Remove-E2EObjects.Tests.ps1` for no project deletes and `-ClearPool`
- [ ] T020 [US5] `.specify/memory/proof.ps1`: set `AMTE2E_POOL_OWNER`, `finally { Clear-E2EPool -Owner }`, renew between scenarios, print `pool projects used:` and `released:`, drop the "left for clean-up" line, fail on `NOT RELEASED`
- [ ] T021 [US5] `tools/e2e/scenarios/picklists/cleanup.ps1` and `tools/e2e/scenarios/testplans-3260/run.ps1` release instead of delete

## Phase 7: US6 - linkscope (P3)

- [ ] T022 [US6] `tools/e2e/scenarios/linkscope/seed.ps1`: the missing project is a name never created; no `New-E2EProject`/`Remove-E2EProject` for it

## Phase 8: Polish

- [ ] T023 Docs: `platform/specs/e2e-harness/cleanup.md`, `platform/specs/e2e-harness/scenarios.md`, `platform/specs/architecture.md` ("Validating Changes End to End") describe `amte2e-pool`, the lease, take and release, adopt, and the not-poolable list
- [ ] T024 Run the Outcome checks in `item.md` live and record output and exit codes for the PR

## Dependencies

T001 → T002-T005 → T007-T012 (US1/US2) → T014. US3 (T015), US4 (T016), US5 (T018-T021), US6 (T022) each need only US1/US2. Tests marked [P] can be written alongside their implementation tasks.

## Strategy

MVP is Phases 1-3 (take and release, proven by `-Case Cycle`). Then adopt (needed before live runs use the existing projects), race and lease, cleanup and proof, linkscope, docs, and the live outcome checks.
