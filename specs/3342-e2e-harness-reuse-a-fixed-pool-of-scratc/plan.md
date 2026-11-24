# Implementation Plan: e2e-harness reuses a fixed pool of scratch projects

**Branch**: `topic/3342-scratch-project-pool` | **Date**: 2026-11-24 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md`. The Purpose, paths, Checked live findings and settled questions in [item.md](item.md) bind this plan.

## Summary

`New-E2EProject` stops creating projects. It claims a free `amte2e-pool-NN` project by creating the lease team `amte2e-pool-lease` (atomic, proven live), clears the project, switches its process with `projectprocessmigration`, renames it to the name the seed asked for and returns it. `Remove-E2EProject` clears it, switches it back to `amte2e-agile`, renames it to `amte2e-pool-NN` and deletes the lease team. The project count per org never changes. A one-off `Initialize-E2EPool.ps1` renames the existing live scratch projects into the pool. `Remove-E2EObjects.ps1` and `.specify/memory/proof.ps1` follow the pool, `linkscope` uses a name that never exists, and the docs change with the code.

## Technical Context

**Language/Version**: PowerShell 7+ only (module `E2EAzDo.psm1`, scripts, Pester 5 tests).

**Primary Dependencies**: Azure DevOps REST 7.1 through the existing `Invoke-E2ERest` (Entra token from `az login`, or `AMT_TEST_PAT`). No new module or credential.

**Storage**: The org itself: the lease team and its description are the only shared state. No local state beyond the proof's existing queue and lock.

**Testing**: Pester 5 with mocked REST for logic (`tools/e2e/E2EPool.Tests.ps1`, `Remove-E2EObjects.Tests.ps1`), and `tools/e2e/Test-E2EPool.ps1` for the live cases named in the item's Outcome checks (Cycle, Race, Lease, Scenario).

**Target Platform**: Developer machines and CI with `pwsh` 7+ and `az`.

**Project Type**: Test tooling (`e2e-harness` system, `tools/e2e/`). No product code.

**Performance Goals**: A take or release takes minutes, not seconds (process switch and rename are operations that are polled). Clearing 45 projects in `-ClearPool` runs one project at a time.

**Constraints**: Never touch `migrationSource1`/`migrationTarget1` (every call goes through `Assert-E2EName`); no change under `src/`; no new project slots (the target org is at its 1,000 limit counting the deleted ones); the harness identity may lack the permission to destroy work items (checked at build, take fails visibly if so).

**Scale/Scope**: About 45 pool projects per org (all eligible live scratch projects). The largest scenario takes 6 projects in one org (`users`). 30 scenario folders, of which about 25 call `New-E2EProject`.

No `NEEDS CLARIFICATION` remains. Unknowns are resolved in [research.md](research.md).

## Constitution Check

`.specify/memory/constitution.md` is the unfilled template, so it states no gates. `platform/specs/rules.md` and `.github/instructions/src.instructions.md` cover `src/` only. The rules that do reach `tools/e2e/`:

| Rule | Result |
|---|---|
| `architecture.md`: use your own scratch projects; never modify projects the automated tests use | Pass. The tests name only `migrationSource1`/`migrationTarget1`; the pool takes only names that pass `Assert-E2EName`. Adopt skips those two by name before any call. |
| `systems.md`: a change to `e2e-harness` updates `platform/specs/e2e-harness/` | Pass, planned: `cleanup.md`, `scenarios.md`, and the "Validating Changes End to End" section of `architecture.md` (FR-017). |
| `Assert-E2EName` guard | Pass, not widened. Pool names `amte2e-pool-NN` already satisfy it. |
| Item Purpose and paths | Pass. Every file below is inside `tools/e2e/**`, `platform/specs/e2e-harness/**`, `platform/specs/architecture.md`, `.specify/memory/proof.ps1` or this folder. |
| Definition of done (4): a real e2e run | Applies to `tools/`. The quickstart is the run on the test orgs. |

Post-design re-check: pass. The design adds no file outside the item's paths and widens no guard. Complexity Tracking: none.

## Project Structure

### Documentation (this feature)

```text
specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/
├── item.md
├── spec.md
├── plan.md              # This file
├── research.md          # Phase 0
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
└── contracts/
    ├── pool-module.md   # Module functions and their behaviour
    └── pool-scripts.md  # Script parameters, output and exit codes
```

### Source Code (repository root)

```text
tools/e2e/
├── E2EAzDo.psm1              # New-E2EProject / Remove-E2EProject become take / release; calls the pool module
├── E2EPool.psm1              # NEW: lease, clear, inventory, process switch, rename, take, release
├── E2EPool.Tests.ps1         # NEW: Pester, REST mocked
├── Initialize-E2EPool.ps1    # NEW: one-off adopt (-WhatIf, idempotent)
├── Test-E2EPool.ps1          # NEW: live cases Cycle | Race | Lease | Scenario
├── Remove-E2EObjects.ps1     # -ClearPool; projects never deleted
├── Remove-E2EObjects.Tests.ps1
└── scenarios/
    ├── linkscope/seed.ps1    # missing project uses a name that never exists; no delete
    ├── picklists/cleanup.ps1 # releases instead of deleting
    ├── testplans-3260/run.ps1# releases instead of deleting
    └── (any seed whose process cannot be switched: marked not poolable)

.specify/memory/proof.ps1     # fixed pool, release in finally, report pool slots
platform/specs/e2e-harness/cleanup.md
platform/specs/e2e-harness/scenarios.md
platform/specs/architecture.md   # "Validating Changes End to End"
```

**Structure Decision**: Pool logic goes in a new module `E2EPool.psm1`, because `E2EAzDo.psm1` holds REST helpers and name rules and pool logic (lease, clear, inventory) is a separate concern that needs its own tests. `E2EAzDo.psm1` keeps the public names `New-E2EProject` and `Remove-E2EProject`, so no seed signature changes. `E2EAzDo.psm1` imports the pool module; the pool module calls back only through `Invoke-E2ERest`, `Assert-E2EName` and `Wait-E2EOperation` (no circular state). Seeds change only where they hard-code deletion (`linkscope`) or cleanup (`picklists/cleanup.ps1`, `testplans-3260/run.ps1`).

## Complexity Tracking

None.
