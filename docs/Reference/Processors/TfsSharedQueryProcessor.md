## Processors: TfsSharedQueryProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Processors](./index.md) > **TfsSharedQueryProcessor**

The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| PrefixProjectToNodes | Boolean | Do we add the source project name into the folder path | false |
| SharedFolderName | String | The name of the shared folder, made a parameter incase it every needs to be edited | Shared Queries |
| SourceToTargetFieldMappings | Dictionary`2 | Mapping of the source to the target | missng XML code comments |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| Source | IEndpointOptions | This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor. | missng XML code comments |
| Target | IEndpointOptions | This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a write only processor. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |


### Example JSON

```JSON
{
    "Version": "11.9",
    "LogLevel": "Verbose",
    
    "Endpoints": {
      "TfsEndpoints": [
        {
          "Name": "Source",
          "AccessToken": "",
          "Query": {
            "Query": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc"
          },
          "Organisation": "https://dev.azure.com/like10-demos/",
          "Project": "SourceProject",
          "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
          "AuthenticationMode": "Prompt",
          "AllowCrossProjectLinking": false,
          "PersonalAccessToken": "",
          "LanguageMaps": {
            "AreaPath": "Area",
            "IterationPath": "Iteration"
          }
        },
        {
          "Name": "Target",
          "AccessToken": "",
          "Query": {
            "Query": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc"
          },
          "Organisation": "https://dev.azure.com/like10-demos/",
          "Project": "TargetProject",
          "ReflectedWorkItemIdField": "nkdScrum.ReflectedWorkItemId",
          "AuthenticationMode": "Prompt",
          "AllowCrossProjectLinking": false,
          "LanguageMaps": {
            "AreaPath": "Area",
            "IterationPath": "Iteration"
          }
        }
      ]
    },
    "Processors": [     
      {
        "$type": "TfsSharedQueryProcessorOptions",
        "Enabled": true,
        "PrefixProjectToNodes": false,
        "SharedFolderName": "Shared Queries",
        "SourceToTargetFieldMappings": null,
        "ProcessorEnrichers": null,
        "SourceName": "Source",
        "TargetName": "Target"
      }
    ]
  }
```
