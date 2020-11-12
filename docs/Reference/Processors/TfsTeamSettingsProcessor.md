## Processors: TfsTeamSettingsProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Processors](./index.md) > **TfsTeamSettingsProcessor**

No description, create a template

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| MigrateTeamSettings | Boolean | {Description} | {Default Value} |
| UpdateTeamSettings | Boolean | {Description} | {Default Value} |
| PrefixProjectToNodes | Boolean | {Description} | {Default Value} |
| Teams | List | {Description} | {Default Value} |
| Processor | String | {Description} | {Default Value} |
| ToConfigure | Type | {Description} | {Default Value} |
| Enabled | Boolean | {Description} | {Default Value} |
| Endpoints | List | {Description} | {Default Value} |
| Enrichers | List | {Description} | {Default Value} |


### Example JSON

```JSON
{
  "ObjectType": "TfsTeamSettingsProcessorOptions",
  "MigrateTeamSettings": true,
  "UpdateTeamSettings": true,
  "PrefixProjectToNodes": false,
  "Teams": null,
  "Enabled": false,
  "Endpoints": [
    {
      "AuthenticationMode": "AccessToken",
      "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
      "Organisation": "https://dev.azure.com/nkdagility-preview/",
      "Project": "sourceProject",
      "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
      "Direction": "Source",
      "Enrichers": null
    },
    {
      "AuthenticationMode": "AccessToken",
      "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
      "Organisation": "https://dev.azure.com/nkdagility-preview/",
      "Project": "targetProject",
      "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
      "Direction": "Target",
      "Enrichers": null
    }
  ],
  "Enrichers": null
}
```