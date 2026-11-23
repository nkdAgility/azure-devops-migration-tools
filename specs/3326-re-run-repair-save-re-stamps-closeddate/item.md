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
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`, `ProcessWorkItemAsync` (lines 921-990): when a target exists and there are no revisions to sync, `VerifyMigratedRevisions` returns `correctStamps`. If the item still differs (`FindReRunDifference`, e.g. a missing link), the repair branch (951-979) runs attachments, links, HTML attachments, embedded links, field maps and the reflected id, then calls `ApplyStateStampedFieldsFromSource` **only `if (correctStamps)`** (969). `StampCatchUpSave` (1214) sets only `ChangedBy` and `ChangedDate`. Every other repair save leaves the state-stamped fields unset. This confirms the code gap in the issue.
- `ApplyStateStampedFieldsFromSource` (1795) and `GetStateStampedTargetValues` (1739): already set every state-stamped date and By field from a source revision (By through the user mapping), below the client rules where the field is read-only, and fail the item if the object model does not keep the value. `VerifyStateStampedFieldsSaved` and `VerifyStateStampedValuesStored` (1690, 1949) check the result after the save, including a second read from the server. The revision replay path (1540) already uses all of this, so only the repair branch lacks it.
- `VerifyMigratedRevisions` (1858): compares each target revision with the source revision it replays. A target value that is empty where the source has one is the #3305 case (warned, and the latest revision is corrected). A target value that is **set but different** is added to `wrong` and the item **fails** with "stamped instead of the source values" (1933-1937). That is the message the issue says the next re-run reports.
- `TfsGitRepositoryTool.GetTargetUri` (222) and `TfsWorkItemLinkTool.HasExternalLink` (199): `GetTargetUri` skips the target-existence check, so a commit link left pointing at the source is reported missing on every run (see Reproduction, Q-6).
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
- Not in the paths yet: the permanent "target differs" for an unresolved Git link (`TfsGitRepositoryTool.cs`, `TfsWorkItemLinkTool.cs`). See Q-6.

## Reproduction
Reproduced on `main` (origin/main 78147078, local build 16.3.6-Local.7, test orgs, process `amte2e-agile`, BypassRules on, 23 November), with the live conditions from Q-4: a Git commit link the target cannot resolve. Steps and the cause analysis are in `assessment.md`; the output is here.
- **Reproduced** (scratch projects `amt3326-gsrc`/`amt3326-gtgt`): a commit was pushed to the source repo only, and 2 inherited-Agile Tasks were linked to it and taken New -> Active -> Closed. First run: both created (exit 0), and the enricher warned "commit ... not found in target repo amt3326-gtgt/amt3326-gtgt; leaving link pointing at the source" (the link is the source's vstfs:///Git/Commit artifact URI). The link was removed on target #13970 (target revision 4) and the run repeated: "target differs: missing external link", 1 updated, exit 0. Target revision 5 has ChangedDate 13:18:13 (revision 4 + 1 s) and **ClosedDate 13:19:19, the save time**, where the source and target revision 3 have 13:16:08. State stays Closed.
- Controls, not reproduced: a Hierarchy-Reverse link repair (scratch projects `amt3326-src`/`amt3326-tgt`, 2 Features and 4 Closed User Stories, parent link removed on 2 target stories): the re-run logged "missing System.LinkTypes.Hierarchy-Reverse link", saved one revision on each (2 updated, 4 in sync, exit 0), and revision 5 kept ClosedDate at the source value (13:04:55 and 13:04:54); the longrun-3305 assert PASS. A Git commit link that resolves in the target (2 Closed stories, commit in both repos, link removed on one target story): re-run saved revision 6 (1 updated, 1 in sync, exit 0), ClosedDate stayed at 13:08:27.
- What sets the reproducing case apart: the repair re-adds an ArtifactLink that still points at the source project and repo.
- Also seen: with the unresolved link on the target, every re-run reports "target differs: missing external link" although nothing is missing, so the item is never in sync. The assessment traces this to `GetTargetUri` skipping the target-existence check (Q-6).
- Not yet observed: issue step 5, the second re-run failing with "stamped instead of the source values". Run it first in the fix.

## Rubber duck
When the migration runs again against a target that has already been migrated, it checks each existing target item. If the source has no new revisions but the target is missing a link (or an attachment, or a field map result), it adds the missing piece and saves the item once. For a Closed item that save can write the current time into `ClosedDate` (and maybe `ClosedBy`), as the reproduction shows for a link that still points at the source. The user sees a Closed date that is different from the source, which is a data-parity fault, and on the next run the item then fails the check because its date is now wrong, so the user cannot get a clean run without deleting the item. It affects anyone who re-runs a migration to repair links, which is the normal use of a re-run, and the 24 items show it is already happening.

The fix, as the issue states it: on every repair save, set the state-stamped fields explicitly to the source's latest values before saving. The machinery for that exists and is already used when `correctStamps` is true; the gap is that it is gated on `correctStamps` instead of being done on every repair save. Why the server re-stamps for this link is not yet known, so the fix has to be shown on the reproduction.

Gaps the explanation showed:
1. **Already wrongly stamped targets fail, they are not corrected.** `VerifyMigratedRevisions` throws for a value that is set but different. The wrong `ClosedDate` is in the latest target revision, which is the catch-up save (dated source latest + 1 s, matched to the latest source revision at or before it). To correct it, the check must tell "the latest revision is a catch-up save of ours with only a re-stamped value" apart from "the target was migrated wrongly" (#3270), which must still fail. See Q-1.
2. **Which source values to apply.** The repair applies the source's latest revision values. If the target's state differs from the source's latest state (a target that is behind), applying them is wrong; the check must establish the target is at the source's latest state first. Today `VerifyMigratedRevisions` fails on a state mismatch, so this holds, but the tests should pin it.
3. **Other saves in the same run.** `ProcessWorkItemLinks(..., true)` can save other work items (`OtherWorkItemsSaved`), for example the other end of a link. Those saves may re-stamp the other item's Closed fields in the same way and are not covered by the item's own stamp setting. Needs checking against the code and the test org (Q-3).
4. **Items that are not Closed.** Resolved, Active and state-change dates follow the same rule; the fix is generic over `StateStampedFields`, but the end-to-end check should include one item that is Resolved or has been reopened, not only Closed.
5. **#3299 speed.** Setting the fields needs the second read of source values (`sourceStamps.Value`). It must only happen on a repair save, never on an in-sync item, so an in-sync re-run stays fast.
6. **Corrected values and the verify steps.** After the save, the existing `VerifyStateStampedFieldsSaved` and `VerifyStateStampedValuesStored` run only when `stampCorrection != null`; they will then run on every repair save, which adds a read per repaired item. Acceptable, but record it.
7. **The log line.** Line 1004 says "current state-stamped values set to the source values"; the repair path will log it for all repairs, so the wording and the "Updated" counts in the re-run summary should still be right.
8. **The permanent "differs".** An unresolved Git link is reported missing on every run, so the item is repaired on every run and never reaches in sync. See Q-6.

## Architecture check
- `platform/specs/rules.md` (comments say only what code cannot): no break expected; the existing doc comments cite issue numbers and the new ones must say why, not what. Settle by reading the diff.
- TFS object model vs REST (`architecture.md`): the issue says no REST in the production path. The change stays in `MigrationTools.Clients.TfsObjectModel` and reuses the object-model second read (`ReadRevisionsAgain`). No finding if it stays that way.
- Documentation sync (`architecture.md`): no option classes change, so `GenerateDocs.ps1` output should not change; processor reference text (`index.md` lines 81-83 describe state-stamped behaviour) may need a sentence for repair saves and the corrected current values. Settle by running `GenerateDocs.ps1` and reading the diff.
- End-to-end validation (`architecture.md`, `CLAUDE.md`): this is a migration-behaviour change, so unit tests are not enough. It needs the harness scenarios on `main` and on the branch, with failure paths: a target wrongly stamped by a repair, and a target that is genuinely wrong (must still fail), each with exit codes.
- Module boundaries: `ProcessWorkItemAsync` is already large; the new code should be a small named method rather than more inline branches, and `VerifyMigratedRevisions` should return a result that says what to correct instead of a bare `bool`. Not a break today, a risk if the fix is inlined. Settle in the spec.
Open findings: 0

## Questions
- Q-1 What makes a wrongly stamped current value safe to correct on a re-run? Proposal: only when the latest target revision is a catch-up save (no source revision within 10 ms, dated after the matching source revision), its State equals the source's, and the only differing state-stamped fields are on that revision; anything else still fails as a wrong target (#3270). (for Martin) open
- Q-2 Reproduction on `main`: run steps 1-5 on the test orgs before the spec. (for the worker) answered: run with a Hierarchy link and a resolvable commit link (not reproduced), then with an unresolved commit link (reproduced); see Reproduction. Step 5 is still to run.
- Q-3 Do the other end of a link's saves (`OtherWorkItemsSaved`) also re-stamp, and is that in this item's Purpose? If yes, the Purpose needs adding to; if it only shows in the end-to-end run, hand back as a follow-up. (for Martin) open
- Q-4 What differs in the live migration from the test-org run? (for Martin) answered from the customer logs (via MANAGER-ADMT): BypassRules on, no custom rules (inherited Agile Task), Azure DevOps Services; every stamping save adds an unresolved commit ArtifactLink. The BypassRules and custom-rule hypotheses are ruled out.
- Q-5 Which outcome check shows the live fault? (for Martin) answered: the unresolved commit link reproduction is the outcome check (see Reproduction and the Real system checks below).
- Q-6 Is the permanent "target differs" for an unresolved Git link (`GetTargetUri` skips the target-existence check, so the link left on the source is never found) in this item's Purpose? It is why the live items are repaired on every run. If yes, add `src/MigrationTools.Clients.TfsObjectModel/Tools/TfsGitRepositoryTool.cs` and `TfsWorkItemLinkTool.cs` to the paths and to the Purpose; if no, hand back as a proposed follow-up. (for Martin) open

## Spec

Working spec: `specs/3326-re-run-repair-save-re-stamps-closeddate/assessment.md` (the bug assessment Martin approves, reassessed after the reproduction).

Spec presented: not yet. The earlier presented hash (503db291) is for the superseded assessment.

## Outcome checks

- Unit, the repair branch sets every state-stamped field (ClosedDate/By, ResolvedDate/By, ActivatedDate/By, StateChangeDate) from the source's latest revision when `correctStamps` is false; pass = exit 0 and the new repair-stamp tests pass: `dotnet test src/MigrationTools.Clients.TfsObjectModel.Tests --filter "TestCategory=L0&FullyQualifiedName~Processors"`
- Unit, a latest target revision that is our catch-up save with only a re-stamped value is corrected, and a wrongly migrated revision (#3270) or a state mismatch still fails; pass = exit 0 and those tests pass: `dotnet test src/MigrationTools.Clients.TfsObjectModel.Tests --filter "TestCategory=L0&FullyQualifiedName~Processors"`
- Unit, an in-sync item does no second read of source values (#3299); pass = exit 0 and the in-sync read-count test passes: `dotnet test src/MigrationTools.Clients.TfsObjectModel.Tests --filter "TestCategory=L0&FullyQualifiedName~FastRerun"`
- Solution builds and the whole test suite passes on the test orgs; pass = exit 0 and no failed tests: `dotnet build MigrationTools.sln; $env:MigrationTools__Tests__UseEntraId='true'; $env:MigrationTools__Tests__SourceCollection='https://dev.azure.com/nkdagility-adomt-source'; $env:MigrationTools__Tests__TargetCollection='https://dev.azure.com/nkdagility-adomt-target'; dotnet test MigrationTools.sln --no-build`
- Real system (nkdagility-adomt-source to nkdagility-adomt-target, scratch projects, new scenario `tools/e2e/scenarios/rerun-3326`), the unresolved-commit-link reproduction: Closed inherited-Agile Tasks linked to a commit that exists only in the source repo; after removing the link on a target, the re-run repairs it, exits 0, and every target ClosedDate/ClosedBy equals the source value; pass = exit 0 and the scenario assert prints PASS: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, controls: a Hierarchy link repair and a resolvable commit link repair on Closed items still keep ClosedDate/By; pass = exit 0 and the rerun-3326 assert prints PASS for those items: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, a Resolved item and a reopened item (Active) repaired the same way keep ResolvedDate/By, ActivatedDate/By and StateChangeDate at the source values; pass = exit 0 and the rerun-3326 assert prints PASS for those items: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, a target already wrongly stamped by an earlier repair save (catch-up revision, same State, only stamped fields differ) is corrected on re-run without delete and migrate; pass = exit 0 and current ClosedDate equals the source: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, a genuinely wrongly migrated target (#3270, a different State or a stamped value on a non-catch-up revision) still fails the item with "stamped instead of the source values"; pass = migration exit non-zero and that message in the log, asserted by the rerun-3326 assert: `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, a further re-run after the repair is clean; pass = exit 0, no new revisions on any item, and every item reported in sync (needs Q-6 for the unresolved-link item): `pwsh -NoProfile -File .specify/memory/proof.ps1`
- Real system, the same scenarios run on `main` first show the fault and on the branch show the fix, with exit codes reported in the pull request; pass = both runs recorded: `pwsh -NoProfile -File tools/e2e/Invoke-MigrationE2E.ps1 -Config tools/e2e/scenarios/rerun-3326/config.json`
- Production path has no REST and no stack trace (issue expectation): the repair path stays in `MigrationTools.Clients.TfsObjectModel`, and the failure-path scenario logs show the "stamped instead of the source values" message without a stack trace; pass = exit 0 from the search for REST use in the changed files and no `   at ` stack frames in the scenario logs: `pwsh -NoProfile -Command "git diff origin/main --name-only -- src | ForEach-Object { if (Select-String -Path $_ -Pattern 'HttpClient|WorkItemTrackingHttpClient|\.Rest\.' -Quiet) { throw \"REST in $_\" } }; Get-ChildItem $env:TEMP/amtproof-* -Recurse -Filter *.log | Select-String -Pattern '^\s+at \S+\(' | ForEach-Object { throw \"stack trace: $_\" }"`
- Docs match the code; pass = exit 0 and `git diff --exit-code -- docs` is clean after regeneration: `pwsh -NoProfile -File GenerateDocs.ps1; git diff --exit-code -- docs`

## Spec

Working spec: `specs/3326-re-run-repair-save-re-stamps-closeddate/assessment.md` (the bug assessment Martin approves).

Spec presented: specs/3326-re-run-repair-save-re-stamps-closeddate/assessment.md sha256 22d531036cb61c36ef7eaac11fa32b412aaa5eccb38c56e5b011689db9cdf305

## Reproduction

Reproduced on `main` (origin/main 78147078, local build 16.3.6-Local.7, test orgs, process `amte2e-agile`, BypassRules on, 23 November). Scratch projects `amt3326-gsrc` / `amt3326-gtgt`.

1. Push a commit to the source repo only. Link 2 inherited-Agile Tasks to it and take them New -> Active -> Closed.
2. Migrate with `tools/e2e/Invoke-MigrationE2E.ps1` (built `MigrationTools.ConsoleFull`, TfsGitRepositoryTool mapping on). Both items created, exit 0. The enricher warns "commit ... not found in target repo amt3326-gtgt/amt3326-gtgt; leaving link vstfs:///Git/Commit/<source project id>%2F<source repo id>%2F<sha> pointing at the source".
3. Remove that commit link on target #13970 (target revision 4).
4. Re-run. Log: "target differs: missing external link", 1 updated, exit 0. Target revision 5: ChangedDate 13:18:13 (revision 4 + 1 s), **ClosedDate 13:19:19, the save time**; source and target revision 3 have 13:16:08. State stays Closed.

Controls that do **not** reproduce on the same orgs and process: a Hierarchy-Reverse link repair on 2 Closed User Stories (revision 5 kept ClosedDate), and a Git commit link repair where the commit exists in the target and is rewritten to the target repo (revision 6 kept ClosedDate). Full output and exit codes: item.md, Reproduction.

Not yet run: step 5 of the issue (a second re-run failing with "stamped instead of the source values"); the code says it must (see `VerifyMigratedRevisions`), but it is not observed. [NEEDS CLARIFICATION: run it as the first step of the fix, to confirm the second half on the test orgs.]

Also seen: with the unresolved link present on the target, every re-run reports "target differs: missing external link" although nothing is missing, so the item is never reported in sync.


## Cause

Two faults.

1. **The re-stamp.** A repair save on an existing target leaves the state-stamped fields unset unless the target's current values were empty (#3305). With BypassRules on and no custom rules, the service still writes the save time into `ClosedDate` when the repair re-adds an ArtifactLink that points at the source project and repo. A Hierarchy link and a resolvable commit link are saved without re-stamping, so what differs is the link, not the state rule. Why the service re-stamps for that link is **not** pinned down (candidates: the server treating an artifact link to a foreign project as a different kind of save, or a second internal save). The next run's check then sees a set but different value and fails the item. Confidence: high that the unset fields are the gap and that setting them on every repair save is correct; **medium** that setting them stops the re-stamp, because the cause of the re-stamp itself is unproven. The fix must be shown on the reproduction above, not assumed.
2. **The permanent "differs".** `GetTargetUri` skips the target-existence check, so an unresolved link is reported missing on every run, and a repair is attempted every time, so the item never reaches in sync. Confidence: medium-high from the code; confirm with a debug run.

Fault 2 is what makes fault 1 reachable on every re-run for the 24 live items; it also means the re-stamp can recur.


## Proposed fix

**Preferred**:
1. Before any change, run issue step 5 on the reproduction, so both halves are observed on `main`, and capture the repair save's field changes to see what else the server writes.
2. In a small named method (not more inline branches in `ProcessWorkItemAsync`), apply the state-stamped fields from the source's latest revision on **every** repair save, after field maps and enrichers, and run `VerifyStateStampedFieldsSaved` / `VerifyStateStampedValuesStored` for all of them. Show on the reproduction that `ClosedDate` then stays at the source value. If it does not (the server restamps whatever the client sets for that link), the fix needs another answer, for example a corrective save after the link save, and the assessment is updated.
3. Make `GetTargetUri` agree with `Enrich` about an unresolved link: a source link left on the source because the target lacks the commit counts as present when the target holds the raw source link. This stops the permanent "differs" and the repeated repair saves. It is a smaller change and likely a separate commit; whether it is in Purpose is a question (Q-6).
4. Make `VerifyMigratedRevisions` return a result that says what to correct (empty values, or a re-stamped catch-up revision) instead of a bare `bool`. A wrong current value is corrected only under Q-1's proposal: the latest target revision is our catch-up save (no source revision within 10 ms, dated after the matching source revision), its State equals the source's, and the only differing stamped fields are on that revision. Anything else still fails as #3270.
5. Read the source stamps lazily so an in-sync re-run stays fast (#3299).

**Alternatives**:
- Fix only fault 2: repairs stop for unresolved links, but every other repair still re-stamps if the server does so. Not enough alone.
- Always replay a corrective revision on repair: more reads and a visible extra revision on every repair; rejected.
- Treat any set-but-different current value as correctable: hides #3270 wrongly migrated targets; rejected.

**Files likely to change**:
- `src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`
- `src/MigrationTools.Clients.TfsObjectModel/Processors/StateStampedFields.cs`
- `src/MigrationTools.Clients.TfsObjectModel/Tools/TfsGitRepositoryTool.cs` and `TfsWorkItemLinkTool.cs` (fault 2; needs adding to the Purpose's `paths` if accepted)
- `src/MigrationTools.Clients.TfsObjectModel.Tests/Processors/**`
- `tools/e2e/**` (scenario), `docs/content/docs/reference/processors/tfs-workitem-migration-processor/index.md` if behaviour text changes (regenerate with `GenerateDocs.ps1`)

**Tests to add or update**:
- Unit: a repair save with `correctStamps` false sets all state-stamped fields from the source.
- Unit: a catch-up revision with only a re-stamped value is corrected; a wrongly migrated revision still fails (#3270); a state mismatch is not "corrected".
- Unit: an in-sync item does no second read (#3299).
- Unit (fault 2): a source commit link left on the source because the target lacks the commit is not reported missing when the target holds it.
- End to end, on `main` and on the branch with exit codes, using the unresolved-link reproduction as the main scenario: repair of a Closed Task; a Resolved item and a reopened item; an item already wrongly stamped; a genuinely wrong target (must fail); a further re-run that is clean and reports every item in sync. Keep the Hierarchy and resolvable-commit repairs as controls.


## Questions

- Q-1 Q-1 — is the catch-up-save rule an acceptable definition of "safe to correct"? (for Martin) open
- Q-2 Q-3 — are `OtherWorkItemsSaved` saves in this item's Purpose? (for Martin) open
- Q-3 Q-6 — is the permanent "target differs" for an unresolved Git link (`GetTargetUri`) in this item's Purpose, or a separate follow-up? It is why the live items are repaired on every run. (for Martin) open
- Q-4 run it as the first step of the fix, to confirm the second half on the test orgs. (for Martin) open

