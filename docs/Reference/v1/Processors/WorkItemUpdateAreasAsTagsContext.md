---
title: WorkItemUpdateAreasAsTagsContext
layout: default
pageType: reference
classType: Processors
architecture: v1
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

A common issue with older *TFS/Azure DevOps* instances is the proliferation of `Area Paths`. With the use of `Area Path` for `Teams` and the addition of the `Node Name` column option these extensive tag hierarchies should instad be moved to tags.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| AreaIterationPath | String | This is a required parameter. That define the root path of the iteration. To get the full path use `\` | \ |
| Enabled | Boolean | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "WorkItemUpdateAreasAsTagsConfig",
  "Enabled": false,
  "AreaIterationPath": null
}
```