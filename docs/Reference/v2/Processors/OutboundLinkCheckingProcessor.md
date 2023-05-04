---
title: <ClassName>
layout: page
template: <template>
pageType: reference
classType: <TypeName>
architecture: <architecture>
toc: true
pageStatus: generated
discussionId: 
---

missng XML code comments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |
| ResultFileName | String | missng XML code comments | missng XML code comments |
| SourceName | String | missng XML code comments | missng XML code comments |
| TargetName | String | missng XML code comments | missng XML code comments |
| WIQLQueryBit | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "OutboundLinkCheckingProcessorOptions",
  "Enabled": false,
  "WIQLQueryBit": "Select [System.Id] From WorkItems Where [System.TeamProject] = @project and not [System.WorkItemType] contains 'Test Suite, Test Plan,Shared Steps,Shared Parameter,Feedback Request'",
  "ResultFileName": "c:/temp/OutboundLinks.csv",
  "ProcessorEnrichers": null,
  "SourceName": null,
  "TargetName": null
}
```