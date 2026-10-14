Read `.github/copilot-instructions.md` and `.github/instructions/*.instructions.md`; they apply to Claude as well.

## Validating changes end to end

Unit tests are not enough for changes to migration behaviour, exit codes, validation or error handling. Prove the change by running the built tool against the test organisation, as described in "Validating Changes End to End" in `.github/copilot-instructions.md`:

- Test orgs `https://dev.azure.com/nkdagility-adomt-source` and `https://dev.azure.com/nkdagility-adomt-target`. The harness signs in with Microsoft Entra ID (`az login`); no credential is committed. Set `AMT_TEST_PAT` only to use a PAT instead, and never print it or send it anywhere else.
- Build `MigrationTools.ConsoleFull`, then run `tools/e2e/Invoke-MigrationE2E.ps1 -Config <scenario.json>`.
- Use your own scratch projects; never modify projects used by the automated tests.
- Run each scenario on `main` and on the change, cover failure paths as well as the happy path, and report the output and exit codes in the pull request.
- Live tests (`TestCategory=L2|L3`) need `MigrationTools__Tests__UseEntraId=true`, `MigrationTools__Tests__SourceCollection` and `MigrationTools__Tests__TargetCollection` set to the test orgs; without them they fail with 401 against `nkdagility-preview`.

## Definition of done (Martin)

1. Up to date with `main`. 2. Builds. 3. Tests pass. 4. A real end-to-end migration completes correctly on the test orgs. 5. Meets the architectural norms. 6. Meets the original ask. 7. Clean up anything created only for the work (scratch projects, temp worktrees, logs).

Then report: what was done, how it was tested, how the docs were updated (with `GenerateDocs.ps1`), confidence, and what was not done. Turn auto-merge on only when confidence is 90% or higher; otherwise make sure it is off.

## WinGet release policy

`build/releaseWingetPackage.ps1` keeps at most one open PR per package in microsoft/winget-pkgs: Production always submits, Preview at most once every 7 days (`ForceRelease` overrides), Canary never. Our own open PRs for the same package are closed as superseded after a successful submission. The real package IDs are `nkdAgility.AzureDevOpsMigrationTools` and `.Preview`.

