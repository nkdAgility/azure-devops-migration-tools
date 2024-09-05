---
optionsClassName: TfsTestPlansAndSuitesMigrationProcessorOptions
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TfsTestPlansAndSuitesMigrationProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TfsTestPlansAndSuitesMigrationProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TfsTestPlansAndSuitesMigrationProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsTestPlansAndSuitesMigrationProcessorOptions",
      "Enabled": false,
      "OnlyElementsWithTag": null,
      "TestPlanQuery": null,
      "RemoveAllLinks": false,
      "MigrationDelay": 0,
      "RemoveInvalidTestSuiteLinks": false,
      "FilterCompleted": false,
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TfsTestPlansAndSuitesMigrationProcessorOptions
description: Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration
className: TfsTestPlansAndSuitesMigrationProcessor
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
- parameterName: FilterCompleted
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: MigrationDelay
  type: Int32
  description: ??Not sure what this does. Check code.
  defaultValue: 0
- parameterName: OnlyElementsWithTag
  type: String
  description: The tag name that is present on all elements that must be migrated. If this option isn't present this processor will migrate all.
  defaultValue: '`String.Empty`'
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missng XML code comments
- parameterName: RemoveAllLinks
  type: Boolean
  description: ??Not sure what this does. Check code.
  defaultValue: false
- parameterName: RemoveInvalidTestSuiteLinks
  type: Boolean
  description: Indicates whether the configuration for node structure transformation should be taken from the common enricher configs. Otherwise the configuration elements below are used
  defaultValue: false
- parameterName: SourceName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TargetName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TestPlanQuery
  type: String
  description: Filtering conditions to decide whether to migrate a test plan or not. When provided, this partial query is added after `Select * From TestPlan Where` when selecting test plans. Among filtering options, `AreaPath`, `PlanName` and `PlanState` are known to work. There is unfortunately no documentation regarding the available fields.
  defaultValue: '`String.Empty`'
status: Beta
processingTarget: Suites & Plans
classFile: /src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestPlansAndSuitesMigrationProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestPlansAndSuitesMigrationProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TfsTestPlansAndSuitesMigrationProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor/
title: TfsTestPlansAndSuitesMigrationProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor-introduction.md
  exists: false
  markdown: ''

---