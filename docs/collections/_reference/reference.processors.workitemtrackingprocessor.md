---
optionsClassName: WorkItemTrackingProcessorOptions
optionsClassFullName: MigrationTools.Processors.WorkItemTrackingProcessorOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "WorkItemTrackingProcessorOptions",
      "Enabled": true,
      "ReplayRevisions": true,
      "CollapseRevisions": false,
      "WorkItemCreateRetryLimit": 5,
      "Enrichers": null,
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.WorkItemTrackingProcessorOptions
description: This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md).
className: WorkItemTrackingProcessor
typeName: Processors
architecture: v2
options:
- parameterName: CollapseRevisions
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
- parameterName: ProcessorEnrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missng XML code comments
- parameterName: ReplayRevisions
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: SourceName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TargetName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemCreateRetryLimit
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/Processors/WorkItemProcessor/WorkItemTrackingProcessor.cs
optionsClassFile: /src/MigrationTools/Processors/WorkItemProcessor/WorkItemTrackingProcessorOptions.cs

redirectFrom:
- /Reference/v2/Processors/WorkItemTrackingProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/WorkItemTrackingProcessor/
title: WorkItemTrackingProcessor
categories:
- Processors
- v2
topics:
- topic: notes
  path: /Processors/WorkItemTrackingProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/WorkItemTrackingProcessor-introduction.md
  exists: false
  markdown: ''

---