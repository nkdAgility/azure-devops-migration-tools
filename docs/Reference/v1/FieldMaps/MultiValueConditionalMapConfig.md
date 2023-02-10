## FieldMaps: MultiValueConditionalMapConfig

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](/docs/index.md) > [Reference](/docs/Reference/index.md) > [API v1](/docs/Reference/v1/index.md) > [FieldMaps](/docs/Reference/v1/FieldMaps/index.md)> **MultiValueConditionalMapConfig**

??? If you know how to use this please send a PR :)

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |
| sourceFieldsAndValues | Dictionary`2 | missng XML code comments | missng XML code comments |
| targetFieldsAndValues | Dictionary`2 | missng XML code comments | missng XML code comments |


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