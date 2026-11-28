# Re-running the work item migration

How `TfsWorkItemMigrationProcessor` treats a work item that is already in the target. Code:
`src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs`.

## Outcomes

For each source work item found in the target, a re-run does one of these:

- **In sync**: every revision, field and link matches the source. Nothing is read again and nothing is saved (#3299).
- **New revisions**: the source has revisions the target lacks. They are replayed as on a first run.
- **Repair**: no revisions to sync, but the target differs (a missing link, attachment or embedded link, a field map
  result, or state-stamped values that are not the source's). The missing parts are added and the item is saved once,
  as a catch-up save: the previous revision's author, dated one second after it (#3192).

## State-stamped values

ClosedDate/By, ResolvedDate/By, ActivatedDate/By and StateChangeDate (`StateStampedFields`) must equal the source.

- **Every repair save sets them.** Before the save, after field maps and enrichers, each one is set to the source's
  latest value, and sent even when the target already has it. Otherwise Azure DevOps can stamp the save time on a
  save that does not change state, for example one that adds a Git commit link the target cannot resolve (#3326).
  The values are checked after the save and again by loading the target from the server.
- **Checking the target.** Every target revision is compared with the source revision it replays (#3270): the one
  dated within 10 ms after it, or else the latest one dated before it.
  - A value that is empty where the source has one (#3305), or that Azure DevOps stamped in a re-run's own catch-up
    save (`StateStampedFields.IsCatchUpSave`: it replays no source revision, and is dated one second after the
    revision before it by the same author; #3326), is history only. A warning lists it, and if it is in the latest
    revision the current values are set to the source's in a repair. Saved revisions cannot be changed.
  - Any other difference, or a State that differs, fails the work item with the revision, the field and both values.

## Git links left on the source

When the target repository lacks a linked commit or branch, `TfsGitRepositoryTool.Enrich` leaves the link pointing at
the source. On a re-run, `TfsWorkItemLinkTool.FindMissingLink` counts that raw link as present while the target still
lacks the artifact (`TfsGitRepositoryTool.TargetLacksArtifact`, using the same cached check as `VerifyTargetExists`),
so such an item reaches in sync instead of being repaired on every run (#3326). Once the commit or branch is in the
target, the link is missing again and the repair repoints it.

## Proof

End-to-end scenario `tools/e2e/scenarios/rerun-3326` (see `platform/specs/e2e-harness/scenarios.md`).
