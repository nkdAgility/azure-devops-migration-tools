## Endpoints

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**


[Overview](.././index.md) > [Reference](../index.md) > *Endpoints*

Azyre DevOps Migration Tools provides _endpoints_ for reading and writing `WorkItems`, `PlansAndSuits`, `Teams`, or `Queries`. 

Client  | WriteTo/ReadFrom | Endpoint | Data Target | Description
----------|-----------|------------
AzureDevops.ObjectModel | Tfs Object Model | `TfsWorkItemEndPoint` | WorkItems | TBA
AzureDevops.Rest | Azure DevOps REST | ?
FileSystem | Local Files | `FileSystemWorkItemEndpoint` | WorkItems | TBA

### Endpoints Options

 All Endpoints have a minimum set of options that are required to run. 

#### Minimum Options to run

The `Direction` option is required to allow the system to set direction. At a minimum you need to set a `Source`.


```JSON
    {
      "ObjectType": "EndpointOptions",
      "Direction": "Source"
    }
```
