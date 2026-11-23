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
Result on `main` (origin/main 78147078, local build 16.3.6-Local.7, test orgs, process `amte2e-agile`, BypassRules on, 23 November): **not reproduced**.
- Hierarchy link (scratch projects `amt3326-src`/`amt3326-tgt`, seeded by a copy of the longrun-3305 seed with 2 Features and 4 Closed User Stories): first run created 6, exit 0, assert PASS. The parent link was removed on 2 target stories; the re-run logged "no revisions to sync, but the target differs: missing System.LinkTypes.Hierarchy-Reverse link", saved one revision on each (2 updated, 4 in sync, exit 0). On both, revision 5 kept ClosedDate at the source value (13:04:55 and 13:04:54) and ClosedBy unchanged; the longrun-3305 assert, current values included, PASS.
- Git commit link (scratch projects `amt3326-gsrc`/`amt3326-gtgt`, one commit pushed to a same-named repo in both orgs, 2 Closed stories linked to it, TfsGitRepositoryTool mapping on): first run created 2, exit 0. The commit link was removed on one target story (target revision 5); the re-run logged "missing external link vstfs:///Git/Commit/..." and saved revision 6 (1 updated, 1 in sync, exit 0). ClosedDate stayed at the source value (13:08:27).
- So on this org and process, a repair save with BypassRules keeps ClosedDate without the fields being set. The live case must differ in something not yet known: the target process (a custom or inherited rule on Closed), BypassRules not granted on the target store (the processor warns and the service then stamps its own clock, line 476), the server version, or another change in the same save. See Q-4.
- **Reproduced** with the live conditions from Q-4: a Git commit link the target cannot resolve. A commit was pushed to the source repo of `amt3326-gsrc` only, and 2 inherited-Agile Tasks were linked to it and taken New -> Active -> Closed. First run: both created (exit 0), and the enricher warned "commit ... not found in target repo amt3326-gtgt/amt3326-gtgt; leaving link vstfs:///Git/Commit/<source project id>%2F<source repo id>%2F<sha> pointing at the source". A re-run with the link in place logged "target differs: missing external link" but changed nothing ("checked with nothing to change"). The link was then removed on target #13970 (target revision 4) and the run repeated: "target differs: missing external link", 1 updated, exit 0. Target revision 5 has ChangedDate 13:18:13 (revision 4 + 1 s) and **ClosedDate 13:19:19, the save time**, where the source and target revision 3 have 13:16:08. State stays Closed. This matches the live report.
- What sets this case apart: the repair re-adds an ArtifactLink that still points at the source project and repo. A link that resolves in the target (rewritten to the target repo) or a Hierarchy link is saved without re-stamping. The cause of the re-stamp itself (the unresolved artifact, or the external-link save) is still to be pinned down in the spec.
- Also seen: with the unresolved link present on the target, every re-run reports the item as "target differs: missing external link", although nothing is missing, so such an item is never reported in sync.

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
- Q-2 Reproduction on `main`: run steps 1-5 on the test orgs before the spec, with the scratch projects named for this item, and record the output and exit codes here. (for the worker, at the start of Specifying, needs `az login`) answered: run, not reproduced (see Reproduction)
- Q-4 What differs in the live migration from the test-org run: the target process and its rules on Closed, whether the log warned that BypassRules is not enabled on the target, and the target server (Azure DevOps Services or Server, which version)? Without one of these, a fix cannot be shown to change the outcome on the test orgs. (for Martin) answered from the customer logs (via MANAGER-ADMT): BypassRules on, no custom rules (inherited Agile Task), Azure DevOps Services; every stamping save adds an unresolved commit ArtifactLink. Reproduced, see Reproduction.
- Q-3 Do the other end of a link's saves (`OtherWorkItemsSaved`) also re-stamp, and is that in this item's Purpose? If yes, the Purpose needs adding to; if it only shows in the end-to-end run, hand back as a follow-up. (for Martin) open

## Spec

Working spec: `specs/3326-re-run-repair-save-re-stamps-closeddate/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3326-re-run-repair-save-re-stamps-closeddate/assessment.md sha256 503db291a90a00856d5fbcc0ffedd5232146321e80c105494b8c282f1a6c5c3d

## Reproduction

1. Seed Closed work items with a parent link and a Git commit link in a scratch project on `nkdagility-adomt-source`; migrate to `nkdagility-adomt-target` with `tools/e2e/Invoke-MigrationE2E.ps1`.
2. Remove the commit link on one target item and the parent link on another.
3. Re-run. Expected per the issue: "no revisions to sync, but the target differs", one extra revision, ClosedDate moved to the save time.
4. Re-run again. Expected per the issue: the item fails with "stamped instead of the source values".

**Result on `main` (origin/main 78147078, test orgs, process `amte2e-agile`, BypassRules on, 23 Nov): not reproduced**, for either the hierarchy link or the Git commit link (details in item.md, Reproduction). ClosedDate stayed at the source value on every repair save.

[NEEDS CLARIFICATION: what differs in the live migration. Candidates, most likely first: (a) BypassRules not granted on the target store, so the service stamps its own clock (the processor only warns, line 476); (b) a target process with a custom or inherited rule on Closed that fires on every save while in Closed; (c) server version or type; (d) another change in the same save. This is Q-4.]


## Cause

A repair save on an existing target leaves the state-stamped fields unset unless the target's current values were empty (#3305). If the target server runs a Closed-state rule on that save, it overwrites `ClosedDate` (and possibly `ClosedBy`) with the save time. The next run's check then sees a set but different value and fails the item. The code gap is certain; whether it is the cause of the 24 live items is **not** established: the same repair on the test orgs with BypassRules kept the value. Confidence: high that the gap exists in the code and that setting the fields on every repair save is harmless and correct; medium-low that it fixes the live fault until Q-4 is answered. The live fault probably depends on BypassRules not being granted, or a target process rule.


## Proposed fix

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


## Outcome checks

- Unit, the repair branch sets every state-stamped field (ClosedDate/By, ResolvedDate/By, ActivatedDate/By, StateChangeDate) from the source's latest revision when `correctStamps` is false; pass = exit 0 and the new repair-stamp tests pass: `dotnet test src/MigrationTools.Clients.TfsObjectModel.Tests --filter "TestCategory=L0&FullyQualifiedName~Processors"`
- Unit, a latest target revision that is our catch-up save with only a re-stamped value is corrected, and a wrongly migrated revision (#3270) or a state mismatch still fails; pass = exit 0 and those tests pass: `dotnet test src/MigrationTools.Clients.TfsObjectModel.Tests --filter "TestCategory=L0&FullyQualifiedName~Processors"`
- Unit, an in-sync item does no second read of source values (#3299); pass = exit 0 and the in-sync read-count test passes: `dotnet test src/MigrationTools.Clients.TfsObjectModel.Tests --filter "TestCategory=L0&FullyQualifiedName~FastRerun"`
- Solution builds and the whole test suite passes on the test orgs; pass = exit 0 and no failed tests: `dotnet build MigrationTools.sln; $env:MigrationTools__Tests__UseEntraId='true'; $env:MigrationTools__Tests__SourceCollection='https://dev.azure.com/nkdagility-adomt-source'; $env:MigrationTools__Tests__TargetCollection='https://dev.azure.com/nkdagility-adomt-target'; dotnet test MigrationTools.sln --no-build`
- Real system (nkdagility-adomt-source to nkdagility-adomt-target, scratch projects, new scenario `tools/e2e/scenarios/rerun-3326`): after removing the parent link on one Closed target item and the commit link on another, the re-run repairs both, exits 0, and every target ClosedDate/ClosedBy equals the source value; pass = exit 0 and the scenario assert prints PASS: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, a Resolved item and a reopened item (Active) repaired the same way keep ResolvedDate/By, ActivatedDate/By and StateChangeDate at the source values; pass = exit 0 and the rerun-3326 assert prints PASS for those items: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, a target already wrongly stamped by an earlier repair save (catch-up revision, same State, only stamped fields differ) is corrected on re-run without delete and migrate; pass = exit 0 and current ClosedDate equals the source: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, a genuinely wrongly migrated target (#3270, a different State or a stamped value on a non-catch-up revision) still fails the item with "stamped instead of the source values"; pass = migration exit non-zero and that message in the log, asserted by the rerun-3326 assert: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, a further re-run after the repair is clean; pass = exit 0, no new revisions on any item, and every item reported in sync: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, the same scenarios run on `main` first show the fault (or its absence, as recorded under Reproduction) and on the branch show the fix, with exit codes reported in the pull request; pass = both runs recorded: `pwsh -NoProfile -File tools/e2e/Invoke-MigrationE2E.ps1 -Config tools/e2e/scenarios/rerun-3326/config.json`
- Docs match the code; pass = exit 0 and `git diff --exit-code -- docs` is clean after regeneration: `pwsh -NoProfile -File GenerateDocs.ps1; git diff --exit-code -- docs`

## Questions

- Q-5 Outcome check not runnable yet: the repair with BypassRules not granted on the target, and with a custom Closed rule, which the issue's live fault may depend on (Q-4). Until Q-4 is answered there is no command that reproduces it, so the fix cannot be shown to change the live outcome. (for Martin) answered: Q-4 answered; BypassRules is on and there are no custom rules. The fault reproduces with an unresolved commit link (see Reproduction), so that is the outcome check to run.

