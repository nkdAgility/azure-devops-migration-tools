# Attachment export Migration

With this processor you can export work item attachments from the source project.

> Note: Make sure you have enough disk-space for all attachments on the migration machine.


| Parameter name | Type    | Description                              | Default Value                            |
|----------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`      | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`   | string  | The name of the processor                | VstsSyncMigrator.Engine.Configuration.Processing.AttachementExportMigrationConfig |
| `QueryBit`     | string  | A work item query to select only important work items with attachments. The query `AND [System.AttachedFileCount] > 0` is recommended. |                                          |

