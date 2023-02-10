## Processors: TfsTeamSettingsProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](/docs/index.md) > [Reference](/docs/Reference/index.md) > [API v2](/docs/Reference/v2/index.md) > [Processors](/docs/Reference/v2/Processors/index.md)> **TfsTeamSettingsProcessor**

Native TFS Processor, does not work with any other Endpoints.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| MigrateTeamCapacities | Boolean | Migrate original team member capacities after their creation on the target team project. Note: It will only migrate team member capacity if the team member with same display name exists on the target collection otherwise it will be ignored. | false |
| MigrateTeamSettings | Boolean | Migrate original team settings after their creation on target team project | false |
| PrefixProjectToNodes | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |
| SourceName | String | missng XML code comments | missng XML code comments |
| TargetName | String | missng XML code comments | missng XML code comments |
| Teams | List | List of Teams to process. If this is `null` then all teams will be processed. | missng XML code comments |
| UpdateTeamSettings | Boolean | Reset the target team settings to match the source if the team exists | false |


### Example JSON

```JSON
{
  "$type": "TfsTeamSettingsProcessorOptions",
  "Enabled": false,
  "MigrateTeamSettings": true,
  "UpdateTeamSettings": true,
  "PrefixProjectToNodes": false,
  "MigrateTeamCapacities": false,
  "Teams": null,
  "ProcessorEnrichers": null,
  "SourceName": "sourceName",
  "TargetName": "targetName"
}
```