# Fix Git commit links

> Note: before this is working you must migrate you source code. See [here](./../index.md) (Code (Git))

This processor add the git links to the migrated work items. You must run the processor `VstsSyncMigrator.Engine.Configuration.Processing.WorkItemMigrationConfig` first do link the commits. Then the links will be updated.


| Parameter name     | Type    | Description                              | Default Value                            |
|--------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`          | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`       | string  | The name of the processor                | VstsSyncMigrator.Engine.Configuration.Processing.FixGitCommitLinksConfig |
| `TargetRepository` | string  | The new name of the repository. You must only set this if your repository name change in the migration process. |  The name of the source repository                                        |
