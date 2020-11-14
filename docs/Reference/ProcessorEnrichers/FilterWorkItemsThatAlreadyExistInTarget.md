## ProcessorEnrichers: FilterWorkItemsThatAlreadyExistInTarget

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [ProcessorEnrichers](./index.md) > **FilterWorkItemsThatAlreadyExistInTarget**

missng XML code comments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Query | QueryOptions | missng XML code comments | missng XML code comments |
| ToConfigure | Type | missng XML code comments | missng XML code comments |
| Enabled | Boolean | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "MigrationTools.Enrichers.FilterWorkItemsThatAlreadyExistInTargetOptions, MigrationTools",
  "Query": {
    "$type": "MigrationTools.Options.QueryOptions, MigrationTools",
    "Query": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc",
    "Paramiters": null
  },
  "Enabled": true
}
```