# Bug Assessment: Test Case step and parameter references silently left on source IDs

- **Slug**: sharedsteps-unresolved-refs
- **Created**: 2026-11-21
- **Source**: https://github.com/nkdAgility/azure-devops-migration-tools/issues/3256 (host: github.com, allowlisted; read with `gh issue view`)
- **Verdict**: valid
- **Severity**: medium

## Report (summarized)

Issue #3256: `TfsWorkItemLinkTool.MigrateSharedSteps` and `MigrateSharedParameters` only rewrite a Test Case's step (`ref="<id>"`) and parameter references when the referenced Shared Steps or Shared Parameter is found in the target. When it isn't, the reference keeps the **source** ID and nothing is logged. The default `WIQLQuery` excludes `'Shared Steps','Shared Parameter'`, so a default run that includes Test Cases always hits this. The issue proposes a warning per unresolved reference and a count in the end-of-run summary, and asks whether to remove the types from the default exclusion.

## Symptom

After a migration in which the referenced Shared Steps / Shared Parameter were not migrated, the target Test Case's `Microsoft.VSTS.TCM.Steps` and `Microsoft.VSTS.TCM.LocalDataSource` hold source work item IDs, and the run exits 0 without saying so. Expected: each unresolved reference is reported, with the Test Case and the referenced source ID, and counted in the summary.

## Reproduction

Reproduced on main (56bab21f), 2026-11-20, on the test orgs with scratch projects `amte2e-3256-src` / `amte2e-3256-tgt` (since deleted):

1. Seed with `tools/e2e/scenarios/sharedsteps/seed.ps1 -Src amte2e-3256-src -Tgt amte2e-3256-tgt`: Shared Steps 5591, Shared Parameter 5592, Test Case 5593 using both.
2. Use the `sharedsteps` scenario config with the project names swapped and `WIQLQuery` narrowed to `[System.WorkItemType] IN ('Test Case')`.
3. Run `tools/e2e/Invoke-MigrationE2E.ps1 -Config <config>`.

Observed: exit 0, `1 created, 0 failed`. Target Test Case 9344 has `<compref id="2" ref="5591" />` and `"dataSetId":5592`, `"sharedParameterDataSetIds":[5592]`; neither ID exists in the target. The log has only the two generic link skips (`SharedStepReferencedBy-Reverse` / `SharedParameterReferencedBy-Reverse ... target WI has not been migrated`, review `link-skipped: 2`). Nothing mentions the field references.

## Suspected Code Paths

- `src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemLinkTool.cs:234-249` (`MigrateSharedSteps`): `if (matchingTargetSharedStep != null)` with no else, so an unmatched reference is skipped silently.
- `src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemLinkTool.cs:293-301` (`MigrateSharedParameters`): same shape; an unmatched parameter never enters `sourceToTarget`, and nothing is reported.
- `src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemLinkTool.cs:257-276` (`ReadableSourceItems`): reports only items missing from the **source** (TF26198), which is the pattern to follow.
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs:1322-1323`: the call site.
- `WorkItemReviewLog` / `WorkItemReviewCategory`: where the summary count comes from.

## Root Cause Hypothesis

The "target not found" case was never handled: both methods only act when a target match exists, and the only reporting added so far (`ReadableSourceItems`) covers a missing source item. Confidence: high (code reading plus the reproduction above).

## Proposed Remediation

**Preferred**: in `TfsWorkItemLinkTool`, when `FindReflectedWorkItemByReflectedWorkItemId` returns null for a Shared Steps or Shared Parameter, log a warning naming the kind, the source referenced ID, the Test Case source and target IDs and the consequence ("its step reference keeps the source id" / "its data source reference keeps the source id"), and record a `WorkItemReviewLog` entry in a **new category** (approved: for example `reference-unresolved`) with the action "Migrate the Shared Steps / Shared Parameter, then re-run". Put the warning and the record in one private helper beside `ReadableSourceItems`. The field keeps the source ID (approved: keep and report, don't clear). Object model only; no REST.

**Alternatives**:
- Reuse `link-skipped`: no new category, but it mixes the field-reference consequence with the link skip already logged for the same item. Not chosen.
- Remove the types from the default WIQL: separate follow-up (approved), because it changes what a default run migrates.

**Files likely to change**:
- `src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemLinkTool.cs`
- `WorkItemReviewCategory` (new category) and its summary wording
- `src/MigrationTools.Clients.TfsObjectModel.Tests/**`
- `tools/e2e/scenarios/sharedsteps-3256/**` (new)
- docs, via `GenerateDocs.ps1`, and the Migrating Plans and Suites warning from #3251

**Tests to add or update**:
- Unit: unresolved Shared Steps → one warning and one review entry; resolved → none, and the `ref` is remapped.
- Unit: the same pair for Shared Parameter.
- E2E: the new scenario (Test Case only in the query) asserts the two warnings and the count; the existing `sharedsteps` scenario still passes with no new entries.

## Risks & Considerations

- Warning volume: one per reference on large test estates. That is intended (each one is a broken Test Case), and the summary count keeps it readable.
- A re-run after the Shared Steps are migrated must remap the reference and stop the warning. `MigrateSharedSteps` replaces in the target value, which on a re-run already holds the source ID, so it still matches. Check this in the e2e re-run.
- A new review category changes the summary output: update the docs and anything that parses the category list.

## Open Questions

- None. Martin approved the three decisions from refinement (new category; default WIQL as a follow-up; keep the stale ID and report it).
