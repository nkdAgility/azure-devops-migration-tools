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

## How revisions, attachments and links are saved

Each source revision is replayed as one target revision, with the source revision's `ChangedBy` and `ChangedDate`. Attachments and links are saved as part of the revision where they were added or removed in the source, so the migration does not add revisions dated after the source history:

- **Attachments** and **links** (hyperlinks, external and Git links, work item links) are added and removed at the matching revision.
- **Links to work items that are not migrated yet** are added later, from the other work item, when that one is migrated.
- **The migration comment** (`GenerateMigrationComment`) is added to the History of the last replayed revision.
- **Re-running** against work items that are already migrated adds no revisions unless something is missing, or a Closed, Activated or Resolved Date differs from the source (for example after a fix-up save failed). If so, it is saved as the last author, one second after the last revision.
- **Closed Date, Activated Date and Resolved Date** are stamped with the time of the save by Azure DevOps when a revision changes state, even with `BypassRules`, and are read-only in that revision. When the saved value differs from the source revision, the processor saves one fix-up revision straight after it, with the same author and `ChangedDate`, that sets the source value. The history then shows an extra revision after each activation, resolution or closing where the server stamped a different date: the first carries the migration time, and the fix-up carries the source date, which is the value from then on.

### Known limitations

- **Azure DevOps shows back-dated additions at revision 1.** When a revision's `ChangedDate` is set in the past, the work item history (and the `updates`/`revisions` REST APIs) list every *added* attachment and link under the first revision. The data itself is correct: each relation keeps the date of the revision it was added in. Removals are shown at the right revision. This is a service behaviour and cannot be changed by the tool.
- **A source that was itself migrated with back-dated history** has the same problem in its revision view: removals are not visible at the revision where they happened. The last replayed revision is compared with the source work item's current attachments and links, so the final state always matches the source, but such removals are shown at the last revision.
- **"Copy current user" rules overwrite identity fields.** Azure DevOps applies inherited process rules such as *"When State is Done, copy the current user to Assigned To"* on every save, even with `BypassRules` and even when the field value is sent. The migrated revision then shows the account running the migration. Disable these rules in the target process while you migrate, and turn them back on afterwards. The processor logs a warning at start-up for each rule of this kind it finds on the target work item types.

---

## WIQL Query

The Work Item queries are all built using Work Item [Query Language (WIQL)](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax). We only support flat queries that have `FROM WorkItems` in the query.

> Note: A useful Azure DevOps Extension to explore WIQL is the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor)

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
