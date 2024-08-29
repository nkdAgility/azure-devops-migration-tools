---
optionsClassName: WorkItemDeleteProcessorOptions
optionsClassFullName: MigrationTools.Processors.WorkItemDeleteProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.WorkItemDeleteProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.WorkItemDeleteProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "WorkItemDeleteProcessorOptions",
      "Enabled": false,
      "WIQLQuery": "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc",
      "WorkItemIDs": null,
      "FilterWorkItemsThatAlreadyExistInTarget": false,
      "PauseAfterEachWorkItem": false,
      "WorkItemCreateRetryLimit": 0,
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.WorkItemDeleteProcessorOptions
description: The `WorkItemDelete` processor allows you to delete any amount of work items that meet the query. **DANGER:** This is not a recoverable action and should be use with extream caution.
className: WorkItemDeleteProcessor
typeName: Processors
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: FilterWorkItemsThatAlreadyExistInTarget
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: PauseAfterEachWorkItem
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missng XML code comments
- parameterName: SourceName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TargetName
  type: String
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
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/WorkItemDeleteProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/WorkItemDeleteProcessorOptions.cs

redirectFrom:
- /Reference/Processors/WorkItemDeleteProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/WorkItemDeleteProcessor/
title: WorkItemDeleteProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/WorkItemDeleteProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/WorkItemDeleteProcessor-introduction.md
  exists: false
  markdown: ''

---