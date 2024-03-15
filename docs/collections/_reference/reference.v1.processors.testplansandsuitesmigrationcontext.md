---
optionsClassName: TestPlansAndSuitesMigrationConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TestPlansAndSuitesMigrationConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TestPlansAndSuitesMigrationConfig",
      "Enabled": false,
      "OnlyElementsWithTag": null,
      "TestPlanQuery": null,
      "RemoveAllLinks": false,
      "MigrationDelay": 0,
      "RemoveInvalidTestSuiteLinks": false,
      "FilterCompleted": false
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TestPlansAndSuitesMigrationConfig
description: Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration
className: TestPlansAndSuitesMigrationContext
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
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
- parameterName: RemoveAllLinks
  type: Boolean
  description: ??Not sure what this does. Check code.
  defaultValue: false
- parameterName: RemoveInvalidTestSuiteLinks
  type: Boolean
  description: Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
  defaultValue: missng XML code comments
- parameterName: TestPlanQuery
  type: String
  description: Filtering conditions to decide whether to migrate a test plan or not. When provided, this partial query is added after `Select * From TestPlan Where` when selecting test plans. Among filtering options, `AreaPath`, `PlanName` and `PlanState` are known to work. There is unfortunately no documentation regarding the available fields.
  defaultValue: '`String.Empty`'
status: Beta
processingTarget: Suites & Plans
classFile: /src/VstsSyncMigrator.Core/Execution/MigrationContext/TestPlansAndSuitesMigrationContext.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/TestPlansAndSuitesMigrationConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/TestPlansAndSuitesMigrationContext/
title: TestPlansAndSuitesMigrationContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/TestPlansAndSuitesMigrationContext-notes.md
  exists: true
  markdown: >2-

    ## Additional Samples & Info


    To run a full plans and suits you should run the three processors in this order below.  `TestVariablesMigrationConfig` and `TestConfigurationsMigrationConfig` only need run once.


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
  path: /docs/Reference/v1/Processors/TestPlansAndSuitesMigrationContext-introduction.md
  exists: false
  markdown: ''

---