---
optionsClassName: TfsTestVariablesMigrationProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsTestVariablesMigrationProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.TfsTestVariablesMigrationProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.TfsTestVariablesMigrationProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsTestVariablesMigrationProcessorOptions",
      "Enabled": false,
      "Processor": "TestVariablesMigrationContext",
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
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
- parameterName: Enrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missing XML code comments
- parameterName: Processor
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
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