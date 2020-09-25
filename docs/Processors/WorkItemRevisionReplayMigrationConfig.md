# Work item revision replay migration

That is the working horse. This processor migrate all the work items. 
The process is different from `WorkItemMigrationConfig`. This process will create a work item step by step. So you get a clean history in the target project. 

> Note: only use this processor if you need a correct history on your work items. This processor is much slower then `WorkItemMigrationConfig` and don't have so much option to change the work item meta data. (CreatedDate, Createdby, BuildFieldTable, AppendMigrationToolSignatureFooter)

That should be run after the following pre processors.

* `NodeStructuresMigrationConfig`
* `TeamMigrationConfig`
 





| Parameter name           | Type    | Description                              | Default Value                            |
|--------------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`                | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`             | string  | The name of the processor                | WorkItemRevisionReplayMigrationConfig |
| `PrefixProjectToNodes`   | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false                                    |
| `UpdateSoureReflectedId` | Boolean | Warning: if this enabled, that will change the work items from the source!<br><br>If this enabled, it will write the link to the work item in the target projekt to the same work item in the source project. | false                                    |
| `QueryBit`               | string  | A work item query to select only important work items. To migrate all leave this empty. |                                          |
