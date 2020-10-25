# Attachment Import Migration

>  Obsolete - merged into WorkItemMigration

> Note: You can only import attachment if you also run the processor `AttachementExportMigrationConfig` before.

With this processor you can import work item attachments that was exported with the `AttachementExportMigrationConfig` processor from the source project. This only works if the `ReflectedWorkItemIDFieldName` is set to the source project work item link.


| Parameter name | Type    | Description                       | Default Value                            |
|----------------|---------|-----------------------------------|------------------------------------------|
| `Enabled`        | Boolean | Active the processor if it true.  | false                                    |
| `ObjectType`     | string  | The name of the processor         | AttachementImportMigrationConfig |

