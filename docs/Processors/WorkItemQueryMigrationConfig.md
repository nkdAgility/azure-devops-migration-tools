# Work item query migration

This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool.

That processor should run after these processors:

* `VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig`
* `VstsSyncMigrator.Engine.Configuration.Processing.TeamMigrationConfig`
 


| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`              | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`           | string  | The name of the processor                | VstsSyncMigrator.Engine.Configuration.Processing.WorkItemQueryMigrationConfig |
| `SharedFolderName`     | string  | The folder where the shared queries are in. | Shared Queries                           |
| `PrefixProjectToNodes` | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig` you must do it here too. | false                                    |


