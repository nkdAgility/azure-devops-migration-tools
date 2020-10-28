# Work item update

> Obsolete - merged into WorkItemMigration

This processor only updates work items in the target project. So you must first run a full migration so you can update this migration with this processor.

| Parameter name | Type    | Description                              | Default Value                            |
|----------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`      | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`   | string  | The name of the processor                | WorkItemUpdateConfig |
| `QueryBit`     | string  | A work item query to select only important work items. To migrate all leave this empty. |                                          |
| `WhatIf`     | Boolean  | Don't change anything only show what it will do if run this whiteout this parameter. |                                          |