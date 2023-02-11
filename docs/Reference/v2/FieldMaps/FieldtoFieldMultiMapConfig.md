## FieldMaps: FieldtoFieldMultiMapConfig

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v2](../index.md) > [FieldMaps](index.md)> **FieldtoFieldMultiMapConfig**

Want to setup a bunch of field maps in a single go. Use this shortcut!

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| SourceToTargetMappings | Dictionary`2 | missng XML code comments | missng XML code comments |
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