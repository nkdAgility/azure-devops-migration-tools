---
issue: 3256
purpose: When a Test Case's Shared Steps or Shared Parameter reference cannot be remapped because the referenced item is not in the target, the run says so (a warning per reference and a count in the end-of-run review summary) instead of silently leaving the source ID.
paths:
  - src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemLinkTool.cs
  - src/MigrationTools.Clients.TfsObjectModel/Tools/UnresolvedTestCaseReference.cs
  - platform/specs/core/**, platform/specs/tfs-object-model/**, platform/specs/e2e-harness/**, platform/specs/docs-site/**
  - src/MigrationTools/**/WorkItemReview*.cs   # the new review category
  - src/MigrationTools.Clients.TfsObjectModel.Tests/**
  - tools/e2e/scenarios/sharedsteps-3256/**
  - docs/** (generated via GenerateDocs.ps1, and the Migrating Plans and Suites warning from #3251)
---

# 3256: Test Case step and parameter references silently left at source IDs

## Scope

In scope:
- `TfsWorkItemLinkTool.MigrateSharedSteps` and `MigrateSharedParameters`: report each reference whose Shared Steps / Shared Parameter exists in the source but has no match in the target.
- Count those in the end-of-run review summary (`WorkItemReviewLog`).
- Unit tests and an e2e scenario for the unresolved and the resolved path.
- Docs update for the warning.

Out of scope:
- Migrating the missing Shared Steps / Shared Parameter automatically.
- Rewriting or blanking the stale reference (it stays as is; only reported). See Decision 3.
- Any REST dependency in migration code (object model only; TFS 2015+).
- Changing the default WIQL query, unless Martin approves Decision 2.

## Sources read

- Issue #3256 (body; no comments).
- `CLAUDE.md`, `.github/copilot-instructions.md` (end-to-end validation section).
- `src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemLinkTool.cs` lines 223-319 (`MigrateSharedSteps`, `ReadableSourceItems`, `MigrateSharedParameters`).
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs:1322-1323` (call site).
- `appsettings.json` and `src/MigrationTools.Samples/configuration-for-copy.json` (default WIQL excludes `'Shared Steps','Shared Parameter'`).
- `tools/e2e/scenarios/sharedsteps/` (seed, config, assert) and `tools/e2e/Invoke-MigrationE2E.ps1`.

## Reproduction (main, 56bab21f)

System: test orgs `nkdagility-adomt-source` / `nkdagility-adomt-target`, scratch projects `amte2e-3256-src` / `amte2e-3256-tgt` (process `amte2e-agile`), Entra ID.

1. Seeded with `tools/e2e/scenarios/sharedsteps/seed.ps1 -Src amte2e-3256-src -Tgt amte2e-3256-tgt`: Shared Steps 5591, Shared Parameter 5592, Test Case 5593 using both.
2. Config: the `sharedsteps` scenario config with project names swapped and the WIQL narrowed to `[System.WorkItemType] IN ('Test Case')`, the same effect as the default exclusion.
3. `tools/e2e/Invoke-MigrationE2E.ps1 -Config <scratch>/config.json` → **exit 0**, `1 created, 0 failed`.

Result in target Test Case 9344:
- `Microsoft.VSTS.TCM.Steps`: `<compref id="2" ref="5591" />` (the **source** Shared Steps ID).
- `Microsoft.VSTS.TCM.LocalDataSource`: `"dataSetId":5592`, `"sharedParameterDataSetIds":[5592]` (the **source** Shared Parameter ID).
- Neither 5591 nor 5592 exists in the target. In a busy target those IDs could point at unrelated items.
- Log: two `[SKIP] Unable to migrate link ... SharedStepReferencedBy-Reverse / SharedParameterReferencedBy-Reverse ... as target WI has not been migrated` warnings and `link-skipped: 2` in the review summary. These are about the **links**. Nothing says the step or parameter **field references** were left at source IDs.

Cleanup: both scratch projects deleted, the seed's `state.local.json` and attachment folder removed.

## Cause

In both methods, when `FindReflectedWorkItemByReflectedWorkItemId` returns `null` the loop just moves on (`if (matchingTargetSharedStep != null) {...}` with no else; same in `MigrateSharedParameters`). `ReadableSourceItems` only reports items missing from the *source* (TF26198). A source item that exists but was not migrated is never reported. So the reference keeps the source ID and nothing is logged about it.

## Proposed fix

In `TfsWorkItemLinkTool`:
- In `MigrateSharedSteps`, when there is no target match: `Log.LogWarning("  [SKIP] Shared Steps {SourceRefId} used by Test Case {SourceId} (target {TargetId}) not remapped: it has not been migrated to the target, so its step reference keeps the source id. Migrate it and re-run.")` and `WorkItemReviewLog.Record(...)` with source and target IDs and the action "Migrate the Shared Steps, then re-run".
- Same in `MigrateSharedParameters` for Shared Parameter ("its data source reference keeps the source id").
- Factor the warning and review record into one private helper next to `ReadableSourceItems`, so both "not in source" and "not in target" share one wording pattern.
- No REST; object model only.

Tests: unit tests with a fake target that returns null / a match, asserting one review entry per unresolved reference and none when resolved. E2E: a new `tools/e2e/scenarios/sharedsteps-3256/` (or a second config in `sharedsteps`) with the query narrowed to Test Case, asserting the warning and the review count; plus the existing `sharedsteps` scenario still passes with no new entries.

## Outcome checks

| # | System | Command | Pass |
|---|--------|---------|------|
| 1 | Test orgs, scratch `amte2e-3256-*`, Test Case only in query | `Invoke-MigrationE2E.ps1 -Config tools/e2e/scenarios/sharedsteps-3256/config.json` | Exit 0; log has one warning naming Shared Steps 5xxx and Test Case, one naming Shared Parameter; review summary counts 2 unresolved references (on main: none) |
| 2 | Test orgs, `sharedsteps` scenario (all three types) | `Invoke-MigrationE2E.ps1 -Config tools/e2e/scenarios/sharedsteps/config.json` then `assert.ps1` | All asserts pass; no unresolved-reference warnings |
| 3 | Local | `dotnet test` on `MigrationTools.Clients.TfsObjectModel.Tests` (L0/L1) | New tests pass: unresolved → one review entry each; resolved → none and the field is remapped |
| 4 | Docs | `GenerateDocs.ps1` | Docs build; Migrating Plans and Suites warning points at the new log line |

## Decisions for Martin

Martin approved the three recommendations: a new review category, the default WIQL change as a separate follow-up, and keeping the stale ID and reporting it.

1. **Review category.** Reuse `link-skipped`, or add a new category (for example `reference-unresolved`) so these show up apart from the link skips that already appear for the same items? Recommendation: new category, because the field reference and the link are different consequences.
2. **Default WIQL.** The issue suggests removing `'Shared Steps','Shared Parameter'` from the default exclusion in `appsettings.json` (and `configuration-for-copy.json`) now that #3245 finds them by `Microsoft.VSTS.Build.IntegrationBuild`. That changes what a default run migrates. Include it in this item, or make it a separate follow-up? Recommendation: separate follow-up; this item only makes the problem visible.
3. **Leave or clear the stale ID.** Keep the source ID in the field (current behaviour, just reported), or strip it? Recommendation: keep and report; stripping loses data and can't be undone by a re-run.

## Spec

Working spec: `.specify/bugs/sharedsteps-unresolved-refs/assessment.md` (the bug assessment Martin approves).

Spec presented: .specify/bugs/sharedsteps-unresolved-refs/assessment.md sha256 f67ea8fdec629a8b614225772cfb7bdc2654f9247b6208a2147b3592e4372ca5

## Reproduction

Reproduced on main (56bab21f), 2026-11-20, on the test orgs with scratch projects `amte2e-3256-src` / `amte2e-3256-tgt` (since deleted):

1. Seed with `tools/e2e/scenarios/sharedsteps/seed.ps1 -Src amte2e-3256-src -Tgt amte2e-3256-tgt`: Shared Steps 5591, Shared Parameter 5592, Test Case 5593 using both.
2. Use the `sharedsteps` scenario config with the project names swapped and `WIQLQuery` narrowed to `[System.WorkItemType] IN ('Test Case')`.
3. Run `tools/e2e/Invoke-MigrationE2E.ps1 -Config <config>`.

Observed: exit 0, `1 created, 0 failed`. Target Test Case 9344 has `<compref id="2" ref="5591" />` and `"dataSetId":5592`, `"sharedParameterDataSetIds":[5592]`; neither ID exists in the target. The log has only the two generic link skips (`SharedStepReferencedBy-Reverse` / `SharedParameterReferencedBy-Reverse ... target WI has not been migrated`, review `link-skipped: 2`). Nothing mentions the field references.


## Cause

The "target not found" case was never handled: both methods only act when a target match exists, and the only reporting added so far (`ReadableSourceItems`) covers a missing source item. Confidence: high (code reading plus the reproduction above).


## Proposed fix

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


Spec agreed: Spec agreed
Spec version: .specify/bugs/sharedsteps-unresolved-refs/assessment.md sha256 f67ea8fdec629a8b614225772cfb7bdc2654f9247b6208a2147b3592e4372ca5

## Spec

Working spec: `specs/3256-test-case-step-and-parameter-references/assessment.md` (the bug assessment Martin approves).


## Reproduction

Reproduced on main (56bab21f), 2026-11-20, on the test orgs with scratch projects `amte2e-3256-src` / `amte2e-3256-tgt` (since deleted):

1. Seed with `tools/e2e/scenarios/sharedsteps/seed.ps1 -Src amte2e-3256-src -Tgt amte2e-3256-tgt`: Shared Steps 5591, Shared Parameter 5592, Test Case 5593 using both.
2. Use the `sharedsteps` scenario config with the project names swapped and `WIQLQuery` narrowed to `[System.WorkItemType] IN ('Test Case')`.
3. Run `tools/e2e/Invoke-MigrationE2E.ps1 -Config <config>`.

Observed: exit 0, `1 created, 0 failed`. Target Test Case 9344 has `<compref id="2" ref="5591" />` and `"dataSetId":5592`, `"sharedParameterDataSetIds":[5592]`; neither ID exists in the target. The log has only the two generic link skips (`SharedStepReferencedBy-Reverse` / `SharedParameterReferencedBy-Reverse ... target WI has not been migrated`, review `link-skipped: 2`). Nothing mentions the field references.


## Cause

The "target not found" case was never handled: both methods only act when a target match exists, and the only reporting added so far (`ReadableSourceItems`) covers a missing source item. Confidence: high (code reading plus the reproduction above).


## Proposed fix

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


## Docs updated

- core: `platform/specs/core/review-log.md`: `reference-left-on-source`
- tfs-object-model: `platform/specs/tfs-object-model/test-case-references.md`: UnresolvedTestCaseReference.Report
- e2e-harness: `platform/specs/e2e-harness/scenarios.md`: sharedsteps-3256
- docs-site: `platform/specs/docs-site/review-log-categories.md`: `reference-left-on-source` (#3256)

## Build evidence

- Unit (L0): `UnresolvedTestCaseReferenceTests`, 6 passed. Existing suites: MigrationTools.Tests L0 227 passed; MigrationTools.Clients.TfsObjectModel.Tests L0|L1 426 passed, 1 skipped.
- E2E `tools/e2e/scenarios/sharedsteps-3256`, scratch projects amte2e-3256-* (deleted afterwards):
  - main (e2cdafa4), Test Cases only (now `config.json`): exit 0; assert `-Expect Unresolved` exit 4. Source ids kept (ref=7524, dataSetId 7525), no warning, 0 `reference-left-on-source` entries, no summary count.
  - change, Test Cases only: exit 0; assert exit 0. Both warnings logged, 2 entries, summary `reference-left-on-source: 2`.
  - change, re-run with `config-all.json` (all three types): exit 0; assert `-Expect Resolved` exit 0. ref=11162 and dataSetId 11161 (target ids), 0 entries.

  - change, again in the proof's shape (`config.json`, no log directory, `assert.ps1` with no arguments), then `config-all.json`: both asserts exit 0.

## Output done evidence

Automated checks (`done-checks.ps1`, on the change, 2026-11-23): `dotnet build MigrationTools.sln` succeeded. `dotnet test MigrationTools.sln` with the live test orgs: Host 12 passed (2 skipped), FileSystem 6, MigrationTools.Tests 305 (2 skipped), TfsObjectModel 434 (6 of them new for this item), Rest 81; 0 failed. Proof (`proof.ps1`): all covered scenarios passed, including the new `sharedsteps-3256` and the existing `sharedsteps`. `docs-check.ps1`: docs reconciled.

Manual validation on the test orgs, own scratch projects amte2e-3256-* (deleted afterwards):
- main (e2cdafa4), Test Cases only: exit 0; the Test Case keeps source ids 7524/7525, with no warning, no review entry and no count. The scenario's assert fails (exit 4).
- change, Test Cases only: exit 0; two `[SKIP] ... not remapped` warnings, two `reference-left-on-source` entries, and `reference-left-on-source: 2` in the summary. The assert passes, both with an explicit log directory and in the proof's shape.
- change, re-run with all three types: exit 0; the references point at the target copies (for example ref=12509, dataSetId 12508), with no new entries. The assert passes.

Not covered: the resolved row for an earlier entry on a re-run is shown by the unit tests only. Each e2e run writes to its own log folder, so the tool does not read the earlier run's log. The default WIQL is unchanged (approved as a separate follow-up).

Confidence: 90%



## Output done evidence

Automated checks (`done-checks.ps1`, on the change, 2026-11-23): `dotnet build MigrationTools.sln` succeeded. `dotnet test MigrationTools.sln` with the live test orgs: Host 12 passed (2 skipped), FileSystem 6, MigrationTools.Tests 305 (2 skipped), TfsObjectModel 434 (6 of them new for this item), Rest 81; 0 failed. Proof (`proof.ps1`): all covered scenarios passed, including the new `sharedsteps-3256` and the existing `sharedsteps`. `docs-check.ps1`: docs reconciled.

Manual validation on the test orgs, own scratch projects amte2e-3256-* (deleted afterwards):
- main (e2cdafa4), Test Cases only: exit 0; the Test Case keeps source ids 7524/7525, with no warning, no review entry and no count. The scenario's assert fails (exit 4).
- change, Test Cases only: exit 0; two `[SKIP] ... not remapped` warnings, two `reference-left-on-source` entries, and `reference-left-on-source: 2` in the summary. The assert passes, both with an explicit log directory and in the proof's shape.
- change, re-run with all three types: exit 0; the references point at the target copies (for example ref=12509, dataSetId 12508), with no new entries. The assert passes.

Not covered: the resolved row for an earlier entry on a re-run is shown by the unit tests only. Each e2e run writes to its own log folder, so the tool does not read the earlier run's log. The default WIQL is unchanged (approved as a separate follow-up).

Confidence: 90%

Reasons:
- The defect is shown failing on main and passing on the change on the real test orgs, with exit codes and the target fields checked.
- The full build, every test suite (live tests included) and the repository's proof all pass on the change.
- The change is small and object-model only: one helper and two null branches in TfsWorkItemLinkTool, plus one review category.
- Not 100%: the re-run resolving an earlier `reference-left-on-source` row is covered by unit tests, not end to end, and it has had no independent review yet.


