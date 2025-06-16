---
optionsClassName: TfsWorkItemMigrationProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsWorkItemMigrationProcessorOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.TfsWorkItemMigrationProcessorOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.TfsWorkItemMigrationProcessorOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsWorkItemMigrationProcessorOptions",
      "Enabled": false,
      "UpdateCreatedDate": true,
      "UpdateCreatedBy": true,
      "WIQLQuery": null,
      "FixHtmlAttachmentLinks": true,
      "WorkItemCreateRetryLimit": 5,
      "FilterWorkItemsThatAlreadyExistInTarget": false,
      "GenerateMigrationComment": true,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.TfsWorkItemMigrationProcessorOptions
description: WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments. Use `WorkItemMigrationConfig` to configure.
className: TfsWorkItemMigrationProcessor
typeName: Processors
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: FilterWorkItemsThatAlreadyExistInTarget
  type: Boolean
  description: This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run. While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart.
  defaultValue: true
- parameterName: FixHtmlAttachmentLinks
  type: Boolean
  description: "**beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML fields as well as discussion comments. You must specify a PersonalAccessToken in the Source project for Azure DevOps; TFS should use integrated authentication."
  defaultValue: true
- parameterName: GenerateMigrationComment
  type: Boolean
  description: If enabled, adds a comment recording the migration
  defaultValue: true
- parameterName: SourceName
  type: String
  description: This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor.
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a read only processor.
  defaultValue: missing XML code comments
- parameterName: UpdateCreatedBy
  type: Boolean
  description: "If this is enabled the creation process on the target project will create the items with the original creation date. (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate, not the internal create date)"
  defaultValue: true
- parameterName: UpdateCreatedDate
  type: Boolean
  description: "If this is enabled the creation process on the target project will create the items with the original creation date. (Important: The item history is always pointed to the date of the migration, it's change only the data column CreateDate, not the internal create date)"
  defaultValue: true
- parameterName: WIQLQuery
  type: String
  description: A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits)
  defaultValue: SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc
- parameterName: WorkItemCreateRetryLimit
  type: Int32
  description: '**beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count. This allows for periodic network glitches not to end the process.'
  defaultValue: 5
status: ready
processingTarget: Work Items
classFile: src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessor.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemMigrationProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TfsWorkItemMigrationProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TfsWorkItemMigrationProcessor/
title: TfsWorkItemMigrationProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: docs/Reference/Processors/TfsWorkItemMigrationProcessor-notes.md
  exists: true
  markdown: >
    ## <a name="WIQLQuery"></a>WIQL Query


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


    ## <a name="NodeBasePath"></a>NodeBasePath Configuration


    Moved to the TfsNodeStructure


    # Iteration Maps and Area Maps


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


    # Removed Properties


    - PrefixProjectToNodes - This option was removed in favour of the Area and Iteration Maps on [TfsNodeStructure](/Reference/v2/ProcessorEnrichers/TfsNodeStructure/)
- topic: introduction
  path: docs/Reference/Processors/TfsWorkItemMigrationProcessor-introduction.md
  exists: true
  markdown: >
    The `WorkItemMigrationContext` processor is used for migrating work items from one Azure DevOps instance to another. This encompasses a variety of activities:


    1. **Transferring Work Items Between Instances**: The primary purpose of the processor is to transfer work items, including bugs, tasks, user stories, features, and more, from one Azure DevOps instance to another.


    2. **Migrating Work Item History**: The processor can also replicate the entire revision history of work items, providing continuity and maintaining a record of changes.


    3. **Migrating Attachments and Links**: The processor can transfer any attachments or links associated with work items. This includes both external links and internal links to other work items.


    4. **Updating Metadata**: If configured, the processor can update the "Created Date" and "Created By" fields on migrated work items to match the original items in the source instance.


    5. **Filtering Work Items**: The processor can be configured to only migrate certain work items based on their area or iteration paths.


    Overall, the `WorkItemMigrationContext` processor is a comprehensive tool for transferring work items and their associated data and metadata between Azure DevOps instances. It should be used whenever there is a need to move work items between instances while preserving as much information as possible.

---