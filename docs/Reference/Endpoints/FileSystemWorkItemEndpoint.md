## Endpoints: FileSystemWorkItemEndpoint

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Endpoints](./index.md) > **FileSystemWorkItemEndpoint**

missng XML code comments

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| FileStore | String | missng XML code comments | missng XML code comments |
| Direction | EndpointDirection | missng XML code comments | missng XML code comments |
| EndpointEnrichers | List | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions, MigrationTools.Clients.FileSystem",
  "FileStore": "c:\\temp\\Store",
  "Direction": "Source",
  "EndpointEnrichers": null
}
```