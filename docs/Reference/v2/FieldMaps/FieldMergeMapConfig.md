---
title: FieldMergeMapConfig
layout: default
pageType: reference
classType: FieldMaps
architecture: v2
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

Ever wanted to merge two or three fields? This mapping will let you do just that.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| formatExpression | String | missng XML code comments | missng XML code comments |
| sourceFields | List | missng XML code comments | missng XML code comments |
| targetField | String | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "FieldMergeMapConfig",
  "WorkItemTypeName": "*",
  "sourceFields": [
    "System.Description",
    "System.Status"
  ],
  "targetField": "System.Description",
  "formatExpression": "{0} \n {1}"
}
```