---
purpose: A re-run repair save on an already-migrated work item keeps the source's state-stamped values (ClosedDate/By, ResolvedDate/By, ActivatedDate/By, StateChangeDate) instead of re-stamping them with the save time, and a re-run corrects a target already stamped wrongly where that is safe.
paths:
  - src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs
  - src/MigrationTools.Clients.TfsObjectModel/Processors/StateStampedFields.cs
  - src/MigrationTools.Clients.TfsObjectModel.Tests/Processors/**
  - tools/e2e/**
  - docs/content/docs/reference/processors/tfs-workitem-migration-processor/**
  - docs/data/**
  - specs/3326-re-run-repair-save-re-stamps-closeddate/**
---

# #3326 Re-run repair save re-stamps ClosedDate on Closed work items

Run: 5d5dd125

## Sources read
- GitHub issue #3326 (label Expedite): the report, the cause, the expected behaviour and the validation. 24 Closed items across two projects were touched by a repair save on 20 and 23 November (16.3.6-Preview.5, `main` 0e017174).
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`, `ProcessWorkItemAsync` (lines 921-990): when a target exists and there are no revisions to sync, `VerifyMigratedRevisions` returns `correctStamps`. If the item still differs (`FindReRunDifference`, e.g. a missing link), the repair branch (951-979) runs attachments, links, HTML attachments, embedded links, field maps and the reflected id, then calls `ApplyStateStampedFieldsFromSource` **only `if (correctStamps)`** (969). `StampCatchUpSave` (1214) sets only `ChangedBy` and `ChangedDate`. Every other repair save leaves the state-stamped fields unset, so the Closed rule stamps `ClosedDate` again. This confirms the cause in the issue.
- `ApplyStateStampedFieldsFromSource` (1795) and `GetStateStampedTargetValues` (1739): already set every state-stamped date and By field from a source revision (By through the user mapping), below the client rules where the field is read-only, and fail the item if the object model does not keep the value. `VerifyStateStampedFieldsSaved` and `VerifyStateStampedValuesStored` (1690, 1949) check the result after the save, including a second read from the server. The revision replay path (1540) already uses all of this, so only the repair branch lacks it.
- `VerifyMigratedRevisions` (1858): compares each target revision with the source revision it replays. A target value that is empty where the source has one is the #3305 case (warned, and the latest revision is corrected). A target value that is **set but different** is added to `wrong` and the item **fails** with "stamped instead of the source values" (1933-1937). That is the message the issue says the next re-run reports.
- `platform/specs/rules.md` (comment rules), `platform/specs/architecture.md` (TFS object model vs REST; end-to-end validation; docs sync) and `platform/specs/systems.md` (this is the `tfs-object-model` system). `.specify/memory/constitution.md` is still the unfilled template, so it contributes no rules.
- Related items named in the issue: #3270 (a wrongly saved revision is never treated as migrated), #3305 (current values corrected when the target's are empty), #3299 (an in-sync re-run does no extra reads or save).

## Scope
- In: the repair branch of `ProcessWorkItemAsync` for an existing target with no revisions to sync. Every repair save there sets the state-stamped fields explicitly to the source's latest revision values, so the server's state rules do not stamp them. Cover all repair causes: links, attachments, embedded links, field maps, and the migration hyperlink save at line 994-998 when it is the only change.
- In: the re-run behaviour for a target already stamped wrongly by an earlier repair save: it is corrected without deleting and migrating the item, where that is safe (see Q-1), and a further re-run is clean with no new revisions.
- In: unit tests for the new branch logic; an end-to-end scenario for the issue's validation; the docs for the processor if behaviour text changes.
- Out: rewriting saved revisions (Azure DevOps cannot). The earlier wrongly stamped revision stays in the history; only the current values are corrected.
- Out: REST. The issue requires no REST in the production path, and no stack traces.
- Out: the revision replay path (`revisionsToMigrate.Count > 0`), which already sets the fields per revision.
- Out: other processors (`TfsWorkItemOverwriteProcessor`, bulk edit), unless the investigation shows they share the same unguarded save. If they do, hand back as a proposed follow-up.

## Reproduction
Not yet run by this worker. The steps below are the issue's, which it observed on a live migration; they must be run on `main` against the test orgs before any code change (Definition of Done step 4).
1. Create scratch projects on `nkdagility-adomt-source` and `nkdagility-adomt-target`; seed Closed work items that have a parent link and a Git commit link. → source `ClosedDate` is the real close time.
2. Migrate with `tools/e2e/Invoke-MigrationE2E.ps1` (built `MigrationTools.ConsoleFull`). → target `ClosedDate` equals the source value.
3. On the target, remove the commit link on one item and the parent link on another. → each is now "differs".
4. Re-run the same migration. → expected on `main` per the issue: "Work item exists with no revisions to sync, but the target differs: missing ...", one extra revision, and `ClosedDate` moved to the save time while State stays Closed.
5. Re-run once more. → expected on `main` per the issue: the item fails with "stamped instead of the source values: revision N ClosedDate ...".
Result on `main`: not observed here yet; see Q-2.

## Rubber duck
When the migration runs again against a target that has already been migrated, it checks each existing target item. If the source has no new revisions but the target is missing a link (or an attachment, or a field map result), it adds the missing piece and saves the item once. For a Closed item that save is a state-bearing save to the server, and the process's Closed rule writes the current time into `ClosedDate` (and maybe `ClosedBy`). The user sees a Closed date that is different from the source, which is a data-parity fault, and on the next run the item then fails the check because its date is now wrong, so the user cannot get a clean run without deleting the item. It affects anyone who re-runs a migration to repair links, which is the normal use of a re-run, and the 24 items show it is already happening.

The fix, as the issue states it: on every repair save, set the state-stamped fields explicitly to the source's latest values before saving. The machinery for that exists and is already used when `correctStamps` is true; the gap is that it is gated on `correctStamps` instead of being done on every repair save.

Gaps the explanation showed:
1. **Already wrongly stamped targets fail, they are not corrected.** `VerifyMigratedRevisions` throws for a value that is set but different. The wrong `ClosedDate` is in the latest target revision, which is the catch-up save (dated source latest + 1 s, matched to the latest source revision at or before it). To correct it, the check must tell "the latest revision is a catch-up save of ours with only a re-stamped value" apart from "the target was migrated wrongly" (#3270), which must still fail. See Q-1.
2. **Which source values to apply.** The repair applies the source's latest revision values. If the target's state differs from the source's latest state (a target that is behind), applying them is wrong; the check must establish the target is at the source's latest state first. Today `VerifyMigratedRevisions` fails on a state mismatch, so this holds, but the tests should pin it.
3. **Other saves in the same run.** `ProcessWorkItemLinks(..., true)` can save other work items (`OtherWorkItemsSaved`), for example the other end of a link. Those saves may re-stamp the other item's Closed fields in the same way and are not covered by the item's own stamp setting. Needs checking against the code and the test org.
4. **Items that are not Closed.** Resolved, Active and state-change dates follow the same rule; the fix is generic over `StateStampedFields`, but the end-to-end check should include one item that is Resolved or has been reopened, not only Closed.
5. **#3299 speed.** Setting the fields needs the second read of source values (`sourceStamps.Value`). It must only happen on a repair save, never on an in-sync item, so an in-sync re-run stays fast.
6. **Corrected values and the verify steps.** After the save, the existing `VerifyStateStampedFieldsSaved` and `VerifyStateStampedValuesStored` run only when `stampCorrection != null`; they will then run on every repair save, which adds a read per repaired item. Acceptable, but record it.
7. **The log line.** Line 1004 says "current state-stamped values set to the source values"; the repair path will log it for all repairs, so the wording and the "Updated" counts in the re-run summary should still be right.

## Architecture check
- `platform/specs/rules.md` (comments say only what code cannot): no break expected; the existing doc comments cite issue numbers and the new ones must say why, not what. Settle by reading the diff.
- TFS object model vs REST (`architecture.md`): the issue says no REST in the production path. The change stays in `MigrationTools.Clients.TfsObjectModel` and reuses the object-model second read (`ReadRevisionsAgain`). No finding if it stays that way.
- Documentation sync (`architecture.md`): no option classes change, so `GenerateDocs.ps1` output should not change; processor reference text (`index.md` lines 81-83 describe state-stamped behaviour) may need a sentence for repair saves and the corrected current values. Settle by running `GenerateDocs.ps1` and reading the diff.
- End-to-end validation (`architecture.md`, `CLAUDE.md`): this is a migration-behaviour change, so unit tests are not enough. It needs the harness scenarios on `main` and on the branch, with failure paths: a target wrongly stamped by a repair, and a target that is genuinely wrong (must still fail), each with exit codes.
- Module boundaries: `ProcessWorkItemAsync` is already large; the new code should be a small named method rather than more inline branches, and `VerifyMigratedRevisions` should return a result that says what to correct instead of a bare `bool`. Not a break today, a risk if the fix is inlined. Settle in the spec.
Open findings: 0

## Questions
- Q-1 What makes a wrongly stamped current value safe to correct on a re-run? Proposal: only when the latest target revision is a catch-up save (no source revision within 10 ms, dated after the matching source revision), its State equals the source's, and the only differing state-stamped fields are on that revision; anything else still fails as a wrong target (#3270). (for Martin) open
- Q-2 Reproduction on `main`: run steps 1-5 on the test orgs before the spec, with the scratch projects named for this item, and record the output and exit codes here. (for the worker, at the start of Specifying, needs `az login`) open
- Q-3 Do the other end of a link's saves (`OtherWorkItemsSaved`) also re-stamp, and is that in this item's Purpose? If yes, the Purpose needs adding to; if it only shows in the end-to-end run, hand back as a follow-up. (for Martin) open
