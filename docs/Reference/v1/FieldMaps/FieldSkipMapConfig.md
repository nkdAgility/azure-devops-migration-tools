## FieldMaps: FieldSkipMapConfig

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v1](../index.md) > [FieldMaps](index.md)> **FieldSkipMapConfig**

Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| targetField | String | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "FieldSkipMapConfig",
  "WorkItemTypeName": "*",
  "targetField": "System.Description"
}
```