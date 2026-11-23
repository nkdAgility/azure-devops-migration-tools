# E2E scenarios

Each folder under `tools/e2e/scenarios/` seeds its own scratch projects on the test organisations, runs `Invoke-MigrationE2E.ps1` with one or more configs, and checks the target with an `assert.ps1`.

## Scenarios added by later work

- `sharedsteps-3256`: seeds a Shared Steps, a Shared Parameter and a Test Case that uses both. `config.json` migrates only the Test Case, and `assert.ps1` (default `-Expect Unresolved`, the shape the done-check proof runs) checks that both references keep their source ids and are reported (two warnings, two `reference-left-on-source` review rows, and the summary count). `config-all.json` then migrates all three, and `assert.ps1 -Expect Resolved` checks that the re-run points both references at the target copies. (#3256)
