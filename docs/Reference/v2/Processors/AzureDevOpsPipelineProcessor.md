## Processors: AzureDevOpsPipelineProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](/docs/index.md) > [Reference](/docs/Reference/index.md) > [API v2](/docs/Reference/v2/index.md) > [Processors](/docs/Reference/v2/Processors/index.md)> **AzureDevOpsPipelineProcessor**

Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines.

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
| RepositoryNameMaps | Dictionary`2 | Map of Source Repository to Target Repository Names | missng XML code comments |
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