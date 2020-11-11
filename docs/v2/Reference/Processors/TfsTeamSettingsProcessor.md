## Processor: TfsTeamSettingsProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Processors](./index.md) > **Tfs TeamSettings Processor**

The Team Settings Processor will allow the migration and manipulation of the Teams within Azure DevOps. 

Property | Type
-------- | ----
MigrateTeamSettings | Boolean
UpdateTeamSettings | Boolean
PrefixProjectToNodes | Boolean
Processor | String
ToConfigure | Type
Enabled | Boolean
Endpoints | List`1
Enrichers | List`1


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