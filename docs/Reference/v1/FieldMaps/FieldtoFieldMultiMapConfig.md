---
title: FieldtoFieldMultiMapConfig
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

Want to setup a bunch of field maps in a single go. Use this shortcut!

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| SourceToTargetMappings | Dictionary | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "FieldtoFieldMultiMapConfig",
  "WorkItemTypeName": "*",
  "SourceToTargetMappings": {
    "$type": "Dictionary`2",
    "Custom.Field1": "Custom.Field4",
    "Custom.Field2": "Custom.Field5",
    "Custom.Field3": "Custom.Field6"
  }
}
```