## Processors: TestPlansAndSuitesMigrationContext

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v1](../index.md) > [Processors](index.md)> **TestPlansAndSuitesMigrationContext**

Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration

> Warning: This migration can result in data lost because unsafe links are not able to migrate! (see description for parameter `RemoveInvalidTestSuiteLinks` or [this issue](https://github.com/nkdAgility/azure-devops-migration-tools/issues/178).)

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| AreaMaps | Dictionary`2 | See documentation for [NodeStructure](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md) | null |
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| FilterCompleted | Boolean | missng XML code comments | missng XML code comments |
| IterationMaps | Dictionary`2 | See documentation for [NodeStructure](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md) | null |
| MigrationDelay | Int32 | ??Not sure what this does. Check code. | 0 |
| NodeBasePaths | String[] | See documentation for [NodeStructure](/docs/Reference/v1/Processors/WorkItemMigrationConfig.md) | [] |
| OnlyElementsWithTag | String | The tag name that is present on all elements that must be migrated. If this option isn't present this processor will migrate all. | `String.Empty` |
| PrefixProjectToNodes | Boolean | Prefix the nodes with the new project name. | false |
| RemoveAllLinks | Boolean | ??Not sure what this does. Check code. | false |
| RemoveInvalidTestSuiteLinks | Boolean | Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178 | missng XML code comments |
| TestPlanQueryBit | String | Filtering conditions to decide whether to migrate a test plan or not. When provided, this partial query is added after `Select * From TestPlan Where` when selecting test plans. Among filtering options, `AreaPath`, `PlanName` and `PlanState` are known to work. There is unfortunately no documentation regarding the available fields. | `String.Empty` |
| UseCommonNodeStructureEnricherConfig | Boolean | Indicates whether the configuration for node structure transformation should be taken from the common enricher configs. Otherwise the configuration elements below are used | false |


### Example JSON

```JSON
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
```

### Additional Samples & Info

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