## Processors: TfsSharedQueryProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Processors](./index.md) > **TfsSharedQueryProcessor**

The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | Do we add the source project name into the folder path | false |
| SharedFolderName | String | The name of the shared folder, made a parameter incase it every needs to be edited | Shared Queries |
| SourceToTargetFieldMappings | Dictionary`2 | Mapping of the source to the target | missng XML code comments |
| ProcessorEnrichers | List | missng XML code comments | missng XML code comments |
| Source | IEndpointOptions | missng XML code comments | missng XML code comments |
| Target | IEndpointOptions | missng XML code comments | missng XML code comments |
| RefName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TfsSharedQueryProcessorOptions",
  "Enabled": false,
  "PrefixProjectToNodes": false,
  "SharedFolderName": "Shared Queries",
  "SourceToTargetFieldMappings": null,
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