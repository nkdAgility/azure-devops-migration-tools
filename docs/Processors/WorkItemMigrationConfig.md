# Work item migration

That is the working horse. This processor migrate all the work items. That should be run after the following pre processors:

* `VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig`
* `VstsSyncMigrator.Engine.Configuration.Processing.TeamMigrationConfig`
 
It will migrate work items using a tip or replay migrator as well as Attachments, & Links.

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
| _{NEW}_ `ReplayRevisions` | Boolean | You can choose to migrate the tip only (a single write) or all of the revisions (many writes). If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true` | true
|
| _{NEW}_ `LinkMigration` | Boolean | If enabled this will migrate the Links for the work item at the same time as the whole work item. | true |
| _{NEW}_ `AttachmentMigration` | Boolean | If enabled this will migrate all of the attachements at the same time as the work item | true |
| _{NEW}_ `AttachmentWorkingPath` | String | `AttachmentMigration` is set to true then you need to specify a working path for attachemnts to be saved localy. | `C:\temp\Migration\` |
| _{NEW}_ `FixHtmlAttachmentLinks` | Boolean | **beta** If enabled this will fix any image attachments URL's in the HTML fields. |
| _{NEW}_ `WorkItemCreateRetryLimit` | Integer | *beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equil to the retry count. This allows for periodic network glitches not to end the process. | 5 |
| _{NEW}_ `FilterWorkItemsThatAlreadyExistInTarget` | Boolean | Instad of using the `UpdateSoureReflectedId` setting this load all of the work items already saved to the Target and removes them from the Source work item list prior to commensing the run. While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart. | true |
| `QueryBit`                           | string  | A work item query to select only important work items. To migrate all leave this empty. |                                          |


