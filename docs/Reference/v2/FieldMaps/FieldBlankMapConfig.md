## FieldMaps: FieldBlankMapConfig

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v2](../index.md) > [FieldMaps](index.md)> **FieldBlankMapConfig**

Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| targetField | String | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "FieldBlankMapConfig",
  "WorkItemTypeName": "*",
  "targetField": "System.Description"
}
```