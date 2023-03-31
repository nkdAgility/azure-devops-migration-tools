## <TypeName>: <ClassName>

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

<Breadcrumbs>

<Description>

> Warning: This migration can result in data lost because unsafe links are not able to migrate! (see description for parameter `RemoveInvalidTestSuiteLinks` or [this issue](https://github.com/nkdAgility/azure-devops-migration-tools/issues/178).)

### Options

<Options>

### Example JSON

```JSON
<ExampleJson>
```

### Additional Samples & Info

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