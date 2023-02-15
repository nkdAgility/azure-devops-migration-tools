## FieldMaps: TreeToTagMapConfig

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v1](../index.md) > [FieldMaps](index.md)> **TreeToTagMapConfig**

Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| timeTravel | Int32 | missng XML code comments | missng XML code comments |
| toSkip | Int32 | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TreeToTagMapConfig",
  "WorkItemTypeName": "*",
  "toSkip": 2,
  "timeTravel": 0
}
```