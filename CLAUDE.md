Read `.github/copilot-instructions.md` and `.github/instructions/*.instructions.md`; they apply to Claude as well.

## Validating changes end to end

Unit tests are not enough for changes to migration behaviour, exit codes, validation or error handling. Prove the change by running the built tool against the test organisation, as described in "Validating Changes End to End" in `.github/copilot-instructions.md`:

- Test orgs `https://dev.azure.com/nkdagility-adomt-source` and `https://dev.azure.com/nkdagility-adomt-target`. The harness signs in with Microsoft Entra ID (`az login`); no credential is committed. Set `AMT_TEST_PAT` only to use a PAT instead, and never print it or send it anywhere else.
- Build `MigrationTools.ConsoleFull`, then run `tools/e2e/Invoke-MigrationE2E.ps1 -Config <scenario.json>`.
- Use your own scratch projects; never modify projects used by the automated tests.
- Run each scenario on `main` and on the change, cover failure paths as well as the happy path, and report the output and exit codes in the pull request.
