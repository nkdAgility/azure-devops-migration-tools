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
      "WIQLQueryBit": "AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
      "WIQLOrderBit": "[System.ChangedDate] desc",
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
- parameterName: FilterWorkItemsThatAlreadyExistInTarget
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: PauseAfterEachWorkItem
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WIQLOrderBit
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WIQLQueryBit
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/WorkItemDelete/
title: WorkItemDelete
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/WorkItemDelete-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/Processors/WorkItemDelete-introduction.md
  exists: false
  markdown: ''

---