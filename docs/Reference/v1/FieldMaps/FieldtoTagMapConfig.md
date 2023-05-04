---
title: FieldtoTagMapConfig
layout: page
template: default
pageType: reference
classType: FieldMaps
architecture: v1
toc: true
pageStatus: generated
discussionId: 
redirect_from: 
 - /Reference/v1/FieldMaps/FieldtoTagMapConfig.html
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

Want to take a field and convert its value to a tag? Done...

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| formatExpression | String | missng XML code comments | missng XML code comments |
| sourceField | String | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "FieldtoTagMapConfig",
  "WorkItemTypeName": "*",
  "sourceField": "Custom.ProjectName",
  "formatExpression": "Project: {0}"
}
```