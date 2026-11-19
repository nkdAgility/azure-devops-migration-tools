---
title: Tfs WorkItem Migration Processor
description: WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments. Use `WorkItemMigrationConfig` to configure.
dataFile: reference.processors.tfsworkitemmigrationprocessor.yaml
slug: tfs-workitem-migration-processor
aliases:
  - /docs/Reference/Processors/TfsWorkItemMigrationProcessor
  - /Reference/Processors/TfsWorkItemMigrationProcessor
  - /learn/azure-devops-migration-tools/Reference/Processors/TfsWorkItemMigrationProcessor
  - /learn/azure-devops-migration-tools/Reference/Processors/TfsWorkItemMigrationProcessor/index.md
date: 2025-06-24T12:07:31Z
discussionId: 2681
---

{{< class-description >}}

## Options

{{< class-options >}}

## Samples

### Sample

{{< class-sample sample="sample" >}}

### Defaults

{{< class-sample sample="defaults" >}}

### Classic

{{< class-sample sample="classic" >}}

## Metadata

{{< class-metadata >}}

## Schema

{{< class-schema >}}

## More Information

The `WorkItemMigrationContext` processor is used for migrating work items from one Azure DevOps instance to another. This encompasses a variety of activities:

1. **Transferring Work Items Between Instances**: The primary purpose of the processor is to transfer work items, including bugs, tasks, user stories, features, and more, from one Azure DevOps instance to another.

2. **Migrating Work Item History**: The processor can also replicate the entire revision history of work items, providing continuity and maintaining a record of changes.

3. **Migrating Attachments and Links**: The processor can transfer any attachments or links associated with work items. This includes both external links and internal links to other work items.

4. **Updating Metadata**: If configured, the processor can update the "Created Date" and "Created By" fields on migrated work items to match the original items in the source instance.

5. **Filtering Work Items**: The processor can be configured to only migrate certain work items based on their area or iteration paths.

Overall, the `WorkItemMigrationContext` processor is a comprehensive tool for transferring work items and their associated data and metadata between Azure DevOps instances. It should be used whenever there is a need to move work items between instances while preserving as much information as possible.

## How migrated work items are found again

On a re-run, with `FilterWorkItemsThatAlreadyExistInTarget`, and when Test Case steps, parameters and links point at other work items, the processor finds the target work item by the reflected work item ID: the source work item's URL, stored in the target's `ReflectedWorkItemIdField`.

At the start of the run the processor reads the reflected work item ID of every target work item with one query, and logs `Found N work items already migrated to the target`. From then on a migrated item, or the other end of a link, is loaded by its target ID, and an item that is not in the map is known not to be in the target, without a query per item. With `AllowCrossProjectLinking` the map covers every project in the target collection. Items created during the run are added to it. If two target items hold the reflected work item ID of the same source item, that item is looked up by query as before.

A work item that is already migrated is **in sync** when it has no revisions to replay, every replayed revision matches the source (see Re-running below), the target has every link of the source (a link to an item that is not in the target is not counted), it has every source attachment by name and size, and running the current field maps on it would change nothing. The field maps run in memory for that check, so a target that already holds the mapped values is in sync, and a field map added or changed since the first run is still applied: the item is updated and logs `the target differs: field maps change <fields>`. An in-sync item is left alone: no enrichers, no link migration and no save. Otherwise only the normal work is done for it: missing revisions are replayed, and missing links are added, which also moves a child to the source's parent when it sits under another one in the target. Test Cases are always checked, for their shared steps and parameters. At the end the run logs `Work items: N in sync and untouched, N checked with nothing to change, N updated, N created, N failed`. "Checked with nothing to change" is an item that went through that work but needed no save, such as a Test Case whose references were already right, or an item whose missing link could not be added. An in-sync item does no link work, so the link skip warnings (for example for a linked item the migration identity cannot read) appear only in the run that migrates or updates it.

`Code Review Request`, `Code Review Response`, `Feedback Request`, `Feedback Response`, `Shared Parameter` and `Shared Steps` can't have custom fields on an inherited process. When one of these types lacks your `ReflectedWorkItemIdField` in the target, the reflected work item ID is written to the built-in `Microsoft.VSTS.Build.IntegrationBuild` field instead, and lookups search that field for those six types. So a re-run does not create a second copy of a Shared Steps or Shared Parameter item. In the Test Cases it processes, a step or parameter reference is rewired only when the referenced Shared Steps or Shared Parameter is already in the target and is found by its reflected work item ID. If it is outside the query and was never migrated, the reference keeps the source ID.

`FilterWorkItemsThatAlreadyExistInTarget: true` runs your `WIQLQuery` against the target (with the project name replaced) and removes the source items whose reflected work item ID it finds there. A target item that the query does not return, for example a closed Test Case when the query selects only active ones, is not filtered out: it is processed again and its references can be rewritten. Set the option to `false` to process every item again. References are rewritten by replacing each source Shared Steps ID in the target's step text (`ref="<source id>"`) with the target ID. If source and target IDs overlap, a reference that is already correct can be changed to the wrong item, so check the Test Cases after such a run.

Writing the reflected work item ID to `Microsoft.VSTS.Build.IntegrationBuild` replaces any value already in that field, with the warning `Overwriting 'Microsoft.VSTS.Build.IntegrationBuild' value ...`. If you use that field for build data, export it before migrating. Any other type without your field still fails validation. See [Migrating Plans and Suites]({{< ref "docs/how-to/migrating-plans-and-suites" >}}).

## How revisions, attachments and links are saved

Each source revision is replayed as one target revision, with the source revision's `ChangedBy` and `ChangedDate`. Attachments and links are saved as part of the revision where they were added or removed in the source, so the migration does not add revisions dated after the source history:

- **Attachments** and **links** (hyperlinks, external and Git links, work item links) are added and removed at the matching revision.
- **Links to work items that are not migrated yet** are added later, from the other work item, when that one is migrated.
- **The migration comment** (`GenerateMigrationComment`) is added to the History of the last replayed revision.
- **The migration hyperlink** (`GenerateMigrationHyperlink`, on by default) is a hyperlink with the comment `Migrated from source` to the web page of the source work item. It never adds a revision of its own: when revisions are replayed it goes into the last replayed revision, and otherwise into the catch-up save that a later migration run makes when it saves the item for another reason. A re-run with nothing to save does not add it, and neither does a save made in Azure DevOps or a run that filters the item out. Each item keeps exactly one: if the source URL has changed, the old migration hyperlink is replaced. The exception is decided by location: a stale migration hyperlink is kept whenever the source work item has a hyperlink at that same URL, whatever its comment (for example because the source was itself migrated), so the target can then have two. If the target already has a hyperlink to the source URL with a different comment, it counts as present and is neither duplicated nor changed, so that item has no `Migrated from source` comment. Setting the option to `false` turns the link off.
- **Re-running** against work items that are already migrated adds no revisions unless something is missing. If so, it is saved as the last author, one second after the last revision. A re-run compares every revision of the target work item with the source revision it replays (the type-change save, fix-up and catch-up saves included): the State, the Activated/Resolved/Closed Date and the Activated/Resolved/Closed By. If any differ (for example because an earlier version stamped the migration time), the work item fails with the revision, the field and both values. It is not corrected, because a correction would be an after-save fix-up. If the target holds the service's own date or the migration identity, delete the work item from the target and migrate it again; if an identity differs because of the user mapping, fix the mapping first (see below). The exception is a target that an earlier version saved with an *empty* state-stamped value where the source has one: the re-run logs a warning listing those revisions and, if the current value is empty, sets the current values to the source's latest ones in a catch-up save, with no delete. Azure DevOps cannot change saved revisions, so the earlier revisions keep the empty value in the history. By fields are compared as accounts, not as text: the object model reads a target identity back as its display name, so when the user mapping gives `Display Name <domain\account>` or a bare account, the processor looks the display name up in the target and compares the accounts. If every account with that display name is the expected account, the value matches; if the target has at least one account with that name and none of them is the expected one, the work item fails. If no account with that name can be found or read, or only some of the accounts that share the name are the expected one, the value is not reported as wrong: a warning names it and the work item is treated as migrated. An identity that differs because of the user mapping comes back the same after a new migration, so first fix the mapping and any target process rule that copies the current user into these fields. `FilterWorkItemsThatAlreadyExistInTarget` is ignored (a warning is logged when it is `true`), so items already in the target are always checked.
- **State-stamped fields** (Activated, Resolved, Closed and State Change Date, and Activated, Resolved and Closed By) carry the source revision's values in the same save that replays the revision, including the save that changes state. Otherwise Azure DevOps would stamp the time of the migration and the migration identity, and they cannot be corrected afterwards. By fields go through the user mapping. An empty source value stays empty on the target. Each value read for a source revision is checked against a second read of the same revision (the work item loaded again through the object model, so this works against servers without a REST API): in a long run a value could be read as empty although the source has it. If one read is empty and the other has a value, the value is used, with a warning; if the two reads differ, or the source cannot be read again, the work item fails with the reason instead of being saved unchecked. There are no fix-up revisions. The object model refuses these dates in a revision that changes state (TF26194), so the processor sets them below that client-side check, and the service keeps them because the connection bypasses rules.
- **Saving a wrong state-stamped value is a failure.** Before migrating, the run stops if the migration identity does not have **Bypass rules on work item updates** in the target, because every activated, resolved or closed item would then get the migration time and identity. After each replayed revision is saved, including the REST save that changes the work item type, the processor checks that the saved revision kept the source values. If not, the work item fails with the field, the source value and the saved value. That check reads the object model's copy of the saved revision, so after the last revision of each work item the target is also loaded again from the server, and every saved revision must read back with the values it was saved with. By fields are compared as accounts in the same way as on a re-run, so an identity that cannot be proven either way (the account behind the display name cannot be read, or the name is shared and only some of those accounts are the expected one) gives a warning instead of a failure. Check that the user mapping names the account you expect and that no target process rule copies the current user or sets these fields, then delete the target work item and migrate it again; a re-run reports it again until you do. A catch-up save on a re-run changes no state, so the service stamps nothing there and it is not checked.
- **When a work item cannot be saved**, the log shows one error line. It gives the source work item, the revision, the type and the target work item if there is one, then the reason the target gave and what to do about it. For example, a `Bug` whose type is disabled in the target gives `VS403417`, with advice to enable the type or map it with `WorkItemTypeMappingTool`. Stack traces and the target field values are logged only at Serilog `MinimumLevel` `Debug`.

### Known limitations

- **Azure DevOps shows back-dated additions at revision 1.** When a revision's `ChangedDate` is set in the past, the work item history (and the `updates`/`revisions` REST APIs) list every *added* attachment and link under the first revision. The data itself is correct: each relation keeps the date of the revision it was added in. Removals are shown at the right revision. This is a service behaviour and cannot be changed by the tool.
- **A source that was itself migrated with back-dated history** has the same problem in its revision view: removals are not visible at the revision where they happened. The last replayed revision is compared with the source work item's current attachments and links, so the final state always matches the source, but such removals are shown at the last revision.
- **"Copy current user" rules overwrite identity fields.** Azure DevOps applies inherited process rules such as *"When State is Done, copy the current user to Assigned To"* on every save, even with `BypassRules` and even when the field value is sent. The migrated revision then shows the account running the migration. Disable these rules in the target process while you migrate, and turn them back on afterwards. The processor logs a warning at start-up for each rule of this kind it finds on the target work item types.

## Work items that need review

Every run writes the work items that need a person to look at them to `work-items-to-review.jsonl` in the run's log folder (`logs\<timestamp>`). The path is logged at the start of the run, before any work item is migrated. Each entry is appended and flushed to disk as soon as the problem is found, so the file is correct up to the last problem even if the run crashes, is stopped or the process is killed. The one exception is a link to an item that is migrated later in the same run: that entry is held in memory and written only if that item fails or the run ends without reaching it, so it is lost if the process is killed before then. A re-run adds the link once both items are in the target. If a row cannot be written (for example the disk is full or access is denied), the run logs `Could not write to the work item review file` and carries on; the row is written only if the same problem is found again later in the run. If that warning appears, treat the review files for that run as incomplete. A later run that finds a line it cannot read in the middle of an earlier review file writes no `resolved` entries. A run with no problems creates no file.

The file is JSON Lines: one JSON object per line, with these fields.

| Field | Meaning |
|---|---|
| `time` | When the problem was found. |
| `sourceId` | The source work item id. |
| `targetId` | The target work item id. Blank if the target item was not created, or the problem was found before its first save. |
| `workItemType` | The source work item type. |
| `revision` | The source revision being migrated when the problem was found. |
| `category` | What kind of problem it is (below). |
| `reason` | The message, as in the log. |
| `action` | What to do about it. |
| `migration` | The source and target collection and project, so re-runs only read entries for the same migration. |

| Category | When it is written |
|---|---|
| `failed` | The work item could not be created in the target. |
| `part-migrated` | The target work item exists but a later revision could not be saved. |
| `update-failed` | The target item has every revision, but a later step for it failed (attachments, links or the catch-up save). It resolves when the item next completes. |
| `rerun-mismatch` | On a re-run, a revision already in the target does not match the source. |
| `link-skipped` | A link was not migrated: the related item was not migrated (a link to an item that migrates later in the run is added from its side, so it is held and written only if that item fails or the run ends without reaching it; a killed run loses it), cannot be read, or its link type does not exist in the target; or a hyperlink is not valid. |
| `link-left-on-source` | A git commit, branch or pull request link could not be rewritten and still points at the source. |
| `field-not-mapped` | A source field has a value but does not exist on the target work item type, so the value was dropped. |
| `field-truncated` | A string value was cut to `StringManipulatorTool` `MaxStringLength`. |
| `field-map-failed` | A field map threw an error (logged as `Field map fault`), so its value was not set. The entry names the map and a short hash of its configuration; it resolves only when that same map, with the same configuration, runs cleanly. If you change the map's configuration to fix it, the old entry stays open in the latest view: the review log cannot tell a corrected map from a different one. |
| `user-not-mapped` | An identity value matches more than one entry in the user mapping file, or is a source user that does not exist in the target and is not mapped. |
| `attachment-not-migrated` | An attachment is bigger than `MaxAttachmentSize`, could not be downloaded, or failed. |
| `embedded-image-not-migrated` | An image embedded in an HTML or Markdown field could not be downloaded or fixed, so it still points at the source. |
| `path-not-mapped` | An area or iteration path does not exist in the target and is not mapped, or a revision was not saved because of it (`SkipRevisionWithInvalidAreaPath`/`SkipRevisionWithInvalidIterationPath`). |
| `type-changed` | The item was created, or a revision kept, as another type because its type does not exist or is disabled in the target. |
| `skipped` | A Test Plan or Test Suite, which this processor does not migrate. |
| `resolved` | On a re-run, an item that an earlier run listed migrated without those problems. |

The same problem for the same item is written once per run.

**Follow-up files.** Problems that someone must deal with after the migration are also written, in the same way, to one file per concern in the same folder: `follow-up-links.jsonl`, `follow-up-users-not-mapped.jsonl`, `follow-up-attachments.jsonl`, `follow-up-fields.jsonl`, `follow-up-area-iteration-paths.jsonl` and `follow-up-skipped.jsonl`. A file with no rows is not created. Item failures, part-migrations, update failures, re-run mismatches and type changes have no follow-up file, so check `work-items-to-review.jsonl` and the category counts at the end of the run, not only the follow-up files, to know whether anything is left to do.

**End of the run.** The log gives the number of entries by category, and each follow-up file with its row count. It also writes `work-items-to-review-latest.jsonl`, the latest entry for each item across this run and the earlier runs. It is a summary only: it holds one entry per item, so an item with several open problems shows only the most recent one. Use the per-run files for the full list.

**Re-runs.** Each run writes its own file. A run reads the review files of earlier runs in the same `logs` folder, but only the entries written for the same source and target collection and project (the `migration` field), because work item ids are only unique within one migration. When an item they listed now migrates with no problems, it gets a `resolved` entry. Problems that are only found while replaying a revision (`failed`, `part-migrated`, `field-not-mapped`, `field-map-failed`, `field-truncated`, `user-not-mapped`, `path-not-mapped`, `type-changed`) count as checked again only when the revision they were found on is replayed in this run; a missing path found by validation counts as checked when validation runs and passes (it does not run when `TfsNodeStructureTool` is disabled). Every other problem counts as checked again only when its check ran for the item in this run: a disabled attachment or link tool does not resolve an attachment or link problem. No `resolved` entry is written until every earlier problem has been checked again. A newer problem can replace the earlier entry in the latest view, so check the per-run files for the full list. If an earlier review log cannot be read, the run logs a warning and writes no `resolved` entries for that migration, because the unread log may list a problem that was not checked again. Run from the same folder, or pass the same `--logPath`, for earlier runs to be found.
---

## WIQL Query

The Work Item queries are all built using Work Item [Query Language (WIQL)](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax). We only support flat queries that have `FROM WorkItems` in the query.

> Note: A useful Azure DevOps Extension to explore WIQL is the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor)

Work items are migrated in the order the query returns them, except that `Shared Steps` and `Shared Parameter` items are migrated first. Test Case steps refer to them by id, and those references can only be pointed at the migrated copies once the copies exist.

### Examples

You can use the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor) to craft a query in Azure DevOps.

A simple example config:

```
"WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc"
```

Scope to Area Path (Team data):

```
"WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.AreaPath] UNDER 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc"
```

Limit to specific work items (for testing purposes)

```
"WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.Id] in (123,456,789) AND [System.TeamProject] = @TeamProject AND [System.AreaPath] UNDER 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc"
```

## NodeBasePath Configuration

Moved to the TfsNodeStructure

## Iteration Maps and Area Maps

Moved to the TfsNodeStructure

## More Complex Team Migrations

The above options allow you to bring over a sub-set of the WIs (using the `WIQLQueryBit`) and move their area or iteration path to a default location. However you may wish to do something more complex e.g. re-map the team structure. This can be done with addition of a `FieldMaps` block to configuration in addition to the `NodeBasePaths`.

Using the above sample structure, if you wanted to map the source project `Team 1` to target project `Team A` etc. you could add the field map as follows


A complete list of [FieldMaps]({{< ref "docs/reference/field-maps" >}}) is available.

```
 "FieldMaps": [
   {
      "$type": "FieldValueMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.AreaPath",
      "targetField": "System.AreaPath",
      "defaultValue": "TargetProg",
      "valueMapping": {
        "SampleProj\\Team 1": "TargetProg\\Team A",
        "SampleProj\\Team 2": "TargetProg\\Team B"
        "SampleProj\\Team 3": "TargetProg\\Team C"
      }
    },
  ],

```

> Note: These mappings could also be achieved with other forms of Field mapper e.g. `RegexFieldMapConfig`, but the value mapper as an example is easy to understand

## Identity Fields

Earlier versions saved each work item again as `Migration` after the replay, to add attachments, links and the migration comment, and process rules could then overwrite identity fields such as `System.AssignedTo`, `Microsoft.VSTS.Common.ResolvedBy` and `Microsoft.VSTS.Common.ClosedBy` with the account running the migration. Those saves no longer happen (see [How revisions, attachments and links are saved](#how-revisions-attachments-and-links-are-saved)), so every target revision keeps the source revision's author.

Process rules that *copy the current user* into an identity field still apply on every save, even with `BypassRules`, so the migration account can appear in that field on the revisions where the rule's condition holds. The processor warns about each such rule at start-up; disable them in the target process while migrating. If you still see identity fields set to the migration account, also check the [TfsUserMappingTool]({{< ref "docs/reference/tools/tfsusermappingtool" >}}) configuration.

## Removed Properties

- PrefixProjectToNodes - This option was removed in favour of the Area and Iteration Maps on [TfsNodeStructure]({{< ref "docs/reference/tools/tfsnodestructuretool" >}})
