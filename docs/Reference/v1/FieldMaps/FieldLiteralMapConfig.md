---
title: FieldLiteralMapConfig
layout: default
template: default
pageType: reference
classType: FieldMaps
architecture: v1
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

Sets a field on the `target` to b a specific value.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| targetField | String | missng XML code comments | missng XML code comments |
| value | String | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |
{: .table .table-striped .table-bordered .d-none .d-md-block}


### Example JSON

```JSON
{
  "$type": "FieldLiteralMapConfig",
  "WorkItemTypeName": "*",
  "targetField": "System.Status",
  "value": "New"
}
```