## Processors

[Overview](.././index.md) > [Reference](../index.md) > *Processors_v1*

We provide a number of Processors that can be used to migrate diferent sorts of data. These processors are the original traditional processors.

| Processors | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [CreateTeamFolders](/docs/Reference/v1/Processors/CreateTeamFolders.md) | alpha | Shared Queries | Creates folders in Sared Queries for each Team |
| [ExportProfilePictureFromADContext](/docs/Reference/v1/Processors/ExportProfilePictureFromADContext.md) | alpha | Profiles | Downloads corporate images and updates TFS/Azure DevOps profiles |
| [ExportTeamList](/docs/Reference/v1/Processors/ExportTeamList.md) | missng XML code comments | missng XML code comments | missng XML code comments |
| [FakeProcessor](/docs/Reference/v1/Processors/FakeProcessor.md) | missng XML code comments | missng XML code comments | Note: this is only for internal usage. Don't use this in your configurations. |
| [FixGitCommitLinks](/docs/Reference/v1/Processors/FixGitCommitLinks.md) | missng XML code comments | missng XML code comments | missng XML code comments |
| [ImportProfilePictureContext](/docs/Reference/v1/Processors/ImportProfilePictureContext.md) | alpha | Profiles | Downloads corporate images and updates TFS/Azure DevOps profiles |
| [TeamMigrationContext](/docs/Reference/v1/Processors/TeamMigrationContext.md) | preview | Teams | Migrates Teams and Team Settings: This should be run after `NodeStructuresMigrationConfig` and before all other processors. |
| [TestConfigurationsMigrationContext](/docs/Reference/v1/Processors/TestConfigurationsMigrationContext.md) | Beta | Suites & Plans | This processor can migrate `test configuration`. This should be run before `LinkMigrationConfig`. |
| [TestPlandsAndSuitesMigrationContext](/docs/Reference/v1/Processors/TestPlandsAndSuitesMigrationContext.md) | Beta | Suites & Plans | Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration |
| [TestVariablesMigrationContext](/docs/Reference/v1/Processors/TestVariablesMigrationContext.md) | Beta | Suites & Plans | This processor can migrate test variables that are defined in the test plans / suites. This must run before ` TestPlansAndSuitesMigrationConfig` |
| [WorkItemDelete](/docs/Reference/v1/Processors/WorkItemDelete.md) | ready | WorkItem | The `WorkItemDelete` processor allows you to delete any amount of work items that meet the query. **DANGER:** This is not a recoverable action and should be use with extream caution. |
| [WorkItemMigrationContext](/docs/Reference/v1/Processors/WorkItemMigrationContext.md) | ready | Work Items | WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments. Use `WorkItemMigrationConfig` to configure. |
| [WorkItemPostProcessingContext](/docs/Reference/v1/Processors/WorkItemPostProcessingContext.md) | preview | Work Items | Reapply field mappings after a migration. Does not migtate Work Items, only reapplied changes to filed mappings. |
| [WorkItemQueryMigrationContext](/docs/Reference/v1/Processors/WorkItemQueryMigrationContext.md) | preview | Shared Queries | This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool. |
| [WorkItemUpdate](/docs/Reference/v1/Processors/WorkItemUpdate.md) | missng XML code comments | WorkItem | This processor allows you to make changes in place where we load from teh Target and update the Target. This is used for bulk updates with the most common reason being a process template change. |
| [WorkItemUpdateAreasAsTagsContext](/docs/Reference/v1/Processors/WorkItemUpdateAreasAsTagsContext.md) | Beta | WorkItem | A common issue with older *TFS/Azure DevOps* instances is the proliferation of `Area Paths`. \With the use of `Area Path` for `Teams` and the addition of the `Node Name` column option these extensive tag hierarchies should instad be moved to tags. |


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