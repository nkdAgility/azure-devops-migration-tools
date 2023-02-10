## Processors: WorkItemQueryMigrationContext

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](/docs/index.md) > [Reference](/docs/Reference/index.md) > [API v1](/docs/Reference/v1/index.md) > [Processors](/docs/Reference/v1/Processors/index.md)> **WorkItemQueryMigrationContext**

Moved Shared Queries best effort

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | Do we add the source project name into the folder path | missng XML code comments |
| SharedFolderName | String | The name of the shared folder, made a parameter incase it every needs to be edited | missng XML code comments |
| SourceToTargetFieldMappings | Dictionary`2 | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "WorkItemQueryMigrationConfig",
  "Enabled": false,
  "PrefixProjectToNodes": false,
  "SharedFolderName": "Shared Queries",
  "SourceToTargetFieldMappings": null
}
```