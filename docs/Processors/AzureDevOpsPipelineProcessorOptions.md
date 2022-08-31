# Azure DevOps Pipeline processor options
AzureDevOpsPipelineProcessorOptions is the main processor used for migrating build pipelines and release pipelines.

# Features
- Migrates service connections
- Migrates variable groups
- Migrates task groups
- Migrates classic and yml build pipelines
- Migrates release pipelines

# Params
| Parameter name                            | Type            | Description                                                                                                                                                                                                                                                                    | Default Value                                                                |
| :---------------------------------------- | :-------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------- |
| `$type `                                  | string          | The name of the processor's options | AzureDevOpsPipelineProcessorOptions |
| `Enabled`                                 | Boolean         | Activate the processor if this value is true | false |
| `MigrateBuildPipelines`                   | Boolean         | Should classic and/or yml build pipelines be migrated | true |
| `MigrateReleasePipelines`                 | Boolean         | Should release pipelines be migrated | true |
| `MigrateTaskGroups`                       | Boolean         | Should task groups be migrated | true |
| `MigrateVariableGroups`                   | Boolean         | Should variable groups be migrated | true |
| `MigrateServiceConnections`               | Boolean         | Should service connections be migrated | true |
| `BuildPipelines`                          | List`<string`>  | List of Build Pipelines to process. If this is `null` then all Build Pipelines will be processed. | null |
| `ReleasePipelines`                        | List`<string`>  | List of Release Pipelines to process. If this is `null` then all Release Pipelines will be processed. | null |
| `RefName`                                 | string          | Used in the "future" to allow for using named Options without the need to copy all of the options. | null |
| `SourceName`                              | string          | Name of the `AzureDevOpsEndpoints` endpoint that is used as the source | sourceName |
| `TargetName`                              | string          | Name of the `AzureDevOpsEndpoints` endpoint that is used as the target | targetName |
| `RepositoryNameMaps`                      | Dictionary`<string, string`> | Map of Source Repository to Target Repository Names | {} |

# Example
The following file is an example that can be used in your `configuration.json` file to migrate Azure DevOps pipelines.
```json
{
    "GitRepoMapping": null,
    "LogLevel": "Information",
    "Processors": [
      {
        "$type": "AzureDevOpsPipelineProcessorOptions",
        "Enabled": true,
        "MigrateBuildPipelines": true,
        "MigrateReleasePipelines": true,
        "MigrateTaskGroups": true,
        "MigrateVariableGroups": true,
        "MigrateServiceConnections": true,
        "BuildPipelines": null,
        "ReleasePipelines": null,
        "RefName": null,
        "SourceName": "Source",
        "TargetName": "Target",
        "RepositoryNameMaps": {}
      }
    ],
    "Version": "12.0",
    "Endpoints": {
      "AzureDevOpsEndpoints": [
        {
          "name": "Source",
          "$type": "AzureDevOpsEndpointOptions",
          "Organisation": "https://dev.azure.com/source-org/",
          "Project": "Awesome project",
          "AuthenticationMode": "AccessToken",
          "AccessToken": "xxxxxx",
          "EndpointEnrichers": null
        },
        {
          "Name": "Target",
          "$type": "AzureDevOpsEndpointOptions",
          "Organisation": "https://dev.azure.com/target-org/",
          "Project": "Cool project",
          "AuthenticationMode": "AccessToken",
          "AccessToken": "xxxxxx",
          "EndpointEnrichers": null
        }
      ]
    }
  }
```

If the repository in the target has a different name from the one that was used in the source project, you should map it.
In the example above replace `"RepositoryNameMaps": {}` with the following:
```json
"RepositoryNameMaps": {
    "Awesome project": "Cool project"
}
```

# Important note
When the application is creating service connections make sure you have proper permissions on Azure Active Directory and you can grant Contributor role to the subscription that was chosen.
