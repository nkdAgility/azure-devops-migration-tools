## FieldMaps: MultiValueConditionalMapConfig

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v2](../index.md) > [FieldMaps](index.md)> **MultiValueConditionalMapConfig**

??? If you know how to use this please send a PR :)

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| sourceFieldsAndValues | Dictionary | missng XML code comments | missng XML code comments |
| targetFieldsAndValues | Dictionary | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "MultiValueConditionalMapConfig",
  "WorkItemTypeName": "*",
  "sourceFieldsAndValues": {
    "$type": "Dictionary`2",
    "Something": "SomethingElse"
  },
  "targetFieldsAndValues": {
    "$type": "Dictionary`2",
    "Something": "SomethingElse"
  }
}
```