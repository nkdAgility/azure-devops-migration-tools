# Fix Git commit links

> Obsolete - merged into WorkItemMigration

> Note: before this is working you must migrate you source code. See [here](./../index.md) (Code (Git))

This processor add the git links to the migrated work items. You must run the processor `WorkItemMigrationConfig` first do link the commits. Then the links will be updated.


| Parameter name     | Type    | Description                              | Default Value                            |
|--------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`          | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`       | string  | The name of the processor                | FixGitCommitLinksConfig |
| `TargetRepository` | string  | The new name of the repository. You must only set this if your repository name change in the migration process. |  The name of the source repository                                        |
| `QueryBit`       | string  | A query to select work items        |  |
| `OrderBit`       | string  |  A work item query to affect the order in which the work items are returned                |  |
