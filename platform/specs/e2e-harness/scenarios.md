# End-to-end scenarios

Each folder under `tools/e2e/scenarios/` is one scenario run against the test organisations with
`tools/e2e/Invoke-MigrationE2E.ps1`. A scenario seeds its own scratch projects (`amte2e-*`), runs the built tool with
its configs, and checks the target.

## Single-run scenarios

A scenario with `seed.ps1`, `config.json` and `assert.ps1`, whose seed takes `-Src` and `-Tgt`, is run by the
repository's proof (`.specify/memory/proof.ps1`) in projects named for that run.

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

## Two-config scenarios

### stringmanip-3258

`StringManipulatorTool`'s built-in default removes only what SOAP cannot save.

- `seed.ps1`: projects `amte2e-3258-src`, `-tgt` and `-tgtu`; one Bug whose Title and Found In hold letters outside the
  BMP and emoji, and one Bug whose Found In holds U+FFFE.
- `config.json` (default, `Manipulators` empty): exits 0; `assert.ps1` checks both Bugs migrated, the letters and emoji
  kept, and U+FFFE removed.
- `config-user.json` (its own manipulator removes `W`): exits 1; `assert.ps1 -Config config-user.json` checks the
  manipulator ran on Title and Found In and the U+FFFE Bug was not migrated.
