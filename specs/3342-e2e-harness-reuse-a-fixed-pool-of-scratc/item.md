---
purpose: Each e2e scenario takes its scratch projects from a fixed pool that the harness keeps and clears between runs, instead of creating and deleting projects, so proofs stop using up the test orgs' 1,000-project limit.
paths:
  - tools/e2e/**
  - platform/specs/e2e-harness/**
  - platform/specs/architecture.md
  - .specify/memory/proof.ps1
  - specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/**
---

# #3342 e2e-harness - Reuse a fixed pool of scratch projects

Run: dc93c742

## Sources read
- `platform/specs/systems.md`: the `e2e-harness` system is `tools/e2e/`, so a change there must update `platform/specs/e2e-harness/`.
- `platform/specs/e2e-harness/cleanup.md`: `Remove-E2EObjects.ps1` deletes projects, then processes, then `Custom.amte2e*` fields. Ownership is by name prefix (`amt<issue>-`, `amte2e-<issue>-`, `-All`). Deleted projects stay in the recycle bin; hard delete is refused (`VS800076`).
- `platform/specs/e2e-harness/scenarios.md`: a single-run scenario has `seed.ps1` (taking `-Src` and `-Tgt`), `config.json` and `assert.ps1`. Multi-step scenarios are run by hand and the proof lists them as not covered.
- `platform/specs/architecture.md` ("Validating Changes End to End"): use your own scratch projects; never modify projects the automated tests use; the harness only touches `amte2e-*` and `amt<issue>-*` names.
- `tools/e2e/E2EAzDo.psm1`: `New-E2EProject` returns an existing project of that name without checking it is empty; `Remove-E2EProject` soft-deletes (this is what fills the recycle bin); `Assert-E2EName` limits what the harness may create or delete to those prefixes; `Wait-E2EOperation` polls.
- `tools/e2e/Remove-E2EObjects.ps1`: the cleanup flow (projects, recycle bin, processes, fields).
- `.specify/memory/proof.ps1`: each run names its projects `amte2e-<item>-p<run>-<scenario>-<param>` (new names every run), holds a machine-wide queue and lock so one proof runs at a time, and leaves the projects "for clean-up". Soft-deleted projects count toward the 1,000 limit until purged, about 28 days later.
- `.specify/memory/constitution.md`: still the unfilled template, so it states no rules. `platform/specs/rules.md` and `.github/instructions/src.instructions.md` apply to `src/` only, so none of them reaches `tools/e2e/`.
- The 30 scenario folders under `tools/e2e/scenarios/`: each seed creates two to four projects with `New-E2EProject` (some, such as `linkscope` and `git`, create more; `picklists` creates them in a loop; `linkscope` deletes one itself).
- The issue (#3342): 49 live and 951 deleted projects in the target org, 47 live in the source org. Not re-checked here, because this session could not call the Azure DevOps REST API.

## Scope
- In:
  - A pool of projects per test org with fixed, known names, kept by the harness.
  - `New-E2EProject` becomes "take a free pool project" and `Remove-E2EProject` becomes "clear and release it". Scenario seeds and the proof keep working with the project names they are given.
  - Clearing a project between runs removes what scenarios create: work items, teams, iterations and areas, queries, and process or field changes. Pipelines, repos, test plans and the other objects the scenarios seed (see the gaps below) are cleared too, or the scenario is listed as not poolable.
  - A claim on each pool project, so two proofs never share one, including proofs on different machines.
  - Taking the existing live `amte2e-*` projects into the pool, with no new project slots.
  - `Remove-E2EObjects.ps1` and `proof.ps1` follow the pool: no more per-run project names or leftover project lists.
  - Updating `platform/specs/e2e-harness/` and the "Validating Changes End to End" section of `platform/specs/architecture.md`.
- Out:
  - Any change to the migration tool (`src/`), so no product behaviour changes.
  - Changing which scenarios the proof selects, or the scenarios' assertions.
  - Purging the 951 projects already in the recycle bin (the service does that on its own schedule).
  - Projects the automated tests use (`TestCategory=L2|L3`).

## Rubber duck
Today every proof makes new projects named for its run, and the cleanup soft-deletes them. Azure DevOps keeps a deleted project for about 28 days and counts it toward the 1,000-project limit the whole time, so a busy month fills the target org (49 live plus 951 deleted) and no new project can be made. No proof can run until the recycle bin empties.

The change is a fixed set of projects that are never deleted. A seed asks the harness for a project instead of creating one, the harness hands out a free one, the scenario runs in it, and the harness empties it and puts it back. The number of projects then stays constant however many proofs run.

Gaps this showed:
1. **Which projects are in the pool.** The issue says the 49 live projects can be taken in. Nobody has said which they are. Some may be the automated tests' projects, which `architecture.md` says must not be modified. Emptying one of those would break the L2/L3 tests. Q-1.
2. **Names.** `Assert-E2EName` lets the harness touch only `amte2e-*` and `amt<issue>-*` names, and a few seeds hard-code names, for example `amte2e-git-src`. Pool names need a fixed form that passes it, such as `amte2e-pool-NN`, and the existing live projects have to be renamed or accepted under their current names. Q-2.
3. **Process.** A project has one process, and the seeds create projects on different processes (`amte2e-agile`, a per-scenario process, a NoBug process, a process with rules). A pool project cannot simply be reused by a scenario that needs a different one. The options are one pool per process, or changing a project's process by migrating it. Q-3.
4. **What "clear" means.** The issue lists work items, teams, iterations and areas, queries, and process or field changes. Seeds also create git repos and commits (`git`, `prlinks`), build and release pipelines and variable groups (`pipelines`, `pipelines-3312`), test plans (`testplans`), dashboards and boards. The list of what each scenario leaves behind has to come from reading every seed, and "clear" must cover it or the scenario is excluded. Work items are the hard one: deleting them only moves them to the recycle bin, and they keep their ids, so a scenario that expects ids, or the `ReflectedWorkItemId` values, to start fresh gets different results. Destroying them is a separate permission. Q-4.
5. **Seeds that expect an empty, new project.** `New-E2EProject` returns an existing project as it is. After pooling that is the intended behaviour, but only if clearing is complete. `linkscope` deletes a project on purpose to test a missing one, which cannot be done with a pool project. It needs its own solution, for example a name that never exists, since the test is the project being absent.
6. **Concurrency.** The proof's queue and lock live in the local temp folder, so they stop two proofs on one machine only. Two workers on different machines, or CI plus a worker, can still pick the same pool project. The claim has to live in the org, for example a tag or property on the project with the claimant and a time, with a release and an expiry for a crashed run. Q-5.
7. **Failure leaves a dirty project.** A crashed run leaves a claimed, half-filled project. The next claim must either clear it first or the run must fail visibly. It cannot assume a free project is empty.
8. **Pool size.** If a proof runs the 30 scenarios at once, it needs up to about 100 projects. The proof runs them one after another, so a proof needs only the largest scenario's count, though the lock makes it one proof at a time anyway. The pool must hold at least that, while the org has to stay under the limit with the 951 deleted still counting for about 28 days. Q-6.
9. **Cleanup script.** The per-issue prefix cleanup stops making sense for projects, since they no longer carry an issue number. Processes and fields still do, so the script keeps those and gets a "clear the pool" mode. `-All` should not delete the pool.
10. **Proof output.** It prints "scratch projects and processes left for clean-up". After the change it should print the pool projects used and that each was cleared.

## Checked live (worker, 2026-11-24, target org)
- Live projects: target 49 (`migrationTarget1` plus 48 `amte2e-*`/`amt3326-*`), source 47 (`migrationSource1` plus 46). Most are on per-run processes (`amte2e-<item>-p<run>-<scenario>-process`), 13 target and 12 source on `amte2e-agile`.
- Claim: a project property cannot hold a claim safely. The properties PATCH rejects the JSON-patch `test` op (`VS850022`), so there is no compare-and-set. Creating a team whose name already exists is refused, deleting it frees the name at once, and recreating it then succeeds. So a team named `amte2e-pool-lease` in the project is an atomic claim that every machine sees. Probed on `amte2e-p11231341-identity-tgt`; the team was removed afterwards.
- Process: `POST {project}/_apis/wit/projectprocessmigration` (7.1-preview.1, body `{ typeId }`) moved `amte2e-p11231341-identity-tgt` from its per-run process to `amte2e-agile`, confirmed through `System.ProcessTemplateType`. It works between inherited processes of the same base.
- L2/L3 tests: `src/TestShared/LiveEnvironmentPreflightTests.cs` names only `migrationSource1` and `migrationTarget1`; no test names an `amte2e-*` project.

## Architecture check
- `platform/specs/architecture.md`, "Projects: never modify projects used by the automated tests": settled. The tests use only `migrationSource1`/`migrationTarget1`, and the pool takes only names that pass `Assert-E2EName` (`amte2e-*`, `amt<issue>-*`), which excludes them.
- `Assert-E2EName` guard: settled without widening it. Pool projects are renamed to `amte2e-pool-NN`, which passes the guard, and every pool operation still goes through it.
- `platform/specs/e2e-harness/cleanup.md` ownership rules: settled by adding a pool row (a pool project is owned by whoever holds its lease team) and a "clear the pool" mode. `-All` releases and clears pool projects and never deletes them.
- `platform/specs/systems.md` and `architecture.md` (docs follow the system): the change touches `tools/e2e/`, so `platform/specs/e2e-harness/cleanup.md`, `scenarios.md` and the harness sections of `architecture.md` must change with it. Not a break yet; carried to the spec as required docs, and listed in the paths above.
- `.specify/memory/constitution.md`: the unfilled template, so there are no rules to check. `platform/specs/rules.md` covers `src/` only.
Open findings: 0

## Questions
- Q-1 Which live projects do the L2/L3 tests use? settled: only `migrationSource1`/`migrationTarget1` (see Checked live). They are outside the guard, so they never enter the pool.
- Q-2 May the existing live projects be renamed and emptied? settled: the brief for this item says the 49 live target projects (43 `amte2e-*`, 5 `amt3326-*`) may be used as the pool. Renaming and clearing is what the pool design does to them.
- Q-3 One pool per process, or migrate the process? settled: migrate. It is proven live, and one pool per process would need more projects than exist. A scenario whose process has a different base (not Agile) is out of reach of the pool and listed as not poolable.
- Q-4 Delete or destroy work items when clearing? settled by the worker: destroy (delete, then purge from the recycle bin). Ids are org-wide either way, so neither choice resets them. Destroying leaves nothing behind for later queries to trip on. Only one option survives, unless the harness identity lacks the permission; that is checked when building, and if so the spec changes.
- Q-5 Where does the claim live? settled: in the org, as a lease team `amte2e-pool-lease` in the project. It is atomic (proven live), and its description holds the claimant and an expiry, so a crashed run's claim can be taken over after it expires.
- Q-6 Pool size per org: settled in the spec from the largest scenario's project count (for the worker).
- Q-7 No command proves US1 scenario 4 (every pool project leased and unexpired: the take fails naming the org, claimants and expiries, and creates no project). Proving it live would mean leasing the whole pool and so stopping every other proof. Do you want a `-Case Exhausted` in `Test-E2EPool.ps1` that fakes the leases in a Pester mock, or is the Pester run in the cleanup line enough? settled by the worker: `-Case Exhausted` with the leases mocked in Pester. Proving it live would lease the whole pool and stop every other proof, so only the mock survives. It is part of the Pester line in Outcome checks.

## Outcome checks

- e2e-harness, target and source orgs: a project taken from the pool is empty by inventory, on the requested process, renamed, and after release is amte2e-pool-NN on amte2e-agile with no lease team; pass = exit 0 and every case PASS: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Cycle`
- e2e-harness, target org: two takes started at once never get the same project, over 10 trials; pass = exit 0 and 10/10 distinct: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Race -Trials 10`
- e2e-harness, target org: an expired lease and a dead local claimant's lease are taken over, a live lease is left alone, and a project left half-filled is cleared on take; pass = exit 0 and every case PASS: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Lease`
- e2e-harness, both orgs: the adopt step renames eligible projects into the pool, creates none, skips migrationSource1/migrationTarget1 and recently changed or leased projects, and a second run changes nothing; pass = exit 0 and live project count unchanged: `pwsh -NoProfile -File tools/e2e/Initialize-E2EPool.ps1 -WhatIf; if ($LASTEXITCODE) { exit 1 }; pwsh -NoProfile -File tools/e2e/Initialize-E2EPool.ps1; exit $LASTEXITCODE`
- e2e-harness, both orgs: a scenario (wicore) runs twice through the pool and passes; live, deleted, process and Custom.amte2e* field counts are the same before and after; pass = exit 0 and counts equal: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Scenario -Scenario wicore -Runs 2`
- e2e-harness, cleanup: -All and per-issue prefix never delete a pool project, and the pool mode releases every lease; pass = exit 0 (Pester): `pwsh -NoProfile -Command "Invoke-Pester tools/e2e -CI"`
- e2e-harness, both orgs: linkscope passes through the pool, its gone project made and deleted in the source org only, and the target org's live + deleted count unchanged; pass = exit 0 and every case PASS: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Linkscope`
- e2e-harness, proof: the proof names each pool project used and that it was released, and prints no per-run project name or "left for clean-up" line; pass = exit 0, output has pool projects used: and released:, and has no left for clean-up: `$o = pwsh -NoProfile -File .specify/memory/proof.ps1 | Out-String; if ($LASTEXITCODE -ne 0 -or $o -notmatch 'pool projects used:' -or $o -notmatch 'released:' -or $o -match 'left for clean-up') { exit 1 }`
- e2e-harness, cleanup: -All -WhatIf lists no pool project for deletion; pass = exit 0 and no amte2e-pool- line says delete: `$o = pwsh -NoProfile -File tools/e2e/Remove-E2EObjects.ps1 -All -WhatIf | Out-String; if ($LASTEXITCODE -ne 0 -or $o -match '(?i)delet\w*[^\r\n]*amte2e-pool-\d+') { exit 1 }`
- e2e-harness, docs: cleanup.md, scenarios.md and architecture.md describe the pool and lease; pass = exit 0 (each file matches): `foreach ($f in 'platform/specs/e2e-harness/cleanup.md','platform/specs/e2e-harness/scenarios.md','platform/specs/architecture.md') { if (-not (Select-String -Path $f -Pattern 'amte2e-pool' -Quiet)) { exit 1 } }`
- e2e-harness, build: the solution still builds; pass = exit 0: `dotnet build MigrationTools.sln`
- product, src unchanged; pass = exit 0 and no output: `git diff --quiet origin/main -- src`

## Spec

Working spec: `specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md` (deleted once reconciled into platform/specs/).

Spec presented: specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md sha256 822601652e1c35fd157a70a468c6f40906f1046c640c4aba61ef2a6be105d987

## Red team

# Challenge: #3342 e2e-harness reuses a fixed pool of scratch projects

Target: `specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md` and `item.md`. Checked against `tools/e2e/E2EAzDo.psm1` and `.specify/memory/proof.ps1`.

The stock clarify, checklist and analyze steps are not repeated. This attacks the idea.

### F1 The premise that deleted projects use up the limit is not checked
The whole case rests on soft-deleted projects counting toward the 1,000-project limit. `item.md` says "Not re-checked here" for the 49 live and 951 deleted figures. 49 + 951 = 1,000 fits the claim, but no one has seen a create refused for that reason. If the real limit counts only live projects, or the failure is something else (rate limit, name reservation after delete), the pool fixes nothing and costs a lot of machinery.
Settled by: one live `POST _apis/projects` on the target org, recording the exact refusal message and code, and the Microsoft limit wording for deleted projects.

### F2 Renaming a project does not rename what hangs off its old name
FR-006 renames the pool project to the seed's name. These names survive a rename: the default repo (created with the project's original name), the default team (`<original> Team`), and pipeline and dashboard names. Adopted projects keep their old per-run names, for example `amte2e-p11231341-git-src`, in those places. A scenario or assertion that looks up repo `<project>` or team `<project> Team` finds nothing or the wrong thing. "Remove repos except the default" also keeps a default repo that is named after a project that no longer exists.
Settled by: rename one live pool project, then read its default repo and default team names. If they stay, specify what the take does about them, or list the scenarios that depend on them as not poolable.

### F3 "Clear" is a hand-written list, and the default repo's contents are not on it
FR-004 clears work items, teams, nodes, queries, extra repos, pipelines, variable groups and test plans. The default repo is kept, so its branches, commits, tags, pull requests and policies carry over. `git` and `prlinks-3286` create exactly those. Also left behind: work item tags (project-scoped definitions, `tags` scenario), test runs and results, attachments, dashboards, boards and backlog settings, service connections, environments, wikis, project and repo permissions, and team iteration and area settings. SC-004 only checks the same list, so it cannot detect anything missing.
Settled by: a diff of "everything a fresh project holds" against a cleared one, from the REST surface (repos, refs, PRs, tags, dashboards, environments, security namespaces). Then either add each item to FR-004 or make the scenario not poolable. Also state that SC-004's check is that diff, not the list.

### F4 Release does not return the project to a process the cleanup can delete
FR-005 switches the project to the seed's process on take. FR-007 release clears and renames, but says nothing about the process. A project left on a per-scenario process holds it, so `Remove-E2EObjects.ps1` cannot delete that process, because Azure DevOps refuses to delete a process that has projects on it. The process list and the `Custom.amte2e*` field list then grow, which is the leak the item set out to stop, moved from projects to processes.
Settled by: switch the project back to `amte2e-agile` on release, and add a SC that the process count is unchanged after a run.

### F5 The process switch is proven for one case only
`item.md` proved a switch from a per-run inherited process to `amte2e-agile`, with an empty project. The pool needs the opposite direction too: `amte2e-agile` to a process with added fields, rules, a custom work item type, or the NoBug process. Migrating can be refused for custom work item types or states, and it is asynchronous with no stated timeout. FR-005's "confirm it by reading the project's process type" does not check that a custom process's fields and rules are active in the project.
Settled by: live switch of an empty pool project onto each distinct process the 30 seeds use, with the work item types read back. Record which are refused and list those scenarios as not poolable now, not during the build.

### F6 Taking over an expired lease can race, and renewal has no owner
US3 covers two takers creating the lease team. It does not cover two takers both finding the same expired lease. Taker A deletes the stale team and creates its own. Taker B, which read the stale team earlier, then deletes by name and removes A's new lease, and B's create succeeds. Both now hold the project, and the destroy-work-items clear runs under a live proof. The spec says renewal is "by the taker while it holds the project" but names no heartbeat. A scenario that outlasts 4 hours, or a hung machine that wakes up, gets its project cleared mid-run. The expiry is compared against each machine's own clock.
Settled by: delete the stale lease by team id and expected description, so a replaced lease is not deleted. Specify a renewal loop with its interval, and a taker re-checks that it still owns the lease before each destructive step. Use the server's time for expiry, for example the team creation date.

### F7 A crash blocks the pool for the length of the lease
A proof that dies leaves its projects leased for up to 4 hours. With a pool of 6 and a 4-project scenario, one crash leaves 2 free, and every later proof, including a retry of the same one, fails with FR-008 until the lease expires. The spec relies on "the lease expires" for crash recovery, but crashes are the common case for a harness under development. The same owner retrying its own crashed run is treated like a stranger.
Settled by: the proof releases in a `finally` and, on start, releases leases whose claimant is its own previous run on this machine. The claimant name should include machine and process id so a dead local run can be told from a live remote one. Also consider a shorter lease with renewal.

### F8 The pool is too small for the concurrency the lease exists to allow
FR-010 sets the size to the largest scenario plus a spare, giving 6 for "at most 4". US3 exists so that two proofs, for example CI and a worker, can run at once, which needs about 8 projects, and a third needs 12. The 2 spare do not cover another live proof. The claim "at most 4" is also unchecked: `linkscope` makes five `New-E2EProject`/`Remove-E2EProject` calls, `rerun-3299-crossproject` four, `git` three, and `picklists` creates projects in a loop with no stated bound. These counts are calls, not projects per org.
Settled by: count the distinct projects per org per scenario from the seeds (and the `picklists` loop bound). Size the pool as (the largest count) times (the number of proofs that may run at once), and say what that number is.

### F9 The adopt step renames projects that people or other runs are using now
FR-011 takes the first 6 live `amte2e-*`/`amt3326-*` per org, in name order, then clears them on first take. Those include `amt3326-*` projects that belong to an issue worker who may be working in them today, and `amte2e-*` projects of a proof running right now. Renaming one mid-run breaks it, and the first take then destroys its work items. The projects left over keep names that may collide with seed-hard-coded names (`amte2e-git-src`), so a take fails with "rename target already exists" for ever.
Settled by: adopt only projects whose newest work item or repo activity is older than a stated age, or that the owner lists by name. List the unadopted projects whose names equal a seed's hard-coded name, and decide what happens to them before adoption.

### F10 Destroying work items depends on a permission nobody has confirmed
Q-4 settles on destroy, and the spec ends with "if the harness cannot, this spec changes". That puts a hard dependency in the Assumptions of an approved spec. Rename of a project, process migration, and removing repos and pipelines each need their own permission too (project administrator or collection administrator), and the spec names only the work item one. The 49 live projects were created under possibly different identities.
Settled by: a live probe with the harness's own sign-in, in a pool candidate: destroy one work item, rename, migrate process, delete a repo and a pipeline. Record each result in the spec before approving it.

### F11 A simpler way to reach the same outcome: stable names per scenario, no rename
The pool needs a lease team, a rename, a process switch back and forth, and an adopt step, and each of these has a finding above (F2, F4, F5, F6, F9). The limit problem comes from creating and deleting projects with new names every run. Give each scenario project a fixed name (`amte2e-<scenario>-src`, `-tgt`), never delete it, and clear it in `New-E2EProject` when found. At roughly 2 to 4 projects over 30 scenarios that is under about 100 projects per org, far below 1,000, and the count stays constant. There is no rename, so repo and team names stay valid and hard-coded names such as `amte2e-git-src` work as they are. There is no process switch, since a scenario's project stays on its own process. `linkscope` is a name that never exists, as US6 already proposes. The lease team and the clear stay, so cross-machine safety and F3 are unchanged. The cost is about 100 permanent projects instead of 6, and a project is idle between uses.
Settled by: compare the two on the F2, F4, F5 and F9 failures. If the one-org count (for example 100 of 1,000, with the 951 deleted aging out) is acceptable to Martin, prefer stable names.

### F12 Ids and history assumptions are asserted, not checked
"Scenarios that need fresh ids already do not rely on this" is stated for 30 seeds without a check. Cleared projects also lose nothing the source side needs (`ReflectedWorkItemId` carries the source project name, which now differs per take), and a target project that used to be new now has had thousands of destroyed work items, which can slow queries and count against org limits for destroyed-item metadata.
Settled by: grep the 30 `assert.ps1` files for literal ids, revision counts, or project names in `ReflectedWorkItemId`, and run two scenarios back to back through a reused project to compare results.

### F13 Success is measured in ways the change can pass while the aim fails
SC-001 compares live and deleted counts before and after "any number of runs". It would pass even if processes, fields, repos or tags grow without bound (F3, F4), and it cannot be measured at all if an unrelated person deletes a project during the run. SC-002 says all scenarios pass "through the pool" but the acceptable not-poolable set is open ended, so a design that excludes most scenarios would pass.
Settled by: add a bound on the not-poolable list, for example at most 3 of the 30, and add the process and field counts to SC-001.


## Dispositions

Each finding is settled in spec.md, "Challenge dispositions", with the live checks in "Checked live before approval". F11 is rejected there with its reason.

Open findings: 0



Spec agreed: approved.
Spec version: specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md sha256 822601652e1c35fd157a70a468c6f40906f1046c640c4aba61ef2a6be105d987

Plan agreed: Approve plan

Building over the WIP limit: Martin, 2026-11-24, "go over the WIP limit, it's expedite". The board stays Specified until a Building slot frees (the workflow tool has no WIP override).

Spec revised (FR-014, US6, SC-001, SC-008: linkscope keeps a disposable source-org project). Spec agreed: Approve

## Spec

Working spec: `specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md` (deleted once reconciled into platform/specs/).

Spec presented: specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md sha256 d48c03c519689cc27e79af8c5d8aae16dd7e68ce36cf0ffaa6fe769c9245771e

## Red team

# Challenge: #3342 e2e-harness reuses a fixed pool of scratch projects

Target: `specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md` and `item.md`. Checked against `tools/e2e/E2EAzDo.psm1` and `.specify/memory/proof.ps1`.

The stock clarify, checklist and analyze steps are not repeated. This attacks the idea.

### F1 The premise that deleted projects use up the limit is not checked
The whole case rests on soft-deleted projects counting toward the 1,000-project limit. `item.md` says "Not re-checked here" for the 49 live and 951 deleted figures. 49 + 951 = 1,000 fits the claim, but no one has seen a create refused for that reason. If the real limit counts only live projects, or the failure is something else (rate limit, name reservation after delete), the pool fixes nothing and costs a lot of machinery.
Settled by: one live `POST _apis/projects` on the target org, recording the exact refusal message and code, and the Microsoft limit wording for deleted projects.

### F2 Renaming a project does not rename what hangs off its old name
FR-006 renames the pool project to the seed's name. These names survive a rename: the default repo (created with the project's original name), the default team (`<original> Team`), and pipeline and dashboard names. Adopted projects keep their old per-run names, for example `amte2e-p11231341-git-src`, in those places. A scenario or assertion that looks up repo `<project>` or team `<project> Team` finds nothing or the wrong thing. "Remove repos except the default" also keeps a default repo that is named after a project that no longer exists.
Settled by: rename one live pool project, then read its default repo and default team names. If they stay, specify what the take does about them, or list the scenarios that depend on them as not poolable.

### F3 "Clear" is a hand-written list, and the default repo's contents are not on it
FR-004 clears work items, teams, nodes, queries, extra repos, pipelines, variable groups and test plans. The default repo is kept, so its branches, commits, tags, pull requests and policies carry over. `git` and `prlinks-3286` create exactly those. Also left behind: work item tags (project-scoped definitions, `tags` scenario), test runs and results, attachments, dashboards, boards and backlog settings, service connections, environments, wikis, project and repo permissions, and team iteration and area settings. SC-004 only checks the same list, so it cannot detect anything missing.
Settled by: a diff of "everything a fresh project holds" against a cleared one, from the REST surface (repos, refs, PRs, tags, dashboards, environments, security namespaces). Then either add each item to FR-004 or make the scenario not poolable. Also state that SC-004's check is that diff, not the list.

### F4 Release does not return the project to a process the cleanup can delete
FR-005 switches the project to the seed's process on take. FR-007 release clears and renames, but says nothing about the process. A project left on a per-scenario process holds it, so `Remove-E2EObjects.ps1` cannot delete that process, because Azure DevOps refuses to delete a process that has projects on it. The process list and the `Custom.amte2e*` field list then grow, which is the leak the item set out to stop, moved from projects to processes.
Settled by: switch the project back to `amte2e-agile` on release, and add a SC that the process count is unchanged after a run.

### F5 The process switch is proven for one case only
`item.md` proved a switch from a per-run inherited process to `amte2e-agile`, with an empty project. The pool needs the opposite direction too: `amte2e-agile` to a process with added fields, rules, a custom work item type, or the NoBug process. Migrating can be refused for custom work item types or states, and it is asynchronous with no stated timeout. FR-005's "confirm it by reading the project's process type" does not check that a custom process's fields and rules are active in the project.
Settled by: live switch of an empty pool project onto each distinct process the 30 seeds use, with the work item types read back. Record which are refused and list those scenarios as not poolable now, not during the build.

### F6 Taking over an expired lease can race, and renewal has no owner
US3 covers two takers creating the lease team. It does not cover two takers both finding the same expired lease. Taker A deletes the stale team and creates its own. Taker B, which read the stale team earlier, then deletes by name and removes A's new lease, and B's create succeeds. Both now hold the project, and the destroy-work-items clear runs under a live proof. The spec says renewal is "by the taker while it holds the project" but names no heartbeat. A scenario that outlasts 4 hours, or a hung machine that wakes up, gets its project cleared mid-run. The expiry is compared against each machine's own clock.
Settled by: delete the stale lease by team id and expected description, so a replaced lease is not deleted. Specify a renewal loop with its interval, and a taker re-checks that it still owns the lease before each destructive step. Use the server's time for expiry, for example the team creation date.

### F7 A crash blocks the pool for the length of the lease
A proof that dies leaves its projects leased for up to 4 hours. With a pool of 6 and a 4-project scenario, one crash leaves 2 free, and every later proof, including a retry of the same one, fails with FR-008 until the lease expires. The spec relies on "the lease expires" for crash recovery, but crashes are the common case for a harness under development. The same owner retrying its own crashed run is treated like a stranger.
Settled by: the proof releases in a `finally` and, on start, releases leases whose claimant is its own previous run on this machine. The claimant name should include machine and process id so a dead local run can be told from a live remote one. Also consider a shorter lease with renewal.

### F8 The pool is too small for the concurrency the lease exists to allow
FR-010 sets the size to the largest scenario plus a spare, giving 6 for "at most 4". US3 exists so that two proofs, for example CI and a worker, can run at once, which needs about 8 projects, and a third needs 12. The 2 spare do not cover another live proof. The claim "at most 4" is also unchecked: `linkscope` makes five `New-E2EProject`/`Remove-E2EProject` calls, `rerun-3299-crossproject` four, `git` three, and `picklists` creates projects in a loop with no stated bound. These counts are calls, not projects per org.
Settled by: count the distinct projects per org per scenario from the seeds (and the `picklists` loop bound). Size the pool as (the largest count) times (the number of proofs that may run at once), and say what that number is.

### F9 The adopt step renames projects that people or other runs are using now
FR-011 takes the first 6 live `amte2e-*`/`amt3326-*` per org, in name order, then clears them on first take. Those include `amt3326-*` projects that belong to an issue worker who may be working in them today, and `amte2e-*` projects of a proof running right now. Renaming one mid-run breaks it, and the first take then destroys its work items. The projects left over keep names that may collide with seed-hard-coded names (`amte2e-git-src`), so a take fails with "rename target already exists" for ever.
Settled by: adopt only projects whose newest work item or repo activity is older than a stated age, or that the owner lists by name. List the unadopted projects whose names equal a seed's hard-coded name, and decide what happens to them before adoption.

### F10 Destroying work items depends on a permission nobody has confirmed
Q-4 settles on destroy, and the spec ends with "if the harness cannot, this spec changes". That puts a hard dependency in the Assumptions of an approved spec. Rename of a project, process migration, and removing repos and pipelines each need their own permission too (project administrator or collection administrator), and the spec names only the work item one. The 49 live projects were created under possibly different identities.
Settled by: a live probe with the harness's own sign-in, in a pool candidate: destroy one work item, rename, migrate process, delete a repo and a pipeline. Record each result in the spec before approving it.

### F11 A simpler way to reach the same outcome: stable names per scenario, no rename
The pool needs a lease team, a rename, a process switch back and forth, and an adopt step, and each of these has a finding above (F2, F4, F5, F6, F9). The limit problem comes from creating and deleting projects with new names every run. Give each scenario project a fixed name (`amte2e-<scenario>-src`, `-tgt`), never delete it, and clear it in `New-E2EProject` when found. At roughly 2 to 4 projects over 30 scenarios that is under about 100 projects per org, far below 1,000, and the count stays constant. There is no rename, so repo and team names stay valid and hard-coded names such as `amte2e-git-src` work as they are. There is no process switch, since a scenario's project stays on its own process. `linkscope` is a name that never exists, as US6 already proposes. The lease team and the clear stay, so cross-machine safety and F3 are unchanged. The cost is about 100 permanent projects instead of 6, and a project is idle between uses.
Settled by: compare the two on the F2, F4, F5 and F9 failures. If the one-org count (for example 100 of 1,000, with the 951 deleted aging out) is acceptable to Martin, prefer stable names.

### F12 Ids and history assumptions are asserted, not checked
"Scenarios that need fresh ids already do not rely on this" is stated for 30 seeds without a check. Cleared projects also lose nothing the source side needs (`ReflectedWorkItemId` carries the source project name, which now differs per take), and a target project that used to be new now has had thousands of destroyed work items, which can slow queries and count against org limits for destroyed-item metadata.
Settled by: grep the 30 `assert.ps1` files for literal ids, revision counts, or project names in `ReflectedWorkItemId`, and run two scenarios back to back through a reused project to compare results.

### F13 Success is measured in ways the change can pass while the aim fails
SC-001 compares live and deleted counts before and after "any number of runs". It would pass even if processes, fields, repos or tags grow without bound (F3, F4), and it cannot be measured at all if an unrelated person deletes a project during the run. SC-002 says all scenarios pass "through the pool" but the acceptable not-poolable set is open ended, so a design that excludes most scenarios would pass.
Settled by: add a bound on the not-poolable list, for example at most 3 of the 30, and add the process and field counts to SC-001.


## Dispositions

Each finding is settled in spec.md, "Challenge dispositions", with the live checks in "Checked live before approval". F11 is rejected there with its reason.

Open findings: 0



Spec agreed: Approve
Spec version: specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md sha256 d48c03c519689cc27e79af8c5d8aae16dd7e68ce36cf0ffaa6fe769c9245771e

## Spec

Working spec: `specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md` (deleted once reconciled into platform/specs/).

Spec presented: specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md sha256 ba52a227862945b18c3d3a61fa556c449a61f993b899398423981b9922f4ffd1

## Red team

# Challenge: #3342 e2e-harness reuses a fixed pool of scratch projects

Target: `specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md` and `item.md`. Checked against `tools/e2e/E2EAzDo.psm1` and `.specify/memory/proof.ps1`.

The stock clarify, checklist and analyze steps are not repeated. This attacks the idea.

### F1 The premise that deleted projects use up the limit is not checked
The whole case rests on soft-deleted projects counting toward the 1,000-project limit. `item.md` says "Not re-checked here" for the 49 live and 951 deleted figures. 49 + 951 = 1,000 fits the claim, but no one has seen a create refused for that reason. If the real limit counts only live projects, or the failure is something else (rate limit, name reservation after delete), the pool fixes nothing and costs a lot of machinery.
Settled by: one live `POST _apis/projects` on the target org, recording the exact refusal message and code, and the Microsoft limit wording for deleted projects.

### F2 Renaming a project does not rename what hangs off its old name
FR-006 renames the pool project to the seed's name. These names survive a rename: the default repo (created with the project's original name), the default team (`<original> Team`), and pipeline and dashboard names. Adopted projects keep their old per-run names, for example `amte2e-p11231341-git-src`, in those places. A scenario or assertion that looks up repo `<project>` or team `<project> Team` finds nothing or the wrong thing. "Remove repos except the default" also keeps a default repo that is named after a project that no longer exists.
Settled by: rename one live pool project, then read its default repo and default team names. If they stay, specify what the take does about them, or list the scenarios that depend on them as not poolable.

### F3 "Clear" is a hand-written list, and the default repo's contents are not on it
FR-004 clears work items, teams, nodes, queries, extra repos, pipelines, variable groups and test plans. The default repo is kept, so its branches, commits, tags, pull requests and policies carry over. `git` and `prlinks-3286` create exactly those. Also left behind: work item tags (project-scoped definitions, `tags` scenario), test runs and results, attachments, dashboards, boards and backlog settings, service connections, environments, wikis, project and repo permissions, and team iteration and area settings. SC-004 only checks the same list, so it cannot detect anything missing.
Settled by: a diff of "everything a fresh project holds" against a cleared one, from the REST surface (repos, refs, PRs, tags, dashboards, environments, security namespaces). Then either add each item to FR-004 or make the scenario not poolable. Also state that SC-004's check is that diff, not the list.

### F4 Release does not return the project to a process the cleanup can delete
FR-005 switches the project to the seed's process on take. FR-007 release clears and renames, but says nothing about the process. A project left on a per-scenario process holds it, so `Remove-E2EObjects.ps1` cannot delete that process, because Azure DevOps refuses to delete a process that has projects on it. The process list and the `Custom.amte2e*` field list then grow, which is the leak the item set out to stop, moved from projects to processes.
Settled by: switch the project back to `amte2e-agile` on release, and add a SC that the process count is unchanged after a run.

### F5 The process switch is proven for one case only
`item.md` proved a switch from a per-run inherited process to `amte2e-agile`, with an empty project. The pool needs the opposite direction too: `amte2e-agile` to a process with added fields, rules, a custom work item type, or the NoBug process. Migrating can be refused for custom work item types or states, and it is asynchronous with no stated timeout. FR-005's "confirm it by reading the project's process type" does not check that a custom process's fields and rules are active in the project.
Settled by: live switch of an empty pool project onto each distinct process the 30 seeds use, with the work item types read back. Record which are refused and list those scenarios as not poolable now, not during the build.

### F6 Taking over an expired lease can race, and renewal has no owner
US3 covers two takers creating the lease team. It does not cover two takers both finding the same expired lease. Taker A deletes the stale team and creates its own. Taker B, which read the stale team earlier, then deletes by name and removes A's new lease, and B's create succeeds. Both now hold the project, and the destroy-work-items clear runs under a live proof. The spec says renewal is "by the taker while it holds the project" but names no heartbeat. A scenario that outlasts 4 hours, or a hung machine that wakes up, gets its project cleared mid-run. The expiry is compared against each machine's own clock.
Settled by: delete the stale lease by team id and expected description, so a replaced lease is not deleted. Specify a renewal loop with its interval, and a taker re-checks that it still owns the lease before each destructive step. Use the server's time for expiry, for example the team creation date.

### F7 A crash blocks the pool for the length of the lease
A proof that dies leaves its projects leased for up to 4 hours. With a pool of 6 and a 4-project scenario, one crash leaves 2 free, and every later proof, including a retry of the same one, fails with FR-008 until the lease expires. The spec relies on "the lease expires" for crash recovery, but crashes are the common case for a harness under development. The same owner retrying its own crashed run is treated like a stranger.
Settled by: the proof releases in a `finally` and, on start, releases leases whose claimant is its own previous run on this machine. The claimant name should include machine and process id so a dead local run can be told from a live remote one. Also consider a shorter lease with renewal.

### F8 The pool is too small for the concurrency the lease exists to allow
FR-010 sets the size to the largest scenario plus a spare, giving 6 for "at most 4". US3 exists so that two proofs, for example CI and a worker, can run at once, which needs about 8 projects, and a third needs 12. The 2 spare do not cover another live proof. The claim "at most 4" is also unchecked: `linkscope` makes five `New-E2EProject`/`Remove-E2EProject` calls, `rerun-3299-crossproject` four, `git` three, and `picklists` creates projects in a loop with no stated bound. These counts are calls, not projects per org.
Settled by: count the distinct projects per org per scenario from the seeds (and the `picklists` loop bound). Size the pool as (the largest count) times (the number of proofs that may run at once), and say what that number is.

### F9 The adopt step renames projects that people or other runs are using now
FR-011 takes the first 6 live `amte2e-*`/`amt3326-*` per org, in name order, then clears them on first take. Those include `amt3326-*` projects that belong to an issue worker who may be working in them today, and `amte2e-*` projects of a proof running right now. Renaming one mid-run breaks it, and the first take then destroys its work items. The projects left over keep names that may collide with seed-hard-coded names (`amte2e-git-src`), so a take fails with "rename target already exists" for ever.
Settled by: adopt only projects whose newest work item or repo activity is older than a stated age, or that the owner lists by name. List the unadopted projects whose names equal a seed's hard-coded name, and decide what happens to them before adoption.

### F10 Destroying work items depends on a permission nobody has confirmed
Q-4 settles on destroy, and the spec ends with "if the harness cannot, this spec changes". That puts a hard dependency in the Assumptions of an approved spec. Rename of a project, process migration, and removing repos and pipelines each need their own permission too (project administrator or collection administrator), and the spec names only the work item one. The 49 live projects were created under possibly different identities.
Settled by: a live probe with the harness's own sign-in, in a pool candidate: destroy one work item, rename, migrate process, delete a repo and a pipeline. Record each result in the spec before approving it.

### F11 A simpler way to reach the same outcome: stable names per scenario, no rename
The pool needs a lease team, a rename, a process switch back and forth, and an adopt step, and each of these has a finding above (F2, F4, F5, F6, F9). The limit problem comes from creating and deleting projects with new names every run. Give each scenario project a fixed name (`amte2e-<scenario>-src`, `-tgt`), never delete it, and clear it in `New-E2EProject` when found. At roughly 2 to 4 projects over 30 scenarios that is under about 100 projects per org, far below 1,000, and the count stays constant. There is no rename, so repo and team names stay valid and hard-coded names such as `amte2e-git-src` work as they are. There is no process switch, since a scenario's project stays on its own process. `linkscope` is a name that never exists, as US6 already proposes. The lease team and the clear stay, so cross-machine safety and F3 are unchanged. The cost is about 100 permanent projects instead of 6, and a project is idle between uses.
Settled by: compare the two on the F2, F4, F5 and F9 failures. If the one-org count (for example 100 of 1,000, with the 951 deleted aging out) is acceptable to Martin, prefer stable names.

### F12 Ids and history assumptions are asserted, not checked
"Scenarios that need fresh ids already do not rely on this" is stated for 30 seeds without a check. Cleared projects also lose nothing the source side needs (`ReflectedWorkItemId` carries the source project name, which now differs per take), and a target project that used to be new now has had thousands of destroyed work items, which can slow queries and count against org limits for destroyed-item metadata.
Settled by: grep the 30 `assert.ps1` files for literal ids, revision counts, or project names in `ReflectedWorkItemId`, and run two scenarios back to back through a reused project to compare results.

### F13 Success is measured in ways the change can pass while the aim fails
SC-001 compares live and deleted counts before and after "any number of runs". It would pass even if processes, fields, repos or tags grow without bound (F3, F4), and it cannot be measured at all if an unrelated person deletes a project during the run. SC-002 says all scenarios pass "through the pool" but the acceptable not-poolable set is open ended, so a design that excludes most scenarios would pass.
Settled by: add a bound on the not-poolable list, for example at most 3 of the 30, and add the process and field counts to SC-001.


## Dispositions

Each finding is settled in spec.md, "Challenge dispositions", with the live checks in "Checked live before approval". F11 is rejected there with its reason.

Open findings: 0



Spec revised (FR-004, FR-004a, FR-004b: Shared Steps and Shared Parameters kept as leftovers).
Spec agreed: Approve
Spec version: specs/3342-e2e-harness-reuse-a-fixed-pool-of-scratc/spec.md sha256 ba52a227862945b18c3d3a61fa556c449a61f993b899398423981b9922f4ffd1

## Output done evidence

Commit a12dd8c7, 2026-11-24, harness sign-in (az login, Entra ID).

- main, before: main's New-E2EProject on the target org fails: "Failed to add a project as this organization already has 1000 projects". EXIT CODE: 1.
- Pool adopted by rename, nothing created or deleted: target 48 (amte2e-pool-01..48), source 46; live 49 -> 49 and 47 -> 47. migrationSource1/migrationTarget1 skipped ("used by the automated tests").
- Test-E2EPool.ps1 -Case Cycle: 14 of 14 PASS, EXIT CODE: 0 (both orgs: empty by inventory, process switched, renamed with team and repo, given back to amte2e-pool-NN on amte2e-agile, no lease, counts unchanged).
- -Case Race -Trials 10: 10 of 10 PASS, EXIT CODE: 0 (first run 9 of 10: a dropped connection, fixed by retrying requests that got no response, never a POST).
- -Case Lease: 6 of 6 PASS, EXIT CODE: 0 (held left alone; expired and orphaned taken over; half-filled project cleared on take).
- -Case Scenario -Scenario wicore -Runs 2: 4 of 4 PASS, EXIT CODE: 0; live, deleted, process and Custom.amte2e* field counts equal before and after in both orgs.
- Pester (tools/e2e): 55 passed, 0 failed.
- done-checks.ps1: build OK; dotnet test 838 passed, 0 failed, 4 skipped (L2/L3 live on the test orgs); proof: all 10 runnable scenarios passed through the pool, "pool projects used: nkdagility-adomt-source/amte2e-pool-25, nkdagility-adomt-target/amte2e-pool-43", "released: ... (cleared)". DONE-CHECKS EXIT CODE: 0.
- linkscope by hand (not run by the proof): 15 of 15 PASS, ASSERT EXIT CODE: 0; gone project made and deleted in the source org only; its 3 pool projects given back.
- Found while building and settled with Martin: linkscope keeps a disposable source-org project (FR-014); Shared Steps and Shared Parameters, which no REST API deletes, are kept as leftovers (FR-004b).
- Cleanup: Remove-E2EObjects.ps1 -Prefix amte2e-3342- deleted this item's 11 scratch processes, EXIT CODE: 0. Target org afterwards: live 49, deleted 951, pool 48 free. Source: live 47, deleted 809 (807 + 2 linkscope gone projects).
- Docs updated: e2e-harness: `platform/specs/e2e-harness/cleanup.md`: "The scratch project pool"; e2e-harness: `platform/specs/e2e-harness/scenarios.md`: "Not poolable"; e2e-harness: `platform/specs/architecture.md`: "Scratch project pool".

Not covered: the multi-step and not-covered scenarios (testplans, users, git and others the proof lists as not covered) were not run through the pool; their seeds use New-E2EProject unchanged. A concurrent proof from a second machine was not run; Race covers two processes on one machine against the same org-side claim.

Confidence: 88%
Reasons:
- Every live check, the full test suite and the proof pass on this commit, and the target org's counts are unchanged.
- Two spec gaps surfaced only on live data (linkscope, Shared Steps); others of that kind may remain in the scenarios the proof does not cover.
- Not proven across two machines, though the claim is enforced by the service, not the machine.

## Docs updated

- e2e-harness: `platform/specs/e2e-harness/cleanup.md`: The scratch project pool
- e2e-harness: `platform/specs/e2e-harness/scenarios.md`: Not poolable

Outcome checks started for #3342 at 7c9e012135da8d1ea071596ecbdb59ee26762707 attempt 661820e539ce:
Outcome checks failed for #3342 at 7c9e012135da8d1ea071596ecbdb59ee26762707 attempt 661820e539ce: Outcome checks failed: e2e-harness, target org: an expired lease and a dead local claimant's lease are taken over, a live lease is left alone, and a project left half-filled is cleared on take; pass = exit 0 and every case PASS, e2e-harness, both orgs: a scenario (wicore) runs twice through the pool and passes; live, deleted, process and Custom.amte2e* field counts are the same before and after; pass = exit 0 and counts equal, e2e-harness, cleanup, e2e-harness, both orgs: linkscope passes through the pool, its gone project made and deleted in the source org only, and the target org's live + deleted count unchanged; pass = exit 0 and every case PASS, e2e-harness, proof: the proof names each pool project used and that it was released, and prints no per-run project name or "left for clean-up" line; pass = exit 0, output has `pool projects used, e2e-harness, cleanup
Outcome checks started for #3342 at dea070c1305e2c462674ac12fb0bf873f7b57261 attempt 48073b8bdcaa:
Outcome checks failed for #3342 at dea070c1305e2c462674ac12fb0bf873f7b57261 attempt 48073b8bdcaa: Outcome checks failed: e2e-harness, cleanup, e2e-harness, proof: the proof names each pool project used and that it was released, and prints no per-run project name or "left for clean-up" line; pass = exit 0, output has `pool projects used, e2e-harness, cleanup
Outcome checks started for #3342 at 29b2b79e2fc6e7a46ca6195556cc97392aac054d attempt 361612bf1655:
Outcome checks failed for #3342 at 29b2b79e2fc6e7a46ca6195556cc97392aac054d attempt 361612bf1655: Outcome checks failed: e2e-harness, proof: the proof names each pool project used and that it was released, and prints no per-run project name or "left for clean-up" line; pass = exit 0, output has pool projects used: and released:, and has no left for clean-up
Outcome checks started for #3342 at 62fc7ff61308f01fd562904c25e86dfc264a2b58 attempt 40f3f98fd1ca:

## Outcome done

Outcome checks passed for #3342 at 62fc7ff6, attempt 40f3f98fd1ca, all checks in one run: Cycle 14/14, Race 10/10, Lease 6/6, Scenario (wicore twice) 4/4 with counts equal, Linkscope 4/4, Pester 55 passed, proof exit 0 with "pool projects used:" and "released: ... (cleared)" and no "left for clean-up" line.

Confidence: 88%
Reasons:
- Every outcome check passed on the same commit against both test orgs, with live and deleted counts unchanged.
- Scenarios the proof does not cover were not run through the pool.
- Not proven across two machines; the claim is enforced by the service.
