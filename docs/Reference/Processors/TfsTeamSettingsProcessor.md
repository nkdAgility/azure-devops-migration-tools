## Processors: TfsTeamSettingsProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Processors](./index.md) > **TfsTeamSettingsProcessor**

Native TFS Processor, does not work with any other Endpoints.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| MigrateTeamSettings | Boolean | Migrate original team settings after their creation on target team project | false |
| UpdateTeamSettings | Boolean | Reset the target team settings to match the source if the team exists | false |
| PrefixProjectToNodes | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false |
| Teams | List | List of Teams to process. If this is `null` then all teams will be processed. | missng XML code comments |
| ProcessorEnrichers | List | missng XML code comments | missng XML code comments |
| Source | IEndpointOptions | missng XML code comments | missng XML code comments |
| Target | IEndpointOptions | missng XML code comments | missng XML code comments |
| RefName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TfsTeamSettingsProcessorOptions",
  "Enabled": false,
  "MigrateTeamSettings": true,
  "UpdateTeamSettings": true,
  "PrefixProjectToNodes": false,
  "Teams": null,
  "ProcessorEnrichers": null,
  "Source": {
    "$type": "TfsEndpointOptions",
    "Organisation": "https://dev.azure.com/nkdagility-preview/",
    "Project": "sourceProject",
    "AuthenticationMode": "AccessToken",
    "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
    "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
    "EndpointEnrichers": null
  },
  "Target": {
    "$type": "TfsEndpointOptions",
    "Organisation": "https://dev.azure.com/nkdagility-preview/",
    "Project": "targetProject",
    "AuthenticationMode": "AccessToken",
    "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
    "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
    "EndpointEnrichers": null
  }
}
```