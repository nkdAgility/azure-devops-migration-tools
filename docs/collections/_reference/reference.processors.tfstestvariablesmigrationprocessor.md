---
optionsClassName: TfsTestVariablesMigrationProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsTestVariablesMigrationProcessorOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.TfsTestVariablesMigrationProcessorOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.TfsTestVariablesMigrationProcessorOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsTestVariablesMigrationProcessorOptions",
      "Enabled": false,
      "Processor": "TestVariablesMigrationContext",
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.TfsTestVariablesMigrationProcessorOptions
description: This processor can migrate test variables that are defined in the test plans / suites. This must run before `TestPlansAndSuitesMigrationConfig`.
className: TfsTestVariablesMigrationProcessor
typeName: Processors
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Processor
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: SourceName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
status: Beta
processingTarget: Suites & Plans
classFile: /src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestVariablesMigrationProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestVariablesMigrationProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TfsTestVariablesMigrationProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TfsTestVariablesMigrationProcessor/
title: TfsTestVariablesMigrationProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TfsTestVariablesMigrationProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TfsTestVariablesMigrationProcessor-introduction.md
  exists: false
  markdown: ''

---