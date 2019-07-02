# Node structures migration

This processor creates the required structure (Area(s) and iteration(s)) of the project. That should be the first migration task.


| Parameter name         | Type          | Description                              | Default Value                            |
|------------------------|---------------|------------------------------------------|------------------------------------------|
| `Enabled`              | Boolean       | Active the processor if it true.         | false                                    |
| `ObjectType`           | string        | The name of the processor                | VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig |
| `PrefixProjectToNodes` | Boolean       | Prefix your iterations and areas with the project name. | false                                    |
| `BasePaths`            | Array<string> | The root paths of the Ares / Iterations you want migrate. | ["/"]                                    |
