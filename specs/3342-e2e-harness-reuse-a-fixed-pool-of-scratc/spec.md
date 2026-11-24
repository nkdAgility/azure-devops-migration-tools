# Feature Specification: e2e-harness reuses a fixed pool of scratch projects

**Feature Branch**: `topic/3342-scratch-project-pool`

**Created**: 2026-11-24

**Status**: Draft

**Input**: #3342 e2e-harness: reuse a fixed pool of scratch projects. Purpose, paths, Checked live findings and settled questions are in `item.md` and bind this spec.

## Outcome

Each end-to-end scenario takes its scratch projects from a fixed pool of projects that the harness keeps and clears between runs, so a proof no longer creates or deletes projects and stops using up the test organisations' 1,000-project limit.

## Size and scope

**Size**: medium. One PowerShell module changes (`E2EAzDo.psm1`), two scripts follow it (`Remove-E2EObjects.ps1`, `.specify/memory/proof.ps1`), one new adopt script is added, and two docs are updated. No scenario assertion changes; seed changes only where a seed hard-codes a project name or deletes a project itself.

**In scope**
- A pool of projects per test org (source and target), named `amte2e-pool-NN`, never deleted by the harness.
- Taking a free pool project, clearing it, switching its process, renaming it to the name the seed asked for, and returning it.
- Releasing a project: clearing it, renaming it back to `amte2e-pool-NN`, and freeing its lease.
- A claim that holds across machines.
- A one-off step that renames existing live `amte2e-*` and `amt3326-*` projects into the pool, with no new project slots.
- `Remove-E2EObjects.ps1` and `proof.ps1` following the pool.
- The `linkscope` scenario's missing-project case, and any seed that hard-codes a name.
- `platform/specs/e2e-harness/` and the "Validating Changes End to End" section of `platform/specs/architecture.md`.

**Out of scope**
- Any change to the migration tool (`src/`).
- Changing which scenarios the proof selects, or any scenario's assertions.
- Purging the projects already in the recycle bin (951 in the target org).
- Deleting the live projects that are not adopted into the pool.
- Anything that touches `migrationSource1` or `migrationTarget1`, or other projects used by `TestCategory=L2|L3` tests.
- Windows PowerShell 5.1: the harness is PowerShell 7+ only.

**Paths and areas touched**: `tools/e2e/**` (module, `Remove-E2EObjects.ps1`, its tests, the adopt script, affected scenario seeds), `.specify/memory/proof.ps1`, `platform/specs/e2e-harness/**`, `platform/specs/architecture.md`, and this folder.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A scenario gets a clean project from the pool (Priority: P1)

A developer or worker runs a scenario. Its seed asks for a project by the name it wants. The harness hands over a free, empty pool project under that name, on the process the seed asks for.

**Why this priority**: This is the outcome. Without it nothing else stops the projects filling the org.

**Independent Test**: Run one scenario twice in a row against the test orgs. The second run starts from an empty project, passes its assertions, and the count of projects in each org is the same before and after both runs.

**Acceptance Scenarios**:

1. **Given** the pool has a free project, **When** a seed asks for project `X`, **Then** a pool project is renamed `X`, holds no work items, extra teams, child area or iteration nodes, queries, extra repos, pipelines, variable groups or test plans, and is on the requested process.
2. **Given** a project that a previous run left half-filled, **When** it is taken, **Then** it is cleared before it is returned.
3. **Given** a seed asks for a name that already exists as a leased pool project of the same run, **When** it is asked again, **Then** the same project is returned and not a second one.
4. **Given** every pool project is leased and unexpired, **When** a seed asks for one, **Then** the call fails visibly, naming the org and the leases' claimants and expiries. No project is created.

---

### User Story 2 - Finishing a run puts the project back (Priority: P1)

When a seed or the proof is done with a project, the harness clears it, renames it back to `amte2e-pool-NN` and frees its lease. It does not delete the project.

**Why this priority**: Release is what keeps the project count constant.

**Independent Test**: After a run, list projects in each org: the project count is unchanged, no project carries a scenario name, and no `amte2e-pool-lease` team remains.

**Acceptance Scenarios**:

1. **Given** a leased project named `X`, **When** it is released, **Then** it is named `amte2e-pool-NN`, is empty, and the lease team is gone.
2. **Given** a release that fails part-way, **When** the next run takes the project, **Then** the lease has expired or been freed and the project is cleared on take.

---

### User Story 3 - Two proofs never share a project (Priority: P1)

Two proofs, on one machine or on different machines (including CI), never get the same pool project at the same time.

**Why this priority**: A shared project would corrupt both proofs.

**Independent Test**: Start two takes of the pool at once; each gets a different project.

**Acceptance Scenarios**:

1. **Given** two takers racing for the last free project, **When** both create the lease team `amte2e-pool-lease`, **Then** exactly one succeeds, and the other moves to the next free project or fails visibly.
2. **Given** a lease whose expiry has passed, **When** another taker finds it, **Then** it may take the project over, clearing it first.
3. **Given** a lease whose expiry has not passed, **When** another taker finds it, **Then** it leaves the project alone.
4. **Given** a lease, **When** its description is read, **Then** it names the claimant and the expiry.

---

### User Story 4 - The existing live projects become the pool (Priority: P2)

A one-off adopt step renames existing live `amte2e-*` and `amt3326-*` projects to `amte2e-pool-NN`. No new project is created, so the live count does not rise.

**Why this priority**: Without it the pool needs new slots in orgs that are close to the limit.

**Independent Test**: Run the adopt step with `-WhatIf`, then for real. Each test org then has the pool-size number of `amte2e-pool-NN` projects, and the org's live project count is the same as before.

**Acceptance Scenarios**:

1. **Given** live `amte2e-*` and `amt<issue>-*` projects, **When** the adopt step runs, **Then** every eligible one (no lease team, no change in 24 hours) is renamed `amte2e-pool-NN`, in each org, and each skipped one is listed with its reason.
2. **Given** `migrationSource1` or `migrationTarget1`, **When** the adopt step runs, **Then** they are never listed, renamed or cleared.
3. **Given** the adopt step runs a second time, **When** the pool already exists, **Then** nothing changes.
4. **Given** a project that is leased or changed in the last 24 hours, **When** the adopt step finishes, **Then** it is left as it is and listed in the output.

---

### User Story 5 - Cleanup and the proof follow the pool (Priority: P2)

`Remove-E2EObjects.ps1` has a "clear the pool" mode, and `-All` clears and releases pool projects instead of deleting them. `proof.ps1` stops using per-run project names and reports the pool projects it used and that each was cleared.

**Why this priority**: Both scripts would otherwise keep creating names and deleting projects.

**Independent Test**: Run `proof.ps1` for one scenario, then read its output. It lists pool projects used and cleared, and does not print "scratch projects ... left for clean-up". Run `Remove-E2EObjects.ps1 -All -WhatIf`: no pool project is listed for deletion.

**Acceptance Scenarios**:

1. **Given** leased pool projects, **When** the clear-the-pool mode runs, **Then** every pool project is cleared, renamed `amte2e-pool-NN` and its lease freed.
2. **Given** `-All` or a per-issue prefix, **When** cleanup runs, **Then** no pool project is deleted, and processes and `Custom.amte2e*` fields are removed as before.
3. **Given** the proof finishes, **When** its summary is printed, **Then** it names each pool project used and states each was cleared.

---

### User Story 6 - `linkscope` still tests a missing project (Priority: P3)

`linkscope` needs a work item that cannot be read: X, in a project that is deleted after links to X are made. Destroying X is not enough, because Azure DevOps then removes its links, and nothing can link into a project that never existed. So `linkscope` alone keeps creating and deleting its `gone` project, only in the source org, through a helper of its own outside the pool (Martin, 2026-11-24: "Delete in source org"). The source org has 146 free project slots; each run uses one for about 28 days.

**Why this priority**: The scenario's purpose is the unreadable item; a pool project cannot give one.

**Independent Test**: Run `linkscope` through the pool. Its assertions pass, its source, target and fail-target projects come from the pool and are given back, and the only project created and deleted is its `gone` project in the source org.

**Acceptance Scenarios**:

1. **Given** the scenario runs, **When** the migration loads X, **Then** it fails with TF26198 as before and the assertions are unchanged.
2. **Given** the helper is asked for a project in the target org, **When** it is called, **Then** it refuses before any call.

---

### Edge Cases

- A run crashes between take and release: the lease expires, and the next taker takes the project over and clears it.
- Clearing fails because the identity cannot destroy work items: the take fails visibly with the permission named. No project is returned half-cleared.
- A scenario needs a process whose base is not Agile: it is listed as not poolable, and its seed says so. It is not forced onto the pool.
- Process switch is refused for a project: the take fails visibly and releases its lease.
- A rename target already exists as a project: the take fails visibly and releases its lease.
- A name outside `amte2e-*` and `amt<issue>-*` is requested: the existing name guard refuses it, as today.
- A seed that hard-codes a name (for example `amte2e-git-src`) is renamed in the same way as any other.
- Pool project in the lease-team name is the only team besides the default: clearing removes every other team and keeps both.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The harness MUST keep a fixed pool of projects per test org named `amte2e-pool-NN`, and MUST never delete a pool project.
- **FR-002**: Taking a project MUST claim it by creating the lease team `amte2e-pool-lease` in it. Creation MUST be the claim: if the team already exists, the claim fails and the next project is tried.
- **FR-003**: The lease team's description MUST hold a claimant token (machine, process id, run id) and an expiry in UTC, two hours after the claim. The holder MUST renew the expiry on every take, release and between scenarios. Before each destructive step (clear, process switch, rename), the holder MUST re-read the lease and stop if its token is no longer there.
- **FR-003a**: An expired lease MAY be taken over: the taker deletes that lease team by its id, only if its description is still the expired one it read, then creates its own lease team. If the create fails, another taker won and the next project is tried. A lease whose claimant is this machine and whose process id is no longer running MAY be taken over at once.
- **FR-004**: After claiming, the harness MUST clear the project: destroy all work items (delete and purge from the recycle bin; Test Cases through the test case API), except Shared Steps and Shared Parameters, which no REST API can delete (only the web portal can). Those are kept as leftovers: retitled `amte2e-pool leftover`, moved to the area `pool-leftovers`, and every link removed (Martin, 2026-11-24: "Tolerate leftovers"). Remove every team except the default team and the lease team; remove area and iteration child nodes except `pool-leftovers`; remove queries; remove work item tag definitions; remove test plans, test runs, pipelines (build and release), variable groups, service connections, environments, dashboards except the default and wikis; replace the repos with a single new empty repo named after the project (old repos deleted and purged from the repo recycle bin).
- **FR-004a**: "Cleared" MUST be checked by an inventory of the REST surfaces in FR-004, plus refs and pull requests in the repo, compared with a fresh project's inventory, allowing only leftovers (FR-004b). It MUST NOT be checked only against the clearing code's own list.
- **FR-004b**: Leftovers MUST be left out of every harness query: `Get-E2EAllWorkItems`, the inventory, and the source queries of the scenarios that migrate Shared Steps or Shared Parameters (`sharedsteps`, `testplans`), by the clause `NOT ([System.Title] = 'amte2e-pool leftover' AND [System.WorkItemType] IN ('Shared Steps','Shared Parameter'))`. Any other item in `pool-leftovers`, or a leftover with a link, MUST fail the inventory.
- **FR-005**: After clearing, the harness MUST switch the project to the process the seed asks for, using the project process migration, and confirm it by reading the project's process type and the work item types the seed uses.
- **FR-006**: After switching, the harness MUST rename the project to the name the seed asked for and return it. The default team and default repo follow the rename (proven live). The returned project MUST be empty.
- **FR-007**: Releasing a project MUST clear it, switch it back to `amte2e-agile`, rename it back to `amte2e-pool-NN` and delete the lease team. It MUST NOT delete the project. Per-run processes are then left with no projects, so cleanup can delete them.
- **FR-007a**: `proof.ps1` MUST release every project it took in a `finally`, including after a failure.
- **FR-008**: If no pool project can be claimed, the harness MUST fail with a message naming the org and each existing lease's claimant and expiry. It MUST NOT create a project.
- **FR-009**: Every pool operation MUST go through the existing name guard. The harness MUST NOT touch `migrationSource1`, `migrationTarget1`, or any name outside the guard.
- **FR-010**: The pool MUST hold every eligible live scratch project per org (see FR-011), about 45 per org. The largest scenario uses 6 projects in one org (`users`: 5 targets), so that pool allows about seven proofs at once with no new project slot.
- **FR-011**: A one-off adopt script MUST rename eligible live `amte2e-*` and `amt<issue>-*` projects into the pool, create no project, be safe to run twice, and support `-WhatIf`. Eligible means no lease team and no change (project, work item or repo) in the last 24 hours. Others are skipped and listed with the reason.
- **FR-012**: `Remove-E2EObjects.ps1` MUST gain a clear-the-pool mode. `-All` and the per-issue prefix MUST NOT delete a pool project. Process and `Custom.amte2e*` field cleanup MUST work as before.
- **FR-013**: `proof.ps1` MUST drop the per-run project names and the "left for clean-up" project list, and MUST report each pool project used and that it was cleared.
- **FR-014**: `linkscope`'s `gone` project MUST be created and deleted only in the source org, through a helper named for that purpose (`New-E2EDisposableProject` / `Remove-E2EDisposableProject`) that refuses any other org and any name outside the guard. No other scenario may use it. Its other projects MUST come from the pool.
- **FR-015**: Scenarios that cannot use a pooled project (a non-Agile base process) MUST be listed as not poolable in `platform/specs/e2e-harness/scenarios.md`.
- **FR-016**: The harness changes MUST run on PowerShell 7+ and need no credential beyond the sign-in the harness already uses.
- **FR-017**: `platform/specs/e2e-harness/cleanup.md`, `platform/specs/e2e-harness/scenarios.md` and the "Validating Changes End to End" section of `platform/specs/architecture.md` MUST describe the pool, the lease, take and release, and the not-poolable list.
- **FR-018**: No file under `src/` MUST change.

### Key Entities

- **Pool project**: A scratch project named `amte2e-pool-NN` when free, or the seed's name while leased. One per slot, per org.
- **Lease team**: The team `amte2e-pool-lease` in a pool project. Its existence is the claim. Its description holds claimant and expiry.
- **Seed request**: The project name and process a scenario seed asks for.
- **Not-poolable scenario**: A scenario that needs a process outside the pool's reach.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After any number of proof runs, the number of live projects and (in the target org) deleted projects, the number of processes and the number of `Custom.amte2e*` fields in each test org are the same as before the runs, once the run's processes and fields are cleaned up.
- **SC-002**: All scenarios the proof selects today pass their assertions through the pool. At most 3 of the 30 may be listed as not poolable, each with a stated reason.
- **SC-003**: Two takes started at the same time never return the same project (checked on at least 10 trials per org).
- **SC-004**: A project taken after a crashed run holds no work items, extra teams, child nodes, queries, extra repos, pipelines, variable groups or test plans when returned.
- **SC-005**: After the adopt step, each org's eligible scratch projects are all `amte2e-pool-NN`, and its live project count has not risen.
- **SC-006**: The proof's final output names every pool project used and states each was cleared, and prints no per-run project name.
- **SC-007**: `git diff` against `main` shows no change under `src/`, and no run touches `migrationSource1` or `migrationTarget1`.
- **SC-008**: Apart from `linkscope`'s `gone` project in the source org, no run creates or deletes a project.

## Checked live before approval (harness sign-in, target org, 2026-11-24)

- The limit: `POST _apis/projects` is refused with "Failed to add a project as this organization already has 1000 projects". The org has 49 live and 951 deleted projects, so deleted projects count.
- Rename: `amte2e-p11231341-identity-tgt` was renamed `amte2e-pool-01`. Its default team became `amte2e-pool-01 Team` and its default repo `amte2e-pool-01` on their own.
- Destroy: a work item was created, deleted and purged from the recycle bin.
- Process switch: per-run process to `amte2e-agile` worked. All 60 `amte2e*` processes in the target org derive from Agile, so every switch is between processes with the same base.
- Claim: creating a team whose name already exists is refused; deleting it frees the name at once.
- Asserts: no `assert.ps1` uses a literal work item id.
- Projects per scenario: at most 6 in one org (`users`).

## Challenge dispositions (`.specify/reviews/challenge.md`)

- F1 settled by the live refusal above. F2 settled: the rename carries the default team and repo. F3 settled by FR-004 (full list, repo replaced) and FR-004a (inventory check). F4 settled by FR-007 (switch back on release) and SC-001 (process and field counts). F5: every process has the Agile base (checked). A switch onto a seed's process is confirmed by its work item types (FR-005); a refused switch makes that scenario not poolable, within SC-002's limit of 3. F6 settled by FR-003 and FR-003a (delete by id only if unchanged, re-check before destructive steps, renewal points named). The expiry is written in UTC by the claimant; machines keep time by NTP. F7 settled by FR-007a and the dead-local-process takeover in FR-003a. F8 settled by FR-010 (every eligible project pooled, counted from the seeds). F9 settled by FR-011 (no lease, no change in 24 hours). F10 settled live for destroy, rename and process switch. Repo, pipeline and other deletes are checked by the inventory proof. F11: rejected. It needs about 100 new projects per org, and the target org can create none until deleted projects age out (about 28 days). The pool needs none. F12 settled: no literal ids in the asserts, and the proof runs scenarios through reused projects. F13 settled by SC-001 and SC-002 above.

## Assumptions

- Ids of work items are org-wide, so a cleared project does not restart ids. No assert relies on them.
- The proof's machine-wide queue and lock stay; the lease covers the cross-machine case.

## Docs

- e2e-harness: `platform/specs/e2e-harness/cleanup.md`, `platform/specs/e2e-harness/scenarios.md`, `platform/specs/architecture.md` ("Validating Changes End to End").
