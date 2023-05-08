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
      "PrefixProjectToNodes": false,
      "OnlyElementsWithTag": null,
      "TestPlanQueryBit": null,
      "RemoveAllLinks": false,
      "MigrationDelay": 0,
      "UseCommonNodeStructureEnricherConfig": false,
      "NodeBasePaths": null,
      "AreaMaps": null,
      "IterationMaps": null,
      "RemoveInvalidTestSuiteLinks": false,
      "FilterCompleted": false
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TestPlansAndSuitesMigrationConfig
description: Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration
className: TestPlansAndSuitesMigrationContext
typeName: Processors
architecture: v1
options:
- parameterName: AreaMaps
  type: Dictionary
  description: See documentation for [NodeStructure](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md)
  defaultValue: null
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: FilterCompleted
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: IterationMaps
  type: Dictionary
  description: See documentation for [NodeStructure](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md)
  defaultValue: null
- parameterName: MigrationDelay
  type: Int32
  description: ??Not sure what this does. Check code.
  defaultValue: 0
- parameterName: NodeBasePaths
  type: String[]
  description: See documentation for [NodeStructure](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md)
  defaultValue: '[]'
- parameterName: OnlyElementsWithTag
  type: String
  description: The tag name that is present on all elements that must be migrated. If this option isn't present this processor will migrate all.
  defaultValue: '`String.Empty`'
- parameterName: PrefixProjectToNodes
  type: Boolean
  description: Prefix the nodes with the new project name.
  defaultValue: false
- parameterName: RemoveAllLinks
  type: Boolean
  description: ??Not sure what this does. Check code.
  defaultValue: false
- parameterName: RemoveInvalidTestSuiteLinks
  type: Boolean
  description: Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
  defaultValue: missng XML code comments
- parameterName: TestPlanQueryBit
  type: String
  description: Filtering conditions to decide whether to migrate a test plan or not. When provided, this partial query is added after `Select * From TestPlan Where` when selecting test plans. Among filtering options, `AreaPath`, `PlanName` and `PlanState` are known to work. There is unfortunately no documentation regarding the available fields.
  defaultValue: '`String.Empty`'
- parameterName: UseCommonNodeStructureEnricherConfig
  type: Boolean
  description: Indicates whether the configuration for node structure transformation should be taken from the common enricher configs. Otherwise the configuration elements below are used
  defaultValue: false
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