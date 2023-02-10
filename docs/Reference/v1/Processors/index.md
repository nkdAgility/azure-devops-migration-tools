## Processors

[Overview](.././index.md) > [Reference](../index.md) > *Processors_v1*

We provide a number of Processors that can be used to migrate diferent sorts of data. These processors are the original traditional processors.

| Processors | Data Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| [FakeProcessor](./FakeProcessor.md) | missng XML code comments | missng XML code comments |  |
| [TeamMigrationContext](./TeamMigrationContext.md) | missng XML code comments | missng XML code comments |  |
| [TestConfigurationsMigrationContext](./TestConfigurationsMigrationContext.md) | missng XML code comments | missng XML code comments |  |
| [TestPlandsAndSuitesMigrationContext](./TestPlandsAndSuitesMigrationContext.md) | missng XML code comments | missng XML code comments |  |
| [TestVariablesMigrationContext](./TestVariablesMigrationContext.md) | missng XML code comments | missng XML code comments |  |
| [WorkItemMigrationContext](./WorkItemMigrationContext.md) | WorkItem | WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments. Use `WorkItemMigrationConfig` to configure. |  |
| [WorkItemPostProcessingContext](./WorkItemPostProcessingContext.md) | missng XML code comments | missng XML code comments |  |
| [WorkItemQueryMigrationContext](./WorkItemQueryMigrationContext.md) | missng XML code comments | missng XML code comments |  |
| [CreateTeamFolders](./CreateTeamFolders.md) | missng XML code comments | missng XML code comments |  |
| [ExportProfilePictureFromADContext](./ExportProfilePictureFromADContext.md) | missng XML code comments | missng XML code comments |  |
| [ExportTeamList](./ExportTeamList.md) | missng XML code comments | missng XML code comments |  |
| [FixGitCommitLinks](./FixGitCommitLinks.md) | missng XML code comments | missng XML code comments |  |
| [ImportProfilePictureContext](./ImportProfilePictureContext.md) | missng XML code comments | missng XML code comments |  |
| [WorkItemDelete](./WorkItemDelete.md) | WorkItem | The `WorkItemDelete` processor allows you to delete any amount of work items that meet the query. **DANGER:** This is not a recoverable action and should be use with extream caution. |  |
| [WorkItemUpdate](./WorkItemUpdate.md) | WorkItem | This processor allows you to make changes in place where we load from teh Target and update the Target. This is used for bulk updates with the most common reason being a process template change. |  |
| [WorkItemUpdateAreasAsTagsContext](./WorkItemUpdateAreasAsTagsContext.md) | WorkItem | A common issue with older *TFS/Azure DevOps* instances is the proliferation of `Area Paths`. \With the use of `Area Path` for `Teams` and the addition of the `Node Name` column option these extensive tag hierarchies should instad be moved to tags. |  |


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