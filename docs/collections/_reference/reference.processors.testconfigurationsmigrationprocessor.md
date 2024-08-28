---
optionsClassName: TestConfigurationsMigrationProcessorOptions
optionsClassFullName: MigrationTools.Processors.TestConfigurationsMigrationProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Processors": []
      }
    }
  sampleFor: MigrationTools.Processors.TestConfigurationsMigrationProcessorOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Processors": []
      }
    }
  sampleFor: MigrationTools.Processors.TestConfigurationsMigrationProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TestConfigurationsMigrationProcessorOptions",
      "Enabled": false,
      "Enrichers": null,
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.TestConfigurationsMigrationProcessorOptions
description: This processor can migrate `test configuration`. This should be run before `LinkMigrationConfig`.
className: TestConfigurationsMigrationProcessor
typeName: Processors
architecture: 
options:
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
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TestConfigurationsMigrationProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TestConfigurationsMigrationProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TestConfigurationsMigrationProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TestConfigurationsMigrationProcessor/
title: TestConfigurationsMigrationProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TestConfigurationsMigrationProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TestConfigurationsMigrationProcessor-introduction.md
  exists: false
  markdown: ''

---