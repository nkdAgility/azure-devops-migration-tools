---
optionsClassName: WorkItemBulkEditProcessorConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.WorkItemBulkEditProcessorConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "WorkItemBulkEditProcessorConfig",
      "Enabled": false,
      "WhatIf": false,
      "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [@ReflectedWorkItemIdFieldName] = ''  AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
      "WorkItemIDs": null,
      "FilterWorkItemsThatAlreadyExistInTarget": false,
      "PauseAfterEachWorkItem": false,
      "WorkItemCreateRetryLimit": 0
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.WorkItemBulkEditProcessorConfig
description: This processor allows you to make changes in place where we load from teh Target and update the Target. This is used for bulk updates with the most common reason being a process template change.
className: WorkItemBulkEditProcessor
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: FilterWorkItemsThatAlreadyExistInTarget
  type: Boolean
  description: This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run. While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart.
  defaultValue: true
- parameterName: PauseAfterEachWorkItem
  type: Boolean
  description: Pause after each work item is migrated
  defaultValue: false
- parameterName: WhatIf
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
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
status: missng XML code comments
processingTarget: WorkItem
classFile: ''
optionsClassFile: ''

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/WorkItemBulkEditProcessor/
title: WorkItemBulkEditProcessor
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/WorkItemBulkEditProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/Processors/WorkItemBulkEditProcessor-introduction.md
  exists: false
  markdown: ''

---