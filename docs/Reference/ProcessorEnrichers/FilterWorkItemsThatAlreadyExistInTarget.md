## ProcessorEnrichers: FilterWorkItemsThatAlreadyExistInTarget

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [ProcessorEnrichers](./index.md) > **FilterWorkItemsThatAlreadyExistInTarget**

No description, create a template

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Query | QueryOptions | {Description} | {Default Value} |
| ToConfigure | Type | {Description} | {Default Value} |
| Enabled | Boolean | {Description} | {Default Value} |


### Example JSON

```JSON
{
  "ObjectType": "FilterWorkItemsThatAlreadyExistInTargetOptions",
  "Query": {
    "Query": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc",
    "Paramiters": null
  },
  "Enabled": true
}
```