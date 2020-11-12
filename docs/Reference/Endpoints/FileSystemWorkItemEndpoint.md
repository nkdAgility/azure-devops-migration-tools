## Endpoints: FileSystemWorkItemEndpoint

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Endpoints](./index.md) > **FileSystemWorkItemEndpoint**

No description, create a template

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| FileStore | String | {Description} | {Default Value} |
| ToConfigure | Type | {Description} | {Default Value} |
| Direction | EndpointDirection | {Description} | {Default Value} |
| Enrichers | List | {Description} | {Default Value} |


### Example JSON

```JSON
{
  "ObjectType": "FileSystemWorkItemEndpointOptions",
  "FileStore": "c:\\temp\\Store",
  "Direction": "Source",
  "Enrichers": null
}
```