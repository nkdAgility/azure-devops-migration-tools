---
title: TfsAreaAndIterationProcessor
layout: default
template: default
pageType: reference
classType: Processors
architecture: v2
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

The `TfsAreaAndIterationProcessor` migrates all of the Area nd Iteraion paths.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| AreaMaps | Dictionary | missng XML code comments | missng XML code comments |
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| IterationMaps | Dictionary | missng XML code comments | missng XML code comments |
| NodeBasePaths | String[] | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |
| SourceName | String | missng XML code comments | missng XML code comments |
| TargetName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TfsAreaAndIterationProcessorOptions",
  "Enabled": false,
  "PrefixProjectToNodes": false,
  "NodeBasePaths": null,
  "AreaMaps": {
    "$type": "Dictionary`2"
  },
  "IterationMaps": {
    "$type": "Dictionary`2"
  },
  "ProcessorEnrichers": null,
  "SourceName": "sourceName",
  "TargetName": "targetName"
}
```