# Work item query migration

This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool.

That processor should run after these processors:

* `NodeStructuresMigrationConfig`
* `TeamMigrationConfig`
 


| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`              | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`           | string  | The name of the processor                | WorkItemQueryMigrationConfig |
| `SharedFolderName`     | string  | The folder where the shaerd queries are in. | Shared Queries                           |
| `PrefixProjectToNodes` | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false                                    |


