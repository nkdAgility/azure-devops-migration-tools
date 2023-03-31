---
title: TeamMigrationContext
layout: default
template: default
pageType: reference
classType: Processors
architecture: v1
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

Migrates Teams and Team Settings: This should be run after `NodeStructuresMigrationConfig` and before all other processors.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| EnableTeamSettingsMigration | Boolean | Migrate original team settings after their creation on target team project | true |
| FixTeamSettingsForExistingTeams | Boolean | Reset the target team settings to match the source if the team exists | true |
| PrefixProjectToNodes | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false |
{: .table .table-striped .table-bordered .d-none .d-md-block}


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