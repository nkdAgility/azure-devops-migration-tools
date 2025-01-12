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
