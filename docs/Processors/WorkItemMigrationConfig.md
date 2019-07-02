# Work item migration

> Note: for a clean history use `VstsSyncMigrator.Engine.Configuration.Processing.WorkItemRevisionReplayMigrationConfig`

That is the working horse. This processor migrate all the work items. That should be run after the following pre processors.

* `VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig`
* `VstsSyncMigrator.Engine.Configuration.Processing.TeamMigrationConfig`
 




| Parameter name                       | Type    | Description                              | Default Value                            |
|--------------------------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`                            | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`                         | string  | The name of the processor                | VstsSyncMigrator.Engine.Configuration.Processing.WorkItemMigrationConfig |
| `PrefixProjectToNodes`               | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig` you must do it here too. | false                                    |
| `UpdateCreatedDate`                  | Boolean | If this is enabled the creation process on the target project will create the items with the original creation date. (Important: The item history is always pointed to the date of the migration, it's change only the data column `CreateDate`, not the internal create date) | false                                    |
| `UpdateCreatedBy`                    | Boolean | If this is enabled the creation process on the target project will create the items with the original creation date. (Important: The item history is always pointed to the date of the migration, it's change only the data column `CreateDate`, not the internal create date) | false                                    |
| `UpdateSoureReflectedId`             | Boolean | Warning: if this enabled, that will change the work items from the source!<br><br>If this enabled, it will write the link to the work item in the target projekt to the same work item in the source project. | false                                    |
| `BuildFieldTable`                    | Boolean | Add in the original field to value table in a history comment. So if you lost information with the field mapping you are on the save side without data lost. | false                                    |
| `AppendMigrationToolSignatureFooter` | Boolean | Add a signatur to the in the comment history of each work item. If you like this project please set this to true 😊 | false                                    |
| `QueryBit`                           | string  | A work item query to select only important work items. To migrate all leave this empty. |                                          |
