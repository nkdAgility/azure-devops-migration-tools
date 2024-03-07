---
optionsClassName: WorkItemMigrationConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.WorkItemMigrationConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "WorkItemMigrationConfig",
      "Enabled": false,
      "UpdateCreatedDate": true,
      "UpdateCreatedBy": true,
      "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
      "FixHtmlAttachmentLinks": false,
      "SkipToFinalRevisedWorkItemType": false,
      "WorkItemCreateRetryLimit": 5,
      "FilterWorkItemsThatAlreadyExistInTarget": false,
      "PauseAfterEachWorkItem": false,
      "AttachRevisionHistory": false,
      "LinkMigrationSaveEachAsAdded": false,
      "GenerateMigrationComment": true,
      "WorkItemIDs": null,
      "MaxGracefulFailures": 0,
      "SkipRevisionWithInvalidIterationPath": false,
      "SkipRevisionWithInvalidAreaPath": false
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.WorkItemMigrationConfig
description: WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments. Use `WorkItemMigrationConfig` to configure.
className: WorkItemMigrationContext
typeName: Processors
architecture: v1
options:
- parameterName: AttachRevisionHistory
  type: Boolean
  description: This will create a json file with the revision history and attach it to the work item. Best used with `MaxRevisions` or `ReplayRevisions`.
  defaultValue: '?'
- parameterName: Enabled
  type: Boolean
  description: If enabled then the processor will run
  defaultValue: false
- parameterName: FilterWorkItemsThatAlreadyExistInTarget
  type: Boolean
  description: This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run. While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart.
  defaultValue: true
- parameterName: FixHtmlAttachmentLinks
  type: Boolean
  description: "**beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML fields as well as discussion comments. You must specify a PersonalAccessToken in the Source project for Azure DevOps; TFS should use integrated authentication."
  defaultValue: '?'
- parameterName: GenerateMigrationComment
  type: Boolean
  description: If enabled, adds a comment recording the migration
  defaultValue: false
- parameterName: LinkMigrationSaveEachAsAdded
  type: Boolean
  description: "If you have changed parents before re-running a sync you may get a `TF26194: unable to change the value of the 'Parent' field` error. This will resolve it, but will slow migration."
  defaultValue: false
- parameterName: MaxGracefulFailures
  type: Int32
  description: The maximum number of failures to tolerate before the migration fails. When set above zero, a work item migration error is logged but the migration will continue until the number of failed items reaches the configured value, after which the migration fails.
  defaultValue: 0
- parameterName: PauseAfterEachWorkItem
  type: Boolean
  description: Pause after each work item is migrated
  defaultValue: false
- parameterName: SkipRevisionWithInvalidAreaPath
  type: Boolean
  description: When set to true, this setting will skip a revision if the source area has not been migrated, has been deleted or is somehow invalid, etc.
  defaultValue: missng XML code comments
- parameterName: SkipRevisionWithInvalidIterationPath
  type: Boolean
  description: This will skip a revision if the source iteration has not been migrated i.e. it was deleted
  defaultValue: missng XML code comments
- parameterName: SkipToFinalRevisedWorkItemType
  type: Boolean
  description: "**beta** If enabled this will fix any image attachments URL's, work item mention URL's or user mentions in the HTML fields as well as discussion comments. You must specify a PersonalAccessToken in the Source project for Azure DevOps; TFS should use integrated authentication."
  defaultValue: false
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
  defaultValue: SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [[System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc
- parameterName: WorkItemCreateRetryLimit
  type: Int32
  description: '**beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count. This allows for periodic network glitches not to end the process.'
  defaultValue: 5
- parameterName: WorkItemIDs
  type: IList
  description: A list of work items to import
  defaultValue: '[]'
status: ready
processingTarget: Work Items
classFile: /src/VstsSyncMigrator.Core/Execution/MigrationContext/WorkItemMigrationContext.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/WorkItemMigrationConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/WorkItemMigrationContext/
title: WorkItemMigrationContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/WorkItemMigrationContext-notes.md
  exists: true
  markdown: >-
    ## <a name="WIQLQueryBits"></a>WIQL Query Bits


    The Work Item queries are all built using Work Item [Query Language (WIQL)](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax).


    > Note: A useful Azure DevOps Extension to explore WIQL is the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor)


    ### Examples


    You can use the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor) to craft a query in Azure DevOps.


    Typical way that queries are built:


    ```
     var targetQuery =
         string.Format(
             @"SELECT [System.Id], [{ReflectedWorkItemIDFieldName}] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {WIQLQueryBit} ORDER BY {WIQLOrderBit}",
             Engine.Target.Config.ReflectedWorkItemIDFieldName,
             _config.WIQLQueryBit,
             _config.WIQLOrderBit
          );
    var targetFoundItems = Engine.Target.WorkItems.GetWorkItems(targetQuery);

    ```


    A simple example config:


    ```

    "WIQLQueryBit": "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",

    "WIQLOrderBit": "[System.ChangedDate] desc",

    ```

    Scope to Area Path (Team data):


    ```

    "WIQLQueryBit": "AND [System.AreaPath] UNDER 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",

    "WIQLOrderBit": "[System.ChangedDate] desc",

    ```


    ```

    "WIQLQueryBit": "AND [System.ChangedDate] > 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",

    "WIQLOrderBit": "[System.ChangedDate] desc",

    ```


    ## <a name="NodeBasePath"></a>NodeBasePath Configuration 


    Moved to the ProcessorEnricher [TfsNodeStructure](/Reference/v2/ProcessorEnrichers/TfsNodeStructure/)


    # Iteration Maps and Area Maps


    Moved to the ProcessorEnricher [TfsNodeStructure](/Reference/v2/ProcessorEnrichers/TfsNodeStructure/)




    ## More Complex Team Migrations

    The above options allow you to bring over a sub-set of the WIs (using the `WIQLQueryBit`) and move their area or iteration path to a default location. However you may wish to do something more complex e.g. re-map the team structure. This can be done with addition of a `FieldMaps` block to configuration in addition to the `NodeBasePaths`.


    Using the above sample structure, if you wanted to map the source project `Team 1`  to target project `Team A` etc. you could add the field map as follows


    A complete list of [FieldMaps](../Reference/v1/FieldMaps/index.md) are available.


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


    > Note: This mappings could also be achieved with other forms of Field mapper e.g. `RegexFieldMapConfig`, but the value mapper as an example is easy to understand


    # Removed Properties


    - PrefixProjectToNodes - This option was removed in favour of the Area and Iteration Maps on [TfsNodeStructure](/Reference/v2/ProcessorEnrichers/TfsNodeStructure/)
- topic: introduction
  path: /docs/Reference/v1/Processors/WorkItemMigrationContext-introduction.md
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