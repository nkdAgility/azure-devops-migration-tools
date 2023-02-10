## FieldMaps: FieldBlankMapConfig

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](/docs/index.md) > [Reference](/docs/Reference/index.md) > [API v2](/docs/Reference/v2/index.md) > [FieldMaps](/docs/Reference/v2/FieldMaps/index.md)> **FieldBlankMapConfig**

Allows you to blank an already populated field

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| WorkItemTypeName | String | missng XML code comments | missng XML code comments |
| targetField | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "FieldBlankMapConfig",
  "WorkItemTypeName": "*",
  "targetField": "System.Description"
}
```