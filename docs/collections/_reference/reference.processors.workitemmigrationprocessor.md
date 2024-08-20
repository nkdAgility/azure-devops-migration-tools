---
optionsClassName: WorkItemMigrationProcessorOptions
optionsClassFullName: MigrationTools.Processors.WorkItemMigrationProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorDefaults": {
          "WorkItemMigrationProcessor": {
            "AttachRevisionHistory": "False",
            "Enabled": "False",
            "FilterWorkItemsThatAlreadyExistInTarget": "False",
            "FixHtmlAttachmentLinks": "True",
            "GenerateMigrationComment": "True",
            "MaxGracefulFailures": "0",
            "PauseAfterEachWorkItem": "False",
            "SkipRevisionWithInvalidAreaPath": "False",
            "SkipRevisionWithInvalidIterationPath": "False",
            "UpdateCreatedBy": "True",
            "UpdateCreatedDate": "True",
            "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
            "WorkItemCreateRetryLimit": "5",
            "WorkItemIDs": null
          }
        }
      }
    }
  sampleFor: MigrationTools.Processors.WorkItemMigrationProcessorOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "WorkItemMigrationProcessorOptions",
      "Enabled": false,
      "UpdateCreatedDate": true,
      "UpdateCreatedBy": true,
      "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
      "FixHtmlAttachmentLinks": true,
      "WorkItemCreateRetryLimit": 5,
      "FilterWorkItemsThatAlreadyExistInTarget": false,
      "PauseAfterEachWorkItem": false,
      "AttachRevisionHistory": false,
      "GenerateMigrationComment": true,
      "WorkItemIDs": null,
      "MaxGracefulFailures": 0,
      "SkipRevisionWithInvalidIterationPath": false,
      "SkipRevisionWithInvalidAreaPath": false,
      "Enrichers": null,
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.WorkItemMigrationProcessorOptions
description: WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments. Use `WorkItemMigrationConfig` to configure.
className: WorkItemMigrationProcessor
typeName: Processors
architecture: 
options:
- parameterName: AttachRevisionHistory
  type: Boolean
  description: This will create a json file with the revision history and attach it to the work item. Best used with `MaxRevisions` or `ReplayRevisions`.
  defaultValue: '?'
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
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
- parameterName: MaxGracefulFailures
  type: Int32
  description: The maximum number of failures to tolerate before the migration fails. When set above zero, a work item migration error is logged but the migration will continue until the number of failed items reaches the configured value, after which the migration fails.
  defaultValue: 0
- parameterName: PauseAfterEachWorkItem
  type: Boolean
  description: Pause after each work item is migrated
  defaultValue: false
- parameterName: ProcessorEnrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missng XML code comments
- parameterName: SkipRevisionWithInvalidAreaPath
  type: Boolean
  description: When set to true, this setting will skip a revision if the source area has not been migrated, has been deleted or is somehow invalid, etc.
  defaultValue: missng XML code comments
- parameterName: SkipRevisionWithInvalidIterationPath
  type: Boolean
  description: This will skip a revision if the source iteration has not been migrated i.e. it was deleted
  defaultValue: missng XML code comments
- parameterName: SourceName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TargetName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
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
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/WorkItemMigrationProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/WorkItemMigrationProcessorOptions.cs

redirectFrom:
- /Reference/Processors/WorkItemMigrationProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/WorkItemMigrationProcessor/
title: WorkItemMigrationProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /Processors/WorkItemMigrationProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/WorkItemMigrationProcessor-introduction.md
  exists: false
  markdown: ''

---