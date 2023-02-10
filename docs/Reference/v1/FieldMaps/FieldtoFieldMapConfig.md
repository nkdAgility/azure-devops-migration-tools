## FieldMaps: FieldtoFieldMapConfig

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](/docs/index.md) > [Reference](/docs/Reference/index.md) > [API v1](/docs/Reference/v1/index.md) > [FieldMaps](/docs/Reference/v1/FieldMaps/index.md)> **FieldtoFieldMapConfig**

Just want to map one field to another? This is the one for you.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| defaultValue | String | missng XML code comments | missng XML code comments |
| sourceField | String | missng XML code comments | missng XML code comments |
| targetField | String | missng XML code comments | missng XML code comments |
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "FieldtoFieldMapConfig",
  "WorkItemTypeName": "*",
  "sourceField": "System.StackRank",
  "targetField": "System.Rank",
  "defaultValue": "1000"
}
```