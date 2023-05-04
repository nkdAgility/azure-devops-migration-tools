---
title: TreeToTagMapConfig
layout: page
template: default
pageType: reference
classType: FieldMaps
architecture: v2
toc: true
pageStatus: generated
discussionId: 
redirect_from: 
 - /Reference/v2/FieldMaps/TreeToTagMapConfig.html
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| timeTravel | Int32 | missng XML code comments | missng XML code comments |
| toSkip | Int32 | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TreeToTagMapConfig",
  "WorkItemTypeName": "*",
  "toSkip": 2,
  "timeTravel": 0
}
```