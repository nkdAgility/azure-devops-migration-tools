# Bug Assessment: Re-run repair save re-stamps ClosedDate on Closed work items

- **Slug**: 3326-repair-save-restamps-closeddate
- **Created**: 2026-11-23 (reassessed 2026-11-23, replaces the earlier assessment)
- **Source**: https://github.com/nkdAgility/azure-devops-migration-tools/issues/3326 (host: github.com, policy branch: allowlisted); item record `specs/3326-re-run-repair-save-re-stamps-closeddate/item.md`. Fetched earlier with the `gh` CLI, only for this issue; no comments.
- **Verdict**: valid (reproduced on `main`)
- **Severity**: high

## Report (summarized)

Issue #3326, labels Expedite, blocked, more-information-needed. On a re-run, a repair save on an already-migrated **Closed** work item re-stamps `Microsoft.VSTS.Common.ClosedDate` with the save time. Seen on a live migration (16.3.6-Preview.5, `main` 0e017174) on 20 and 23 November: 24 Closed items across two projects. The re-run logs "Work item exists with no revisions to sync, but the target differs: missing external link `vstfs:///Git/Commit/...`" (or a missing Hierarchy link) and saves one extra revision in which `ClosedDate` moves from the source value to the save time. State stays Closed; StateChangeDate, ActivatedDate and ResolvedDate are unchanged. The next re-run fails the item with "stamped instead of the source values: revision N ClosedDate ...".

Expected: every repair save sets the state-stamped fields (ClosedDate/By, ResolvedDate/By, ActivatedDate/By, StateChangeDate) to the source values; a re-run corrects a target already stamped wrongly where that is safe; no REST in the production path; no stack traces. Related: #3270, #3305, #3299.

Live conditions, from the customer logs (Q-4, Q-5 answered): BypassRules on, no custom rules (inherited Agile Task), Azure DevOps Services, and every stamping save adds a Git commit ArtifactLink that the migration could not resolve in the target.

## Symptom

A re-run that repairs a missing link on an already-migrated Closed item saves a new revision in which `ClosedDate` is the save time, not the source value. The following re-run fails that item, so the user cannot get a clean run without deleting it. Expected: the repair save keeps the source's state-stamped values, and a re-run corrects a target already stamped wrongly.

## Reproduction

Reproduced on `main` (origin/main 78147078, local build 16.3.6-Local.7, test orgs, process `amte2e-agile`, BypassRules on, 23 November). Scratch projects `amt3326-gsrc` / `amt3326-gtgt`.

1. Push a commit to the source repo only. Link 2 inherited-Agile Tasks to it and take them New -> Active -> Closed.
2. Migrate with `tools/e2e/Invoke-MigrationE2E.ps1` (built `MigrationTools.ConsoleFull`, TfsGitRepositoryTool mapping on). Both items created, exit 0. The enricher warns "commit ... not found in target repo amt3326-gtgt/amt3326-gtgt; leaving link vstfs:///Git/Commit/<source project id>%2F<source repo id>%2F<sha> pointing at the source".
3. Remove that commit link on target #13970 (target revision 4).
4. Re-run. Log: "target differs: missing external link", 1 updated, exit 0. Target revision 5: ChangedDate 13:18:13 (revision 4 + 1 s), **ClosedDate 13:19:19, the save time**; source and target revision 3 have 13:16:08. State stays Closed.

Controls that do **not** reproduce on the same orgs and process: a Hierarchy-Reverse link repair on 2 Closed User Stories (revision 5 kept ClosedDate), and a Git commit link repair where the commit exists in the target and is rewritten to the target repo (revision 6 kept ClosedDate). Full output and exit codes: item.md, Reproduction.

Not yet run: step 5 of the issue (a second re-run failing with "stamped instead of the source values"); the code says it must (see `VerifyMigratedRevisions`), but it is not observed. [NEEDS CLARIFICATION: run it as the first step of the fix, to confirm the second half on the test orgs.]

Also seen: with the unresolved link present on the target, every re-run reports "target differs: missing external link" although nothing is missing, so the item is never reported in sync.

## Suspected Code Paths

- `TfsWorkItemMigrationProcessor.cs:969-972` — `ApplyStateStampedFieldsFromSource` runs only `if (correctStamps)`. Every other repair save (links, attachments, embedded links, field maps, hyperlink) leaves the state-stamped fields unset. This is the code gap the issue names. It is now shown to matter on a live-like repair: the unresolved-link repair re-stamps.
- `TfsWorkItemMigrationProcessor.cs:973` and `StampCatchUpSave` (~1214) — sets only `ChangedBy` and `ChangedDate`.
- `TfsWorkItemMigrationProcessor.cs:991-1006` — the save and the post-save verification, both gated on `stampCorrection != null`.
- `TfsWorkItemMigrationProcessor.cs:1858-1937` (`VerifyMigratedRevisions`) — a set-but-different value goes to `wrong` and fails the item; it cannot tell our own catch-up save from a wrongly migrated target (#3270). This is the "next re-run fails" half.
- `TfsWorkItemMigrationProcessor.cs:1739` (`GetStateStampedTargetValues`), `:1795` (`ApplyStateStampedFieldsFromSource`), `:1690`/`:1949` (`VerifyStateStampedFieldsSaved`, `VerifyStateStampedValuesStored`) — existing machinery to reuse.
- `TfsGitRepositoryTool.cs:222-235` (`GetTargetUri`) with `:409-411` — `GetTargetUri` calls `MapGitLinkToTargetUri(..., report: false)`, so `verifyTarget = report && Options.VerifyTargetExists` is false and the target-existence check is skipped. It returns the rewritten target URI even when the commit is not in the target. `Enrich` (`:144`, report on) does verify and leaves the **source** URI on the target. So `TfsWorkItemLinkTool.HasExternalLink` (`TfsWorkItemLinkTool.cs:199-215`) looks for the rewritten URI, never finds the raw one the target holds, and `FindMissingLink` returns "external link ..." on every run. This is the cause of the permanent "target differs" (new finding; hypothesis from reading, not yet confirmed by a debug run).
- `TfsWorkItemLinkTool.cs:321-362` (`CreateExternalLink`) — re-adds the link; `SaveAfterEachLink` (`:27`, off by default) would save before the stamps are applied, so check it for the repair path.
- `TfsWorkItemMigrationProcessor.cs:476` — BypassRules not on: warns only. Now ruled out as the live cause (BypassRules is on live).
- `ProcessWorkItemLinks(..., true)` / `OtherWorkItemsSaved` (958) — saves of the other end of a link; not covered by the item's own stamping (Q-3).

## Root Cause Hypothesis

Two faults.

1. **The re-stamp.** A repair save on an existing target leaves the state-stamped fields unset unless the target's current values were empty (#3305). With BypassRules on and no custom rules, the service still writes the save time into `ClosedDate` when the repair re-adds an ArtifactLink that points at the source project and repo. A Hierarchy link and a resolvable commit link are saved without re-stamping, so what differs is the link, not the state rule. Why the service re-stamps for that link is **not** pinned down (candidates: the server treating an artifact link to a foreign project as a different kind of save, or a second internal save). The next run's check then sees a set but different value and fails the item. Confidence: high that the unset fields are the gap and that setting them on every repair save is correct; **medium** that setting them stops the re-stamp, because the cause of the re-stamp itself is unproven. The fix must be shown on the reproduction above, not assumed.
2. **The permanent "differs".** `GetTargetUri` skips the target-existence check, so an unresolved link is reported missing on every run, and a repair is attempted every time, so the item never reaches in sync. Confidence: medium-high from the code; confirm with a debug run.

Fault 2 is what makes fault 1 reachable on every re-run for the 24 live items; it also means the re-stamp can recur.

## Proposed Remediation

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

## Risks & Considerations

- The cause of the server's re-stamp for this link type is unproven; setting the fields might not stop it. The reproduction is the test, and the Definition of Done needs it passing on the branch.
- Correcting a current value does not rewrite history; the earlier wrongly stamped revision stays in the target.
- One extra read per repaired item (post-save verification); in-sync items must not pay it.
- Changing `GetTargetUri` affects revision replay (#3192), which uses it to find the target link to remove when the source removes a Git link; check that path does not start leaving or removing the wrong link.
- Other-end-of-link saves (`OtherWorkItemsSaved`) may re-stamp too; whether that is in Purpose is Q-3.
- `TfsWorkItemOverwriteProcessor` and bulk edit are out of scope; hand back as follow-ups only if they share the unguarded save.

## Open Questions

- [NEEDS CLARIFICATION: Q-1 — is the catch-up-save rule an acceptable definition of "safe to correct"?]
- [NEEDS CLARIFICATION: Q-3 — are `OtherWorkItemsSaved` saves in this item's Purpose?]
- [NEEDS CLARIFICATION: Q-6 — is the permanent "target differs" for an unresolved Git link (`GetTargetUri`) in this item's Purpose, or a separate follow-up? It is why the live items are repaired on every run.]

Answered since the earlier assessment: Q-4 (live conditions) and Q-5 (outcome check); BypassRules as the cause is ruled out.
