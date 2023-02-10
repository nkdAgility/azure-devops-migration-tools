## Processors: TeamMigrationContext

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](/docs/index.md) > [Reference](/docs/Reference/index.md) > [API v1](/docs/Reference/v1/index.md) > [Processors](/docs/Reference/v1/Processors/index.md)> **TeamMigrationContext**

Migrates Teams and Team Settings

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| EnableTeamSettingsMigration | Boolean | missng XML code comments | missng XML code comments |
| FixTeamSettingsForExistingTeams | Boolean | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TeamMigrationConfig",
  "Enabled": false,
  "PrefixProjectToNodes": false,
  "EnableTeamSettingsMigration": true,
  "FixTeamSettingsForExistingTeams": false
}
```