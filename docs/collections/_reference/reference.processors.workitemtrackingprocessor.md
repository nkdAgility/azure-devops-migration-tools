---
optionsClassName: WorkItemTrackingProcessorOptions
optionsClassFullName: MigrationTools.Processors.WorkItemTrackingProcessorOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.WorkItemTrackingProcessorOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.WorkItemTrackingProcessorOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "WorkItemTrackingProcessorOptions",
      "Enabled": false,
      "ReplayRevisions": false,
      "CollapseRevisions": false,
      "WorkItemCreateRetryLimit": 0,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.WorkItemTrackingProcessorOptions
description: This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md).
className: WorkItemTrackingProcessor
typeName: Processors
architecture: 
options:
- parameterName: CollapseRevisions
  type: Boolean
  description: Gets or sets a value indicating whether to collapse revisions into a single work item.
  defaultValue: missing XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: ReplayRevisions
  type: Boolean
  description: Gets or sets a value indicating whether to replay all revisions during migration.
  defaultValue: missing XML code comments
- parameterName: SourceName
  type: String
  description: This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor.
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a read only processor.
  defaultValue: missing XML code comments
- parameterName: WorkItemCreateRetryLimit
  type: Int32
  description: Gets or sets the number of times to retry work item creation if it fails.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools/Processors/WorkItemTrackingProcessorOptions.cs
optionsClassFile: src/MigrationTools/Processors/WorkItemTrackingProcessorOptions.cs

redirectFrom:
- /Reference/Processors/WorkItemTrackingProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/WorkItemTrackingProcessor/
title: WorkItemTrackingProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/WorkItemTrackingProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/WorkItemTrackingProcessor-introduction.md
  exists: false
  markdown: ''

---