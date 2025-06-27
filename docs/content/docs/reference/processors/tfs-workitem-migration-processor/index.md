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

---

## WIQL Query

The Work Item queries are all built using Work Item [Query Language (WIQL)](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax). We only support flat quereis that have `FROM WorkItems` in the query.

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

A complete list of [FieldMaps](../FieldMaps/index.md) are available.

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

## Removed Properties

- PrefixProjectToNodes - This option was removed in favour of the Area and Iteration Maps on [TfsNodeStructure]({{< ref "docs/reference/tools/tfs-node-structure-tool" >}})
