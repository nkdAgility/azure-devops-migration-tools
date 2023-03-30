## Processors: AzureDevOpsPipelineProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v2](../index.md) > [Processors](index.md)> **AzureDevOpsPipelineProcessor**

Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines.

### Features
- Migrates service connections
- Migrates variable groups
- Migrates task groups
- Migrates classic and yml build pipelines
- Migrates release pipelines

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| BuildPipelines | List | List of Build Pipelines to process. If this is `null` then all Build Pipelines will be processed. | missng XML code comments |
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| MigrateBuildPipelines | Boolean | Migrate Build Pipelines | true |
| MigrateReleasePipelines | Boolean | Migrate Release Pipelines | true |
| MigrateServiceConnections | Boolean | Migrate Service Connections **secrets need to be entered manually** | true |
| MigrateTaskGroups | Boolean | Migrate Task Groups | true |
| MigrateVariableGroups | Boolean | Migrate Valiable Groups | true |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |
| ReleasePipelines | List | List of Release Pipelines to process. If this is `null` then all Release Pipelines will be processed. | missng XML code comments |
| RepositoryNameMaps | Dictionary | Map of Source Repository to Target Repository Names | missng XML code comments |
| SourceName | String | missng XML code comments | missng XML code comments |
| TargetName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "AzureDevOpsPipelineProcessorOptions",
  "Enabled": false,
  "MigrateBuildPipelines": true,
  "MigrateReleasePipelines": true,
  "MigrateTaskGroups": true,
  "MigrateVariableGroups": true,
  "MigrateServiceConnections": true,
  "BuildPipelines": null,
  "ReleasePipelines": null,
  "RepositoryNameMaps": null,
  "ProcessorEnrichers": null,
  "SourceName": "sourceName",
  "TargetName": "targetName"
}
```


### Example Full Migration from v12.0

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