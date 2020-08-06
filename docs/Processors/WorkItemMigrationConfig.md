# Work item migration

That is the working horse. This processor migrate all the work items. That should be run after the following pre processors:

* `NodeStructuresMigrationConfig`
* `TeamMigrationConfig`
* `WorkItemQueryMigrationConfig`
 
It will migrate work items using a tip or replay migrator as well as Attachments, & Links.

## Features

- Migrate Work Items, Links, & Attachments
- Restart will skip completed work items in Tip, and will filter completed revisions in replay.

## Params

| Parameter name                       | Type    | Description                              | Default Value                            |
|--------------------------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`                            | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`                         | string  | The name of the processor                | VstsSyncMigrator. Engine. Configuration. Processing. WorkItemMigrationConfig |
| `PrefixProjectToNodes`               | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig` you must do it here too. | false                                    |
| `UpdateCreatedDate`                  | Boolean | If this is enabled the creation process on the target project will create the items with the original creation date. (Important: The item history is always pointed to the date of the migration, it's change only the data column `CreateDate`, not the internal create date) | false                                    |
| `UpdateCreatedBy`                    | Boolean | If this is enabled the creation process on the target project will create the items with the original creation date. (Important: The item history is always pointed to the date of the migration, it's change only the data column `CreateDate`, not the internal create date) | false                                    |
| _{obsolite}_ `UpdateSoureReflectedId`             | Boolean | Warning: if this enabled, that will change the work items from the source!<br><br>If this enabled, it will write the link to the work item in the target projekt to the same work item in the source project. | false                                    |
| `BuildFieldTable`                    | Boolean | Add in the original field to value table in a history comment. So if you lost information with the field mapping you are on the save side without data lost. This table is searchable using a `CONTAINS` WIQL query | false                                    |
| `AppendMigrationToolSignatureFooter` | Boolean | Add a signatur to the in the comment history of each work item. If you like this project please set this to true 😊 | false                                    |
| `ReplayRevisions` | Boolean | You can choose to migrate the tip only (a single write) or all of the revisions (many writes). If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true` | true |
| `LinkMigration` | Boolean | If enabled this will migrate the Links for the work item at the same time as the whole work item. | true |
| _{NEW}_ `LinkMigrationSaveEachAsAdded` | Boolean | If you have changed parents before re-running a sync you may get a `TF26194: unable to change the value of the 'Parent' field` error. This will resolve it, but will slow migration. | false                                    |
| `AttachmentMigration` | Boolean | If enabled this will migrate all of the attachements at the same time as the work item | true |
| `AttachmentWorkingPath` | String | `AttachmentMigration` is set to true then you need to specify a working path for attachemnts to be saved localy. | `C:\temp\Migration\` |
| `AttachmentMaxSize` | int | `AttachmentMigration` is set to true then you need to specify a max file size for upload in bites. For Azure DevOps Services the default is 480,000,000 bites (60mb), for TFS its 32,000,000 bites (4mb). | `480000000` |
| `FixHtmlAttachmentLinks` | Boolean | **beta** If enabled this will fix any image attachments URL's in the HTML fields. You must specify a PersonalAccessToken in the Source project for Azure DevOps; TFS should use integrated authentication.  |
| `WorkItemCreateRetryLimit` | Integer | *beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equil to the retry count. This allows for periodic network glitches not to end the process. | 5 |
| `FilterWorkItemsThatAlreadyExistInTarget` | Boolean | Instad of using the `UpdateSoureReflectedId` setting this load all of the work items already saved to the Target and removes them from the Source work item list prior to commensing the run. While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart. | true |
| `QueryBit`                           | string  | A work item query to select only important work items. To migrate all leave this empty. |                                          |
| `OrderBit` | string | A work item query to affect the order in which the work items are migrated. Don't leave this empty. | [System.ChangedDate] desc
| `SkipToFinalRevisedWorkItemType` | Boolean | If enabled, when a revision is found that changes the work item type it will use the most recent revision work item type when migrating the initial work item. This should only be enabled for migrations from Azure DevOps Service to Azure DevOps Server. | false
| `CollapseRevisions` | Boolean | If enabled, all revisions except the most recent are collapsed into a JSON format and attached as an attachment. Requires ReplayRevisions to be enabled. | false

