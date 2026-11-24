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

## Outcome checks

- e2e-harness, target and source orgs: a project taken from the pool is empty by inventory, on the requested process, renamed, and after release is `amte2e-pool-NN` on `amte2e-agile` with no lease team; pass = exit 0 and every case PASS: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Cycle`
- e2e-harness, target org: two takes started at once never get the same project, over 10 trials; pass = exit 0 and 10/10 distinct: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Race -Trials 10`
- e2e-harness, target org: an expired lease and a dead local claimant's lease are taken over, a live lease is left alone, and a project left half-filled is cleared on take; pass = exit 0 and every case PASS: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Lease`
- e2e-harness, both orgs: the adopt step renames eligible projects into the pool, creates none, skips migrationSource1/migrationTarget1 and recently changed or leased projects, and a second run changes nothing; pass = exit 0 and live project count unchanged: `pwsh -NoProfile -File tools/e2e/Initialize-E2EPool.ps1 -WhatIf; pwsh -NoProfile -File tools/e2e/Initialize-E2EPool.ps1; pwsh -NoProfile -File tools/e2e/Initialize-E2EPool.ps1`
- e2e-harness, both orgs: a scenario (wicore) runs twice through the pool and passes; live, deleted, process and Custom.amte2e* field counts are the same before and after; pass = exit 0 and counts equal: `pwsh -NoProfile -File tools/e2e/Test-E2EPool.ps1 -Case Scenario -Scenario wicore -Runs 2`
- e2e-harness, cleanup: `-All` and per-issue prefix never delete a pool project, and the pool mode releases every lease; pass = exit 0 (Pester): `pwsh -NoProfile -Command "Invoke-Pester tools/e2e -CI"`
- product, src unchanged; pass = exit 0 and no output: `git diff --quiet origin/main -- src`
