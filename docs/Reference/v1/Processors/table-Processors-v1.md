| Processors | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [CreateTeamFolders](CreateTeamFolders.md) | alpha | Shared Queries | Creates folders in Sared Queries for each Team |
| [ExportProfilePictureFromADContext](ExportProfilePictureFromADContext.md) | alpha | Profiles | Downloads corporate images and updates TFS/Azure DevOps profiles |
| [ExportTeamList](ExportTeamList.md) | missng XML code comments | missng XML code comments | missng XML code comments |
| [FakeProcessor](FakeProcessor.md) | missng XML code comments | missng XML code comments | Note: this is only for internal usage. Don't use this in your configurations. |
| [FixGitCommitLinks](FixGitCommitLinks.md) | missng XML code comments | missng XML code comments | missng XML code comments |
| [ImportProfilePictureContext](ImportProfilePictureContext.md) | alpha | Profiles | Downloads corporate images and updates TFS/Azure DevOps profiles |
| [TeamMigrationContext](TeamMigrationContext.md) | preview | Teams | Migrates Teams and Team Settings: This should be run after `NodeStructuresMigrationConfig` and before all other processors. |
| [TestConfigurationsMigrationContext](TestConfigurationsMigrationContext.md) | Beta | Suites & Plans | This processor can migrate `test configuration`. This should be run before `LinkMigrationConfig`. |
| [TestPlansAndSuitesMigrationContext](TestPlansAndSuitesMigrationContext.md) | Beta | Suites & Plans | Rebuilds Suits and plans for Test Cases migrated using the WorkItemMigration |
| [TestVariablesMigrationContext](TestVariablesMigrationContext.md) | Beta | Suites & Plans | This processor can migrate test variables that are defined in the test plans / suites. This must run before `TestPlansAndSuitesMigrationConfig`. |
| [WorkItemDelete](WorkItemDelete.md) | ready | WorkItem | The `WorkItemDelete` processor allows you to delete any amount of work items that meet the query. **DANGER:** This is not a recoverable action and should be use with extream caution. |
| [WorkItemMigrationContext](WorkItemMigrationContext.md) | ready | Work Items | WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments. Use `WorkItemMigrationConfig` to configure. |
| [WorkItemPostProcessingContext](WorkItemPostProcessingContext.md) | preview | Work Items | Reapply field mappings after a migration. Does not migtate Work Items, only reapplied changes to filed mappings. |
| [WorkItemQueryMigrationContext](WorkItemQueryMigrationContext.md) | preview | Shared Queries | This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool. |
| [WorkItemUpdate](WorkItemUpdate.md) | missng XML code comments | WorkItem | This processor allows you to make changes in place where we load from teh Target and update the Target. This is used for bulk updates with the most common reason being a process template change. |
| [WorkItemUpdateAreasAsTagsContext](WorkItemUpdateAreasAsTagsContext.md) | Beta | Work Item | A common issue with older *TFS/Azure DevOps* instances is the proliferation of `Area Paths`. With the use of `Area Path` for `Teams` and the addition of the `Node Name` column option these extensive tag hierarchies should instad be moved to tags. |
{: .table .table-striped .table-bordered .d-none .d-md-block}
