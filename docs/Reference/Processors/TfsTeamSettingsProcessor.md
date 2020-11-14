## Processors: TfsTeamSettingsProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Processors](./index.md) > **TfsTeamSettingsProcessor**

Native TFS Processor, does not work with any other Endpoints.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| MigrateTeamSettings | Boolean | missng XML code comments | missng XML code comments |
| UpdateTeamSettings | Boolean | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | missng XML code comments | missng XML code comments |
| Teams | List | missng XML code comments | missng XML code comments |
| Enabled | Boolean | Active the processor if it true. | missng XML code comments |
| Endpoints | List | missng XML code comments | missng XML code comments |
| ProcessorEnrichers | List | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "MigrationTools.Processors.TfsTeamSettingsProcessorOptions, MigrationTools.Clients.AzureDevops.ObjectModel",
  "Enabled": false,
  "MigrateTeamSettings": true,
  "UpdateTeamSettings": true,
  "PrefixProjectToNodes": false,
  "Teams": null,
  "Endpoints": [
    {
      "$type": "MigrationTools.Endpoints.TfsEndpointOptions, MigrationTools.Clients.AzureDevops.ObjectModel",
      "Direction": "Source",
      "Organisation": "https://dev.azure.com/nkdagility-preview/",
      "Project": "sourceProject",
      "AuthenticationMode": "AccessToken",
      "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
      "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
      "EndpointEnrichers": null
    },
    {
      "$type": "MigrationTools.Endpoints.TfsEndpointOptions, MigrationTools.Clients.AzureDevops.ObjectModel",
      "Direction": "Target",
      "Organisation": "https://dev.azure.com/nkdagility-preview/",
      "Project": "targetProject",
      "AuthenticationMode": "AccessToken",
      "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
      "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
      "EndpointEnrichers": null
    }
  ],
  "ProcessorEnrichers": null
}
```