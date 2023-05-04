---
title: TfsSharedQueryProcessor
layout: page
template: default
pageType: reference
classType: Processors
architecture: v2
toc: true
pageStatus: generated
discussionId: 
redirect_from: 
 - /Reference/v2/Processors/TfsSharedQueryProcessor.html
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| PrefixProjectToNodes | Boolean | Do we add the source project name into the folder path | false |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |
| SharedFolderName | String | The name of the shared folder, made a parameter incase it every needs to be edited | Shared Queries |
| SourceName | String | missng XML code comments | missng XML code comments |
| SourceToTargetFieldMappings | Dictionary | Mapping of the source to the target | missng XML code comments |
| TargetName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TfsSharedQueryProcessorOptions",
  "Enabled": false,
  "PrefixProjectToNodes": false,
  "SharedFolderName": "Shared Queries",
  "SourceToTargetFieldMappings": null,
  "ProcessorEnrichers": null,
  "SourceName": "sourceName",
  "TargetName": "targetName"
}
```