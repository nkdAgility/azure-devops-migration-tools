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
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
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
    "EndpointEnrichers": null,
    "RefName": null
  },
  "Target": {
    "$type": "TfsEndpointOptions",
    "Organisation": "https://dev.azure.com/nkdagility-preview/",
    "Project": "targetProject",
    "AuthenticationMode": "AccessToken",
    "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
    "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
    "EndpointEnrichers": null,
    "RefName": null
  },
  "RefName": null
}
```