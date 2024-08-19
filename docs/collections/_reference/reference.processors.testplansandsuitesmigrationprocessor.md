---
optionsClassName: TestPlansAndSuitesMigrationProcessorOptions
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TestPlansAndSuitesMigrationProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorDefaults": {
          "TestPlansAndSuitesMigrationProcessor": {}
        }
      }
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TestPlansAndSuitesMigrationProcessorOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TestPlansAndSuitesMigrationProcessorOptions",
      "Enabled": false,
      "OnlyElementsWithTag": null,
      "TestPlanQuery": null,
      "RemoveAllLinks": false,
      "MigrationDelay": 0,
      "RemoveInvalidTestSuiteLinks": false,
      "FilterCompleted": false,
      "Enrichers": null,
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TestPlansAndSuitesMigrationProcessorOptions
description: Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration
className: TestPlansAndSuitesMigrationProcessor
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
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
- parameterName: ProcessorEnrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
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
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TestPlansAndSuitesMigrationProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TestPlansAndSuitesMigrationProcessorOptions.cs

redirectFrom:
- /Reference/v1/Processors/TestPlansAndSuitesMigrationProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TestPlansAndSuitesMigrationProcessor/
title: TestPlansAndSuitesMigrationProcessor
categories:
- Processors
- v1
topics:
- topic: notes
  path: /Processors/TestPlansAndSuitesMigrationProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/TestPlansAndSuitesMigrationProcessor-introduction.md
  exists: false
  markdown: ''

---