## Processors

[Overview](.././index.md) > [Reference](../index.md) > *Processors_v1*

We provide a number of Processors that can be used to migrate diferent sorts of data. These processors are the original traditional processors.

| Processors | Data Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| [FakeProcessor](/FakeProcessor.md) |  |  |  |
| [TeamMigrationContext](/TeamMigrationContext.md) |  |  |  |
| [TestConfigurationsMigrationContext](/TestConfigurationsMigrationContext.md) |  |  |  |
| [TestPlandsAndSuitesMigrationContext](/TestPlandsAndSuitesMigrationContext.md) |  |  |  |
| [TestVariablesMigrationContext](/TestVariablesMigrationContext.md) |  |  |  |
| [WorkItemMigrationContext](/WorkItemMigrationContext.md) |  |  |  |
| [WorkItemPostProcessingContext](/WorkItemPostProcessingContext.md) |  |  |  |
| [WorkItemQueryMigrationContext](/WorkItemQueryMigrationContext.md) |  |  |  |
| [CreateTeamFolders](/CreateTeamFolders.md) |  |  |  |
| [ExportProfilePictureFromADContext](/ExportProfilePictureFromADContext.md) |  |  |  |
| [ExportTeamList](/ExportTeamList.md) |  |  |  |
| [FixGitCommitLinks](/FixGitCommitLinks.md) |  |  |  |
| [ImportProfilePictureContext](/ImportProfilePictureContext.md) |  |  |  |
| [WorkItemDelete](/WorkItemDelete.md) |  |  |  |
| [WorkItemUpdate](/WorkItemUpdate.md) |  |  |  |
| [WorkItemUpdateAreasAsTagsContext](/WorkItemUpdateAreasAsTagsContext.md) |  |  |  |


### Processor Options

 All processors have a minimum set of options that are required to run. 

#### Minimum Options to run
The `Enabled` options is common to all processors.


```JSON
     {
      "$type": "ProcessorConfig",
      "Enabled": true,
    }
```