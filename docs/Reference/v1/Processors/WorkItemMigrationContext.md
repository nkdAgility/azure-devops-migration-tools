## Processors: WorkItemMigrationContext

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../././index.md) > [Reference](.././index.md) > [API v1](../index.md) > [Processors](./index.md)> **WorkItemMigrationContext**

missng XML code comments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| ReplayRevisions | Boolean | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | missng XML code comments | missng XML code comments |
| UpdateCreatedDate | Boolean | missng XML code comments | missng XML code comments |
| UpdateCreatedBy | Boolean | missng XML code comments | missng XML code comments |
| WIQLQueryBit | String | missng XML code comments | missng XML code comments |
| WIQLOrderBit | String | missng XML code comments | missng XML code comments |
| LinkMigration | Boolean | missng XML code comments | missng XML code comments |
| AttachmentMigration | Boolean | missng XML code comments | missng XML code comments |
| AttachmentWorkingPath | String | missng XML code comments | missng XML code comments |
| FixHtmlAttachmentLinks | Boolean | missng XML code comments | missng XML code comments |
| SkipToFinalRevisedWorkItemType | Boolean | missng XML code comments | missng XML code comments |
| WorkItemCreateRetryLimit | Int32 | missng XML code comments | missng XML code comments |
| FilterWorkItemsThatAlreadyExistInTarget | Boolean | missng XML code comments | missng XML code comments |
| PauseAfterEachWorkItem | Boolean | missng XML code comments | missng XML code comments |
| AttachmentMaxSize | Int32 | missng XML code comments | missng XML code comments |
| AttachRevisionHistory | Boolean | missng XML code comments | missng XML code comments |
| LinkMigrationSaveEachAsAdded | Boolean | missng XML code comments | missng XML code comments |
| GenerateMigrationComment | Boolean | missng XML code comments | missng XML code comments |
| WorkItemIDs | IList | missng XML code comments | missng XML code comments |
| MaxRevisions | Int32 | missng XML code comments | missng XML code comments |
| NodeStructureEnricherEnabled | Nullable | missng XML code comments | missng XML code comments |
| UseCommonNodeStructureEnricherConfig | Boolean | missng XML code comments | missng XML code comments |
| StopMigrationOnMissingAreaIterationNodes | Boolean | missng XML code comments | missng XML code comments |
| NodeBasePaths | String[] | missng XML code comments | missng XML code comments |
| AreaMaps | Dictionary`2 | missng XML code comments | missng XML code comments |
| IterationMaps | Dictionary`2 | missng XML code comments | missng XML code comments |
| MaxGracefulFailures | Int32 | missng XML code comments | missng XML code comments |
| SkipRevisionWithInvalidIterationPath | Boolean | This will skip a revision if the source iteration has not been migrated i.e. it was deleted | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "WorkItemMigrationConfig",
  "Enabled": false,
  "ReplayRevisions": true,
  "PrefixProjectToNodes": false,
  "UpdateCreatedDate": true,
  "UpdateCreatedBy": true,
  "WIQLQueryBit": "AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request')",
  "WIQLOrderBit": "[System.ChangedDate] desc",
  "LinkMigration": true,
  "AttachmentMigration": true,
  "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
  "FixHtmlAttachmentLinks": false,
  "SkipToFinalRevisedWorkItemType": true,
  "WorkItemCreateRetryLimit": 5,
  "FilterWorkItemsThatAlreadyExistInTarget": true,
  "PauseAfterEachWorkItem": false,
  "AttachmentMaxSize": 480000000,
  "AttachRevisionHistory": false,
  "LinkMigrationSaveEachAsAdded": false,
  "GenerateMigrationComment": true,
  "WorkItemIDs": null,
  "MaxRevisions": 0,
  "NodeStructureEnricherEnabled": null,
  "UseCommonNodeStructureEnricherConfig": false,
  "StopMigrationOnMissingAreaIterationNodes": true,
  "NodeBasePaths": null,
  "AreaMaps": {
    "$type": "Dictionary`2"
  },
  "IterationMaps": {
    "$type": "Dictionary`2"
  },
  "MaxGracefulFailures": 0,
  "SkipRevisionWithInvalidIterationPath": false
}
```