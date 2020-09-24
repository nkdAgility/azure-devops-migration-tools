# Link migration

This processor migrate links between work items. This must run after `WorkItemMigrationConfig`.

| Parameter name | Type    | Description                              | Default Value                            |
|----------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`      | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`   | string  | The name of the processor                | LinkMigrationConfig |
| `QueryBit`     | string  | A work item query to select only important work items. The query `AND ([System.ExternalLinkCount] > 0 OR [System.RelatedLinkCount] > 0)` is recommended. |                                          |
