## Processors: AzureDevOpsPipelineProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Processors](./index.md) > **AzureDevOpsPipelineProcessor**

Azure DevOps Processor that migrates Taskgroups, Buils- and Release Pipelines.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| MigrateBuildPipelines | Boolean | Migrate Build Pipelines | true |
| MigrateReleasePipelines | Boolean | Migrate Release Pipelines | true |
| MigrateTaskGroups | Boolean | Migrate Task Groups | true |
| BuildPipelines | List | List of Build Pipelines to process. If this is `null` then all Build Pipelines will be processed. | missng XML code comments |
| ReleasePipelines | List | List of Release Pipelines to process. If this is `null` then all Release Pipelines will be processed. | missng XML code comments |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| Source | IEndpointOptions | This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor. | missng XML code comments |
| Target | IEndpointOptions | This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a write only processor. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "AzureDevOpsPipelineProcessorOptions",
  "Enabled": false,
  "MigrateBuildPipelines": true,
  "MigrateReleasePipelines": true,
  "MigrateTaskGroups": true,
  "BuildPipelines": null,
  "ReleasePipelines": null,
  "ProcessorEnrichers": null,
  "Source": {
    "$type": "AzureDevOpsEndpointOptions",
    "Organisation": "https://dev.azure.com/nkdagility-preview/",
    "Project": "sourceProject",
    "AuthenticationMode": "AccessToken",
    "AccessToken": "qosss7crwz3vie4fupzpaafjndoy6g6ulgkzhoxtmjgicv2lqjyq",
    "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
    "EndpointEnrichers": null
  },
  "Target": {
    "$type": "AzureDevOpsEndpointOptions",
    "Organisation": "https://dev.azure.com/nkdagility-preview/",
    "Project": "targetProject",
    "AuthenticationMode": "AccessToken",
    "AccessToken": "qosss7crwz3vie4fupzpaafjndoy6g6ulgkzhoxtmjgicv2lqjyq",
    "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
    "EndpointEnrichers": null
  }
}
```