# End-to-end scenarios

Each folder under `tools/e2e/scenarios/` is one scenario run against the test organisations with
`tools/e2e/Invoke-MigrationE2E.ps1`. A scenario seeds its own scratch projects (`amte2e-*`), runs the built tool with
its configs, and checks the target.

Scratch projects come from the scratch project pool (`amte2e-pool-NN`, see [cleanup.md](cleanup.md)): `New-E2EProject`
takes a pool project, clears it, puts it on the process the seed asks for and gives it the seed's name, and
`Remove-E2EProject` gives it back. No scenario creates or deletes a project, except `linkscope` and `wicore-fresh` (below).

## Single-run scenarios

A scenario with `seed.ps1`, `config.json` and `assert.ps1`, whose seed takes `-Src` and `-Tgt`, is run by the
repository's proof (`.specify/memory/proof.ps1`) in pool projects renamed for that run: seed, one migration with the
config pointed at those projects, then the assert. An assert may run further migrations itself, through
`Invoke-MigrationE2E.ps1` with the same config. The proof gives back each
scenario's projects before the next scenario, and again when it ends, even after a failure, then lists the pool
projects it used and that each was released. `-Only <scenario>` runs just that scenario; a name that cannot run fails the proof before it starts. Each run's id
ends in six random hex digits and a run id already in use is refused; a run deletes only the `-fresh` target it
recorded making (`AMTE2E_CREATED_LOG`), checked by id.

### rerun-3326

Re-run repairs on already migrated items (#3326). See `platform/specs/tfs-object-model/rerun.md`.

- **Seed**: a commit pushed to the target and source repositories, and one to the source only. Items: Closed Tasks
  with an unresolved and a resolvable commit link, a Closed User Story with a parent link, a Resolved User Story with
  an unresolved commit link, a Task the assert stamps as an earlier re-run's catch-up save would have, and a Task the
  assert makes genuinely wrong.
- **Assert**: after the first run, every current state-stamped value equals the source. It removes the links, then:
  - a re-run exits 0 and every current state-stamped value equals the source again, the stamped Task included;
  - a further re-run exits 0, updates nothing and saves no revision;
  - after a ClosedDate is changed in an ordinary revision, a re-run fails that item with "stamped instead of the
    source values", exits non-zero, and logs no stack trace.
- `-BuildOutput` runs the assert's migrations with another build, for example `main`'s, for comparison.

## Not poolable

A scenario that cannot run in a pool project is listed here with its reason (at most 3).

- `linkscope`: its X work item must be unreadable, which only a deleted project gives. Its source, target and fail-target
  projects come from the pool; its `*-gone` project is created and deleted in the source org with
  `New-E2EDisposableProject`.
- `wicore-fresh`: `wicore`'s seed and assertions, migrating into a brand-new target project (`*-fresh`, made with
  `New-E2EDisposableProject` and deleted by the proof after the scenario), so behaviour that differs on a new project is
  still caught. It is the one project a run makes in the target org; its source comes from the pool. It runs only
  with `AMTE2E_FRESH_PROJECTS=1` (the target org may have no room for a new project); without it the proof prints
  "wicore-fresh skipped: AMTE2E_FRESH_PROJECTS is not set" and `New-E2EDisposableProject` refuses any target-org project.

## Multi-step scenarios

A scenario whose behaviour shows only across several runs has one config per step (`step<n>-<what>.json`) and a seed with
`-Step`, and is run step by step. The proof lists it as not covered; its runs are reported on the pull request.

### testplans-3260

Re-running Plans and Suites into an existing target plan adds the Test Cases missing from its root suite.

- `seed.ps1 -Step 1`: projects `amte2e-3260-src`/`-tgt`; TC1 in the root suite and in static Suite A of plan
  "amte2e 3260 plan". `-Step 2`: TC2 added to the root suite and Suite A. `-Step 3`: TC3 added to the root suite only.
  `-StateDir` holds the ids between steps.
- Configs: `step2-testcases.json`, `step3-variables-configurations.json`, `step4-plans-suites.json`, the steps of the
  Migrating Plans and Suites guide.
- `assert.ps1 -Root <titles> -SuiteA <titles>`: the target plan exists once, and its root suite and Suite A hold exactly
  those Test Cases. Exits 1 otherwise.
- Order: step 1, then step 2, 3 and 4 configs, assert (TC1 / TC1); step 2, then step 2 and 4 configs, assert
  (TC1, TC2 / TC1, TC2); step 4 config again, same assert; step 3, then step 4 config (exits 1 and names TC3); step 2
  and 4 configs, assert (TC1, TC2, TC3 / TC1, TC2).
