---
title: RegexFieldMapConfig
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

I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| pattern | String | missng XML code comments | missng XML code comments |
| replacement | String | missng XML code comments | missng XML code comments |
| sourceField | String | missng XML code comments | missng XML code comments |
| targetField | String | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "RegexFieldMapConfig",
  "WorkItemTypeName": "*",
  "sourceField": "Custom.MyVersion",
  "targetField": "Custom.MyVersionYearOnly",
  "pattern": "([0-9]{4})",
  "replacement": "$1"
}
```