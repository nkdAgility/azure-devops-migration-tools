---
optionsClassName: TestVariablesMigrationProcessorOptions
optionsClassFullName: MigrationTools.Processors.TestVariablesMigrationProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: Default Unavailable
  sampleFor: MigrationTools.Processors.TestVariablesMigrationProcessorOptions
- name: sample
  description: 
  code: Sample Unavailable
  sampleFor: MigrationTools.Processors.TestVariablesMigrationProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TestVariablesMigrationProcessorOptions",
      "Enabled": false,
      "Processor": "TestVariablesMigrationContext",
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.TestVariablesMigrationProcessorOptions
description: This processor can migrate test variables that are defined in the test plans / suites. This must run before `TestPlansAndSuitesMigrationConfig`.
className: TestVariablesMigrationProcessor
typeName: Processors
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: Processor
  type: String
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
status: Beta
processingTarget: Suites & Plans
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TestVariablesMigrationProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TestVariablesMigrationProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TestVariablesMigrationProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TestVariablesMigrationProcessor/
title: TestVariablesMigrationProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TestVariablesMigrationProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TestVariablesMigrationProcessor-introduction.md
  exists: false
  markdown: ''

---