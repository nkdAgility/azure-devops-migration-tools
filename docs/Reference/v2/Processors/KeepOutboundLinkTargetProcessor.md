## Processors: KeepOutboundLinkTargetProcessor

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../././index.md) > [Reference](.././index.md) > [API v2](../index.md) > [Processors](./index.md)> **KeepOutboundLinkTargetProcessor**

missng XML code comments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| WIQLQueryBit | String | missng XML code comments | missng XML code comments |
| TargetLinksToKeepOrganization | String | missng XML code comments | missng XML code comments |
| TargetLinksToKeepProject | String | missng XML code comments | missng XML code comments |
| CleanupFileName | String | missng XML code comments | missng XML code comments |
| PrependCommand | String | missng XML code comments | missng XML code comments |
| DryRun | Boolean | missng XML code comments | missng XML code comments |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| SourceName | String | missng XML code comments | missng XML code comments |
| TargetName | String | missng XML code comments | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "KeepOutboundLinkTargetProcessorOptions",
  "Enabled": false,
  "WIQLQueryBit": "Select [System.Id] From WorkItems Where [System.TeamProject] = @project and not [System.WorkItemType] contains 'Test Suite, Test Plan,Shared Steps,Shared Parameter,Feedback Request'",
  "TargetLinksToKeepOrganization": "https://dev.azure.com/nkdagility",
  "TargetLinksToKeepProject": "ada193e6-f4f6-4bac-9fea-47c2a463f6c7",
  "CleanupFileName": "c:/temp/OutboundLinkTargets.bat",
  "PrependCommand": "start",
  "DryRun": true,
  "ProcessorEnrichers": null,
  "SourceName": null,
  "TargetName": null
}
```