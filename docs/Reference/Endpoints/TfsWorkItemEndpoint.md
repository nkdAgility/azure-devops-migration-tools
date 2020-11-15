## Endpoint: TfsWorkItemEndpoint

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Endpoints](./index.md) > TfsWorkItemEndpoint

The Work Item endpoint is super awesome.

Client  | WriteTo/ReadFrom | Endpoint | Data Target | Description
----------|-----------|------------
AzureDevops.ObjectModel | Tfs Object Model | `TfsWorkItemEndPoint` | WorkItems | TBA
AzureDevops.Rest | Azure DevOps REST | ?
FileSystem | Local Files | `FileSystemWorkItemEndpoint` | WorkItems | TBA

### Endpoints Options

 All Endpoints have a minimum set of options that are required to run. 

#### Minimum Options to run

The `Direction` option is required to allow the system to set direction. At a minimum you need to set a `Source`.


```JSON
    {
      {
  "$type": "TfsWorkItemEndpointOptions",
  "Organisation": "https://dev.azure.com/nkdagility-preview/",
  "Project": "NeedToSetThis",
  "Query": {
    "$type": "QueryOptions",
    "Query": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc",
    "Paramiters": {
      "$type": "Dictionary`2",
      "TeamProject": "migrationSource1"
    }
  },
  "AuthenticationMode": "AccessToken",
  "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
  "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
  "EndpointEnrichers": null,
  "RefName": null
}
    }
```
