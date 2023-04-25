---
title: FieldValueMapConfig
layout: default
template: default
pageType: reference
classType: FieldMaps
architecture: v1
toc: true
pageStatus: generated
discussionId: 
redirect_from: 
 - /Reference/v1/FieldMaps/FieldValueMapConfig.html
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

Need to map not just the field but also values? This is the default value mapper.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| defaultValue | String | missng XML code comments | missng XML code comments |
| sourceField | String | missng XML code comments | missng XML code comments |
| targetField | String | missng XML code comments | missng XML code comments |
| valueMapping | Dictionary | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "FieldValueMapConfig",
  "WorkItemTypeName": "*",
  "sourceField": "System.Status",
  "targetField": "System.Status",
  "defaultValue": "New",
  "valueMapping": {
    "$type": "Dictionary`2",
    "New": "New",
    "Active": "Committed",
    "Resolved": "Committed",
    "Closed": "Done"
  }
}
```