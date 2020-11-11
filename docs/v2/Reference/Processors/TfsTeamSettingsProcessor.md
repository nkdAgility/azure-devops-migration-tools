## Endpoint: TfsTeamSettingsProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.. /./ index.md) > [Reference](.. / index.md) > [Processors](./index.md) > **TfsTeamSettingsProcessor**

No description, create a template

Options not yet implmeneted

```JSON
{
  "ObjectType": "TfsTeamSettingsProcessorOptions",
  "MigrateTeamSettings": true,
  "UpdateTeamSettings": true,
  "PrefixProjectToNodes": false,
  "Enabled": false,
  "Endpoints": [
    {
      "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
      "Organisation": "https://dev.azure.com/nkdagility-preview/",
      "Project": "sourceProject",
      "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
      "Direction": "Source",
      "Enrichers": null
    },
    {
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