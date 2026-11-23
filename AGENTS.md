Read `.github/copilot-instructions.md` and `.github/instructions/*.instructions.md`; they apply to Codex as well.

## Validating changes end to end

Unit tests are not enough for changes to migration behaviour, exit codes, validation or error handling. Prove the change by running the built tool against the test organisation, as described in "Validating Changes End to End" in `.github/copilot-instructions.md`:

- Test orgs `https://dev.azure.com/nkdagility-adomt-source` and `https://dev.azure.com/nkdagility-adomt-target`. The harness signs in with Microsoft Entra ID (`az login`); no credential is committed. Set `AMT_TEST_PAT` only to use a PAT instead, and never print it or send it anywhere else.
- Build `MigrationTools.ConsoleFull`, then run `tools/e2e/Invoke-MigrationE2E.ps1 -Config <scenario.json>`.
- Use your own scratch projects; never modify projects used by the automated tests.
- Run each scenario on `main` and on the change, cover failure paths as well as the happy path, and report the output and exit codes in the pull request.
- Live tests (`TestCategory=L2|L3`) need `MigrationTools__Tests__UseEntraId=true`, `MigrationTools__Tests__SourceCollection` and `MigrationTools__Tests__TargetCollection` set to the test orgs; without them they fail with 401 against `nkdagility-preview`.

## Definition of done (Martin)

1. Up to date with `main`. 2. Builds. 3. Tests pass. 4. A real end-to-end migration completes correctly on the test orgs. This applies to a change to the product (`src/`, `tools/`, `build/`); a change only to process or docs files has no migration to prove and skips it. 5. Meets the architectural norms. 6. Meets the original ask. 7. Clean up anything created only for the work (scratch projects, temp worktrees, logs).

Then report: what was done, how it was tested, how the docs were updated (with `GenerateDocs.ps1`), confidence, and what was not done. Turn auto-merge on only when confidence is 90% or higher; otherwise make sure it is off.

## The worker's Purpose decides scope (Martin)

A worker delivers one outcome, its Purpose, written at the top of the item's record, `specs/<issue number>-<name>/item.md`, with the `paths` it may edit.

- Asked for something outside it, answer: "This worker is working on {Purpose}; {the ask} is outside it. Do you want to add it to my scope?" Act only on a yes, then add it to the Purpose.
- Hand back only what is outside the Purpose or an extra you can finish without. It goes on the item as a proposed follow-up.
- A bug or a required refactor is never handed back: the Definition of Done places it.

## Draft until ready (Martin)

A pull request is a draft while anything it needs is not in it: an uncommitted or unpushed change on its branch, a shared file not yet copied in, or an open check or rule. It is marked ready only when all of these are done. Any agent that edits a file on a branch with a pull request marks that pull request draft first.

## Pushing (Martin)

Workers push their own item branch and open its draft pull request without asking. Merging still needs Martin.

## Azure DevOps scope (Martin)

Ignore `dev.azure.com/nkdagility/AzureDevOps-Tools`: never query it or use it to decide anything. Releases run from `.github/workflows/main.yml`; the legacy `build/azure-pipeline.yml` was removed in October 2026.

## WinGet release policy

`build/releaseWingetPackage.ps1` keeps at most one open PR per package in microsoft/winget-pkgs: Production always submits, Preview at most once every 7 days (`ForceRelease` overrides), Canary never. Our own open PRs for the same package are closed as superseded after a successful submission. The real package IDs are `nkdAgility.AzureDevOpsMigrationTools` and `.Preview`.

`wingetcreate update` copies the locale text from the previous manifest in microsoft/winget-pkgs, not from this repo, so the script overwrites `ShortDescription`/`Description` with the values from `src/MigrationTools.WinGet/*.locale.en-US.yaml` before `wingetcreate submit`. To test locally, run it with `-DryRun` and `DOTNET_ROLL_FORWARD=Major` (wingetcreate needs .NET 9). Manifest validation fails on long paths such as the scratchpad.

## Issue status comment

Each issue being worked on has one status comment, the one that starts "I'm working on this".
Update it only with the nkda-engineering-delivery stage-gate tool, from the repository root:

    Test-StageGate.ps1 -Package <issue number> -Status "<the current status, written for the customer>"

`-Status` replaces the current status and keeps the comment's hidden tags and its History. Never edit
or delete the status comment with `gh` directly; the plugin's hook refuses it. Other comments, PR
comments and review replies are unaffected.
