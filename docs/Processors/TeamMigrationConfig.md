# Team migration

This processor can migrate teams. (I currently don't know how much preview this thing is...)

This should be run after `NodeStructuresMigrationConfig` and before all other processors.


| Parameter name                | Type    | Description                                                                | Default Value                                                        |
|-------------------------------|---------|----------------------------------------------------------------------------|----------------------------------------------------------------------|
| `Enabled`                     | Boolean | Active the processor if it true.                                           | false                                                                |
| `ObjectType`                  | string  | The name of the processor                                                  | TeamMigrationConfig |
| `EnableTeamSettingsMigration` | Boolean | Migrate original team settings after their creation on target team project | false                                                                |
| `FixTeamSettingsForExistingTeams` | Boolean | Reset the target team settings to match the source if the team exists | false                                                                |
| `PrefixProjectToNodes`        | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false |