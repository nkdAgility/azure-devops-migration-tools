---
optionsClassName: TfsTestPlansAndSuitesMigrationProcessorOptions
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TfsTestPlansAndSuitesMigrationProcessorOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TfsTestPlansAndSuitesMigrationProcessorOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TfsTestPlansAndSuitesMigrationProcessorOptions
- name: classic
  order: 3
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
      "TestPlanIds": [],
      "SourceName": null,
      "TargetName": null
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
  defaultValue: missing XML code comments
- parameterName: FilterCompleted
  type: Boolean
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: MigrationDelay
  type: Int32
  description: ??Not sure what this does. Check code.
  defaultValue: 0
- parameterName: OnlyElementsWithTag
  type: String
  description: The tag name that is present on all elements that must be migrated. If this option isn't present this processor will migrate all.
  defaultValue: '`String.Empty`'
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
  description: This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor.
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a read only processor.
  defaultValue: missing XML code comments
- parameterName: TestPlanIds
  type: Int32[]
  description: 'This flag filters all test plans and retains only the specified ones for migration. Pass the test plan IDs as an array. Example: "TestPlanIds": [123, 456, 789]   Works optimally when "TestPlanQuery" is set to null.'
  defaultValue: missing XML code comments
- parameterName: TestPlanQuery
  type: String
  description: Filtering conditions to decide whether to migrate a test plan or not. When provided, this partial query is added after `Select * From TestPlan Where` when selecting test plans. Among filtering options, `AreaPath`, `PlanName` and `PlanState` are known to work. There is unfortunately no documentation regarding the available fields.
  defaultValue: '`String.Empty`'
status: Beta
processingTarget: Suites & Plans
classFile: src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestPlansAndSuitesMigrationProcessor.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Processors/TfsTestPlansAndSuitesMigrationProcessorOptions.cs
notes:
  exists: true
  path: docs/Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor-notes.md
  markdown: >
    ## Additional Samples & Info


    To run a full plans and suites you should run the three processors in this order below. `TestVariablesMigrationConfig` and `TestConfigurationsMigrationConfig` only need run once.


    ```json

    "Processors": [
        {
          "$type": "TestVariablesMigrationConfig",
          "Enabled": false
        },
        {
          "$type": "TestConfigurationsMigrationConfig",
          "Enabled": true
        },
        {
          "$type": "TestPlansAndSuitesMigrationConfig",
          "Enabled": true,
          "PrefixProjectToNodes": false,
          "OnlyElementsWithTag": null,
          "TestPlanQueryBit": null,
          "RemoveAllLinks": false,
          "MigrationDelay": 0,
          "UseCommonNodeStructureEnricherConfig": false,
          "NodeBasePaths": [],
          "AreaMaps": null,
          "IterationMaps": null,
          "RemoveInvalidTestSuiteLinks": false,
          "FilterCompleted": false
        }
    ]

    ```


    ## Known working TestPlanQueryBit filter fields names


    `AreaPath`, `PlanName` and `PlanState`


    ```json

    "TestPlanQueryBit": "PlanName = 'ABC'"

    ```

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
  path: docs/Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor-notes.md
  exists: true
  markdown: >
    ## Additional Samples & Info


    To run a full plans and suites you should run the three processors in this order below. `TestVariablesMigrationConfig` and `TestConfigurationsMigrationConfig` only need run once.


    ```json

    "Processors": [
        {
          "$type": "TestVariablesMigrationConfig",
          "Enabled": false
        },
        {
          "$type": "TestConfigurationsMigrationConfig",
          "Enabled": true
        },
        {
          "$type": "TestPlansAndSuitesMigrationConfig",
          "Enabled": true,
          "PrefixProjectToNodes": false,
          "OnlyElementsWithTag": null,
          "TestPlanQueryBit": null,
          "RemoveAllLinks": false,
          "MigrationDelay": 0,
          "UseCommonNodeStructureEnricherConfig": false,
          "NodeBasePaths": [],
          "AreaMaps": null,
          "IterationMaps": null,
          "RemoveInvalidTestSuiteLinks": false,
          "FilterCompleted": false
        }
    ]

    ```


    ## Known working TestPlanQueryBit filter fields names


    `AreaPath`, `PlanName` and `PlanState`


    ```json

    "TestPlanQueryBit": "PlanName = 'ABC'"

    ```
- topic: introduction
  path: docs/Reference/Processors/TfsTestPlansAndSuitesMigrationProcessor-introduction.md
  exists: false
  markdown: ''

---