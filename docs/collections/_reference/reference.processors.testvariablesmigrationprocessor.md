---
optionsClassName: TestVariablesMigrationProcessorOptions
optionsClassFullName: MigrationTools.Processors.TestVariablesMigrationProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorDefaults": {
          "TestVariablesMigrationProcessor": {}
        }
      }
    }
  sampleFor: MigrationTools.Processors.TestVariablesMigrationProcessorOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TestVariablesMigrationProcessorOptions",
      "Enabled": false,
      "Enrichers": null,
      "Processor": "TestVariablesMigrationContext",
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.TestVariablesMigrationProcessorOptions
description: This processor can migrate test variables that are defined in the test plans / suites. This must run before `TestPlansAndSuitesMigrationConfig`.
className: TestVariablesMigrationProcessor
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
- parameterName: Processor
  type: String
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
status: Beta
processingTarget: Suites & Plans
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TestVariablesMigrationProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TestVariablesMigrationProcessorOptions.cs

redirectFrom:
- /Reference/v1/Processors/TestVariablesMigrationProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TestVariablesMigrationProcessor/
title: TestVariablesMigrationProcessor
categories:
- Processors
- v1
topics:
- topic: notes
  path: /Processors/TestVariablesMigrationProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/TestVariablesMigrationProcessor-introduction.md
  exists: false
  markdown: ''

---