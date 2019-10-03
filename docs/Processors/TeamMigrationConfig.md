# Team migration

This processor can migrate teams. (I currently don't know how much preview this thing is...)

This should be run after `VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig` and before all other processors.


| Parameter name                | Type    | Description                                                                | Default Value                                                        |
|-------------------------------|---------|----------------------------------------------------------------------------|----------------------------------------------------------------------|
| `Enabled`                     | Boolean | Active the processor if it true.                                           | false                                                                |
| `ObjectType`                  | string  | The name of the processor                                                  | VstsSyncMigrator.Engine.Configuration.Processing.TeamMigrationConfig |
| `EnableTeamSettingsMigration` | Boolean | Migrate original team settings after their creation on target team project | false                                                                |
