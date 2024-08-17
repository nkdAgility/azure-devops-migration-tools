---
optionsClassName: WorkItemDeleteConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.WorkItemDeleteConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "WorkItemDeleteConfig",
      "Enabled": false,
      "Enrichers": null,
      "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
      "WorkItemIDs": null,
      "FilterWorkItemsThatAlreadyExistInTarget": false,
      "PauseAfterEachWorkItem": false,
      "WorkItemCreateRetryLimit": 0
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.WorkItemDeleteConfig
description: The `WorkItemDelete` processor allows you to delete any amount of work items that meet the query. **DANGER:** This is not a recoverable action and should be use with extream caution.
className: WorkItemDelete
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
- parameterName: FilterWorkItemsThatAlreadyExistInTarget
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: PauseAfterEachWorkItem
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WIQLQuery
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemCreateRetryLimit
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemIDs
  type: IList
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: WorkItem
classFile: /src/VstsSyncMigrator.Core/Execution/ProcessingContext/WorkItemDelete.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/WorkItemDeleteConfig.cs

redirectFrom:
- /Reference/v1/Processors/WorkItemDeleteConfig/
layout: reference
toc: true
permalink: /Reference/Processors/WorkItemDelete/
title: WorkItemDelete
categories:
- Processors
- v1
topics:
- topic: notes
  path: /Processors/WorkItemDelete-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/WorkItemDelete-introduction.md
  exists: false
  markdown: ''

---