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

`Code Review Request`, `Code Review Response`, `Feedback Request`, `Feedback Response`, `Shared Parameter` and `Shared Steps` can't have custom fields on an inherited process. When one of these types lacks your `ReflectedWorkItemIdField` in the target, the reflected work item ID is written to the built-in `Microsoft.VSTS.Build.IntegrationBuild` field instead, and lookups search that field for those six types. So a re-run does not create a second copy of a Shared Steps or Shared Parameter item. In the Test Cases it processes, a step or parameter reference is rewired only when the referenced Shared Steps or Shared Parameter is already in the target and is found by its reflected work item ID. If it is outside the query and was never migrated, the reference keeps the source ID.

`FilterWorkItemsThatAlreadyExistInTarget: true` runs your `WIQLQuery` against the target (with the project name replaced) and removes the source items whose reflected work item ID it finds there. A target item that the query does not return, for example a closed Test Case when the query selects only active ones, is not filtered out: it is processed again and its references can be rewritten. Set the option to `false` to process every item again. References are rewritten by replacing each source Shared Steps ID in the target's step text (`ref="<source id>"`) with the target ID. If source and target IDs overlap, a reference that is already correct can be changed to the wrong item, so check the Test Cases after such a run.

Writing the reflected work item ID to `Microsoft.VSTS.Build.IntegrationBuild` replaces any value already in that field, with the warning `Overwriting 'Microsoft.VSTS.Build.IntegrationBuild' value ...`. If you use that field for build data, export it before migrating. Any other type without your field still fails validation. See [Migrating Plans and Suites]({{< ref "docs/how-to/migrating-plans-and-suites" >}}).

## How revisions, attachments and links are saved

Each source revision is replayed as one target revision, with the source revision's `ChangedBy` and `ChangedDate`. Attachments and links are saved as part of the revision where they were added or removed in the source, so the migration does not add revisions dated after the source history:

- **Attachments** and **links** (hyperlinks, external and Git links, work item links) are added and removed at the matching revision.
- **Links to work items that are not migrated yet** are added later, from the other work item, when that one is migrated.
- **The migration comment** (`GenerateMigrationComment`) is added to the History of the last replayed revision.
- **The migration hyperlink** (`GenerateMigrationHyperlink`, on by default) is a hyperlink with the comment `Migrated from source` to the web page of the source work item. It never adds a revision of its own: when revisions are replayed it goes into the last replayed revision, and otherwise into the catch-up save that a later migration run makes when it saves the item for another reason. A re-run with nothing to save does not add it, and neither does a save made in Azure DevOps or a run that filters the item out. Each item keeps exactly one: if the source URL has changed, the old migration hyperlink is replaced. The exception is decided by location: a stale migration hyperlink is kept whenever the source work item has a hyperlink at that same URL, whatever its comment (for example because the source was itself migrated), so the target can then have two. If the target already has a hyperlink to the source URL with a different comment, it counts as present and is neither duplicated nor changed, so that item has no `Migrated from source` comment. Setting the option to `false` turns the link off.
- **Re-running** against work items that are already migrated adds no revisions unless something is missing. If so, it is saved as the last author, one second after the last revision.
- **State-stamped fields** (Activated, Resolved and Closed Date, and Activated, Resolved and Closed By) carry the source revision's values in the same save that replays the revision, including the save that changes state. Otherwise Azure DevOps would stamp the time of the migration and the migration identity, and they cannot be corrected afterwards. By fields go through the user mapping. An empty source value stays empty on the target. There are no fix-up revisions. The object model refuses these dates in a revision that changes state (TF26194), so the processor sets them below that client-side check, and the service keeps them because the connection bypasses rules.
- **Saving a wrong state-stamped value is a failure.** Before migrating, the run stops if the migration identity does not have **Bypass rules on work item updates** in the target, because every activated, resolved or closed item would then get the migration time and identity. After each save, the processor checks that the saved revision kept the source values. If not, the work item fails with the field, the source value and the saved value.

### Known limitations

- **Azure DevOps shows back-dated additions at revision 1.** When a revision's `ChangedDate` is set in the past, the work item history (and the `updates`/`revisions` REST APIs) list every *added* attachment and link under the first revision. The data itself is correct: each relation keeps the date of the revision it was added in. Removals are shown at the right revision. This is a service behaviour and cannot be changed by the tool.
- **A source that was itself migrated with back-dated history** has the same problem in its revision view: removals are not visible at the revision where they happened. The last replayed revision is compared with the source work item's current attachments and links, so the final state always matches the source, but such removals are shown at the last revision.
- **"Copy current user" rules overwrite identity fields.** Azure DevOps applies inherited process rules such as *"When State is Done, copy the current user to Assigned To"* on every save, even with `BypassRules` and even when the field value is sent. The migrated revision then shows the account running the migration. Disable these rules in the target process while you migrate, and turn them back on afterwards. The processor logs a warning at start-up for each rule of this kind it finds on the target work item types.

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
