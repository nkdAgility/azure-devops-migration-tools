# Bug Assessment: Re-run repair save re-stamps ClosedDate on Closed work items

- **Slug**: 3326-repair-save-restamps-closeddate
- **Created**: 2026-11-23
- **Source**: https://github.com/nkdAgility/azure-devops-migration-tools/issues/3326 (host: github.com, policy branch: allowlisted); item record `specs/3326-re-run-repair-save-re-stamps-closeddate/item.md`. The fetch was done with the `gh` CLI, not a web fetch, and only for this issue; there were no comments.
- **Verdict**: likely valid, needs reproduction
- **Severity**: high

## Report (verbatim or summarized)

Issue #3326, labels Expedite, blocked, more-information-needed. "On a re-run, a repair save on an already-migrated **Closed** work item re-stamps `Microsoft.VSTS.Common.ClosedDate` with the time of the save." Seen on a live migration with 16.3.6-Preview.5 (`main` 0e017174) on 20 and 23 November: 24 Closed items across two projects. The re-run logs "Work item exists with no revisions to sync, but the target differs: missing external link `vstfs:///Git/Commit/...`" (or a missing Hierarchy link) and saves one extra revision. That revision's field changes are `System.Rev`, `AuthorizedDate`, `RevisedDate`, `ChangedDate` (+1 s), `Watermark` and `ClosedDate`, which moves from the source value to the save time. State stays Closed. StateChangeDate, ActivatedDate and ResolvedDate are unchanged. The next re-run fails the item with "stamped instead of the source values: revision N ClosedDate ...".

Expected: every repair save sets the state-stamped fields (ClosedDate/By, ResolvedDate/By, ActivatedDate/By, StateChangeDate) explicitly to the source values; a re-run corrects an already wrongly stamped target where that is safe; no REST in the production path; no stack traces. Related: #3270, #3305, #3299.

## Symptom

A re-run that repairs a missing link on an already-migrated Closed item saves a new revision in which `ClosedDate` is the save time rather than the source value. The following re-run then fails that item, so the user cannot get a clean run without deleting the item. Expected: the repair save keeps the source's state-stamped values, and a re-run corrects a target already stamped wrongly.

## Reproduction

1. Seed Closed work items with a parent link and a Git commit link in a scratch project on `nkdagility-adomt-source`; migrate to `nkdagility-adomt-target` with `tools/e2e/Invoke-MigrationE2E.ps1`.
2. Remove the commit link on one target item and the parent link on another.
3. Re-run. Expected per the issue: "no revisions to sync, but the target differs", one extra revision, ClosedDate moved to the save time.
4. Re-run again. Expected per the issue: the item fails with "stamped instead of the source values".

**Result on `main` (origin/main 78147078, test orgs, process `amte2e-agile`, BypassRules on, 23 Nov): not reproduced**, for either the hierarchy link or the Git commit link (details in item.md, Reproduction). ClosedDate stayed at the source value on every repair save.

[NEEDS CLARIFICATION: what differs in the live migration. Candidates, most likely first: (a) BypassRules not granted on the target store, so the service stamps its own clock (the processor only warns, line 476); (b) a target process with a custom or inherited rule on Closed that fires on every save while in Closed; (c) server version or type; (d) another change in the same save. This is Q-4.]

## Suspected Code Paths

- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs:969-972` — `ApplyStateStampedFieldsFromSource` is called only `if (correctStamps)`; every other repair save (links, attachments, embedded links, field maps, hyperlink) skips it. This confirms the issue's cause on the code, but I could not show it changes the outcome on the test orgs.
- `TfsWorkItemMigrationProcessor.cs:973` and `StampCatchUpSave` (~1214) — sets only `ChangedBy` and `ChangedDate`, so the state-stamped fields are left to the server.
- `TfsWorkItemMigrationProcessor.cs:994-1005` — the save and the post-save verification, both gated on `stampCorrection != null`.
- `TfsWorkItemMigrationProcessor.cs:1858-1937` (`VerifyMigratedRevisions`) — a target value that is set but different goes to `wrong` and fails the item. This is the cause of the "next re-run fails" half, and it does not distinguish our own catch-up save from a wrongly migrated target (#3270).
- `TfsWorkItemMigrationProcessor.cs:1739` (`GetStateStampedTargetValues`) and `:1795` (`ApplyStateStampedFieldsFromSource`) — the existing machinery to reuse.
- `TfsWorkItemMigrationProcessor.cs:476` — BypassRules is not on: warns only, then the service stamps its own clock. Relevant to Q-4.
- `ProcessWorkItemLinks(..., true)` / `OtherWorkItemsSaved` (958) — saves of the other end of a link; not covered by the item's own stamping (Q-3).

## Root Cause Hypothesis

A repair save on an existing target leaves the state-stamped fields unset unless the target's current values were empty (#3305). If the target server runs a Closed-state rule on that save, it overwrites `ClosedDate` (and possibly `ClosedBy`) with the save time. The next run's check then sees a set but different value and fails the item. The code gap is certain; whether it is the cause of the 24 live items is **not** established: the same repair on the test orgs with BypassRules kept the value. Confidence: high that the gap exists in the code and that setting the fields on every repair save is harmless and correct; medium-low that it fixes the live fault until Q-4 is answered. The live fault probably depends on BypassRules not being granted, or a target process rule.

## Proposed Remediation

**Preferred**: First, get Q-4 answered, or reproduce with BypassRules off on a scratch target and with a custom Closed rule, so the fix can be shown to change the outcome. Then, in a small named method (not more inline branches in `ProcessWorkItemAsync`), apply the state-stamped fields from the source's latest revision on **every** repair save, after field maps and enrichers, and run the existing `VerifyStateStampedFieldsSaved` / `VerifyStateStampedValuesStored` for all of them. Make `VerifyMigratedRevisions` return a result that says what to correct (empty values, or a re-stamped catch-up revision) instead of a bare `bool`. A wrong current value is corrected only under Q-1's proposal: the latest target revision is our catch-up save (no source revision within 10 ms, dated after the matching source revision), its State equals the source's, and the only differing stamped fields are on that revision. Anything else still fails as #3270. Read the source stamps lazily so an in-sync re-run stays fast (#3299). If BypassRules is the cause, also consider failing or warning loudly on a re-run repair save without BypassRules, since the service would stamp regardless.

**Alternatives**:
- Always replay a corrective revision on repair: more reads and a visible extra revision on every repair; rejected.
- Treat any set-but-different current value as correctable: hides #3270 wrongly migrated targets; rejected.

**Files likely to change**:
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`
- `src/MigrationTools.Clients.TfsObjectModel/Processors/StateStampedFields.cs`
- `src/MigrationTools.Clients.TfsObjectModel.Tests/Processors/**`
- `tools/e2e/**` (scenario), `docs/content/docs/reference/processors/tfs-workitem-migration-processor/index.md` if behaviour text changes (regenerate with `GenerateDocs.ps1`)

**Tests to add or update**:
- Unit: a repair save with `correctStamps` false still sets all state-stamped fields from the source.
- Unit: a catch-up revision with only a re-stamped value is corrected; a wrongly migrated revision still fails (#3270); a state mismatch is not "corrected".
- Unit: an in-sync item does no second read (#3299).
- End to end, on `main` and on the branch with exit codes: repair of a Closed item (hierarchy and commit link), a Resolved or reopened item, an item already wrongly stamped, a genuinely wrong target (must fail), a further clean re-run. Includes the BypassRules-off and custom-rule variants if Q-4 points there.

## Risks & Considerations

- A fix that cannot be shown to change the outcome on the test orgs does not meet Definition of Done step 4. Q-4 must be answered or the live conditions reproduced.
- Without BypassRules the service stamps its own clock whatever the client sets, so setting the fields may not help; it would then need a different answer (warn or fail).
- Correcting a current value without rewriting history leaves the earlier wrongly stamped revision in the target.
- One extra read per repaired item (post-save verification); in-sync items must not pay it.
- Other-end-of-link saves (`OtherWorkItemsSaved`) may re-stamp too; whether that is in Purpose is Q-3.
- The item's Purpose edit scope: `TfsWorkItemOverwriteProcessor` and bulk edit are out, to be handed back as follow-ups only if they share the unguarded save.

## Open Questions

- [NEEDS CLARIFICATION: Q-4 — target process and rules on Closed, whether the live log warned that BypassRules is not enabled on the target, and the target server type and version.]
- [NEEDS CLARIFICATION: Q-1 — is the catch-up-save rule an acceptable definition of "safe to correct"?]
- [NEEDS CLARIFICATION: Q-3 — are `OtherWorkItemsSaved` saves in this item's Purpose?]
