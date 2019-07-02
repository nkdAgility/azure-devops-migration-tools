# Test configurations migration

This processor can migrate `test configuration`. This should be run before `VstsSyncMigrator.Engine.Configuration.Processing.LinkMigrationConfig`.

| Parameter name | Type    | Description                      | Default Value                            |
|----------------|---------|----------------------------------|------------------------------------------|
| `Enabled`      | Boolean | Active the processor if it true. | false                                    |
| `ObjectType`   | string  | The name of the processor        | VstsSyncMigrator.Engine.Configuration.Processing.TestConfigurationsMigrationConfig |

