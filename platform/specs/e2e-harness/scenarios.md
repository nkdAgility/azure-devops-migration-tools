# End-to-end scenarios

Each folder under `tools/e2e/scenarios/` is one scenario. A scenario with a `seed.ps1`, a `config.json` and an
`assert.ps1` whose seed takes `-Src` and `-Tgt` runs in `.specify/memory/proof.ps1`, in scratch projects of its own:
seed, one migration with the config pointed at the scratch projects, then the assert. An assert may run further
migrations itself, through `Invoke-MigrationE2E.ps1` with the same config.

## rerun-3326

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
