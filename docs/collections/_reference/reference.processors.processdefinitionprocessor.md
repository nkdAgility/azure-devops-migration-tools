---
optionsClassName: ProcessDefinitionProcessorOptions
optionsClassFullName: MigrationTools.Processors.ProcessDefinitionProcessorOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "ProcessDefinitionProcessorOptions",
      "Enabled": false,
      "Processes": {
        "$type": "Dictionary`2",
        "*": [
          "*"
        ]
      },
      "ProcessMaps": {
        "$type": "Dictionary`2"
      },
      "UpdateProcessDetails": true,
      "MaxDegreeOfParallelism": 1,
      "Enrichers": null,
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.ProcessDefinitionProcessorOptions
description: Process definition processor used to keep processes between two orgs in sync
className: ProcessDefinitionProcessor
typeName: Processors
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
- parameterName: MaxDegreeOfParallelism
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Processes
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ProcessMaps
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ProcessorEnrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
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
- parameterName: UpdateProcessDetails
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
status: Beta
processingTarget: Pipelines
classFile: /src/MigrationTools.Clients.AzureDevops.Rest/Processors/ProcessDefinitionProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.Rest/Processors/ProcessDefinitionProcessorOptions.cs

redirectFrom:
- /Reference/v2/Processors/ProcessDefinitionProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/ProcessDefinitionProcessor/
title: ProcessDefinitionProcessor
categories:
- Processors
- v2
topics:
- topic: notes
  path: /Processors/ProcessDefinitionProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/ProcessDefinitionProcessor-introduction.md
  exists: false
  markdown: ''

---