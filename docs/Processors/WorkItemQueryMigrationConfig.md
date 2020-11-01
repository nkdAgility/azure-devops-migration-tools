# Work item query migration

This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool.

That processor should run after these processors:

* `NodeStructuresMigrationConfig` (Obsolete)
* `TeamMigrationConfig`
 


| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`              | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`           | string  | The name of the processor                | WorkItemQueryMigrationConfig |
| `SharedFolderName`     | string  | The folder where the shared queries are in. | Shared Queries                           |
| `PrefixProjectToNodes` | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false                                    |
| `SourceToTargetFieldMappings` | Dictionary`<string, string`>| Any field mappings | none |

