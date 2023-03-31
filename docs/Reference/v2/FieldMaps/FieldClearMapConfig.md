---
title: FieldClearMapConfig
layout: default
template: default
pageType: reference
classType: FieldMaps
architecture: v2
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

Allows you to set an already populated field to Null. This will only work with fields that support null.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| targetField | String | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |

{: .table .table-striped .table-bordered .d-none .d-md-block}

### Example JSON

```JSON
{
  "$type": "FieldClearMapConfig",
  "WorkItemTypeName": "*",
  "targetField": "System.Description"
}
```