---
optionsClassName: TfsWorkItemBulkEditProcessorOptions
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TfsWorkItemBulkEditProcessorOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TfsWorkItemBulkEditProcessorOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TfsWorkItemBulkEditProcessorOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsWorkItemBulkEditProcessorOptions",
      "Enabled": false,
      "WhatIf": false,
      "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [@ReflectedWorkItemIdField] = ''  AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
      "WorkItemIDs": null,
      "FilterWorkItemsThatAlreadyExistInTarget": false,
      "PauseAfterEachWorkItem": false,
      "WorkItemCreateRetryLimit": 0,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TfsWorkItemBulkEditProcessorOptions
description: This processor allows you to make changes in place where we load from the Target and update the Target. This is used for bulk updates with the most common reason being a process template change.
className: TfsWorkItemBulkEditProcessor
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
- parameterName: PauseAfterEachWorkItem
  type: Boolean
  description: Pause after each work item is migrated
  defaultValue: false
- parameterName: SourceName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: WhatIf
  type: Boolean
  description: Gets or sets a value indicating whether to run in "what if" mode without making actual changes to work items.
  defaultValue: false
- parameterName: WIQLQuery
  type: String
  description: A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits)
  defaultValue: AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request')
- parameterName: WorkItemCreateRetryLimit
  type: Int32
  description: '**beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count. This allows for periodic network glitches not to end the process.'
  defaultValue: 5
- parameterName: WorkItemIDs
  type: IList
  description: A list of work items to import
  defaultValue: '[]'
status: missing XML code comments
processingTarget: WorkItem
classFile: src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemBulkEditProcessor.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemBulkEditProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TfsWorkItemBulkEditProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TfsWorkItemBulkEditProcessor/
title: TfsWorkItemBulkEditProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TfsWorkItemBulkEditProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TfsWorkItemBulkEditProcessor-introduction.md
  exists: false
  markdown: ''

---