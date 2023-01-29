## Reference Overview


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**


[Overview](.././index.md) > **Reference**

The system works by setting one or more [Processors](../Reference/Processors/index.md) in the json 
configuration file. This processor can have many [ProcessorEnrichers](../Reference/ProcessorEnrichers/index.md) that 
enable additional features, and must have at least two [Endpoints](../Reference/Endpoints/index.md); 
a *Source* `Endpoint` and a *Target* `Endpoint`. Each `Endpoint` 
may have additional [EndpointEnrichers](../Reference/EndpointEnrichers/index.md) that add 
additional *Client* specific functionality.

### What types of things do we have

- **[Processors](../Reference/Processors/index.md)** - Processors allow you to move different types of data between `Endpoints` and does not care what `Endpoint` you have on each end.
- **[Processor Enrichers](../Reference/ProcessorEnrichers/index.md)** - Enrichers at the processor level allow you to add additional functionality to a processor without endangering the core functionality. Each Enricher should have a single responsibility and can add functionality to the following stages of the processor pipeline.
- **[Endpoints](../Reference/Endpoints/index.md)** connect to the target system and load and save the data. Endpoint can load or save data from any system, but we are focusing on Azure DevOps & Github.
- **[Endpoint Enrichers](../Reference/EndpointEnrichers/index.md)** - Because systems likely have different data shapes we also have *EndpointEnrichers* that can be added to `Endpoints` that allow loading and saving of specific data.
- **[Mapping Tools](../Reference/MappingTools/index.md)** - 

We currently have a `WorkItemTrackingProcessor` with Endpoints for *InMemory* (for testing), *FileSystem*, and *Tfs*. You can mix-and-match Endpoints so that you would be able to migrate your `WorkItem` data from *Tfs* to *FileSystem* as needed.

The model should also work for other data `Teams`, `SharedQueries`, `PlansAndSuits`.

### How the Configuration file flows

```JSON
{
  "Version": "0.0",
  "LogLevel": "Verbose",
  "MappingTools": [
    {
      "$type": "MappingToolA",
      "Enabled": true
    },
    {
      "$type": "MappingToolA",
      "Enabled": true
    }
  ],
  "Processors": [
    {
      "$type": "ProcessorA",
      "Enabled": true,
      "Enrichers": [
        {
          "$type": "EnricherA",
          "Enabled": true
        },
        {
          "$type": "EnricherB",
          "Enabled": true
        }
      ],
      "Endpoints": [
        {
          "$type": "EndPointA",
          "Direction": "Source",
          "Enrichers": [
            {
              "$type": "EnricherA",
              "Enabled": true,
            },
            {
              "$type": "EnricherB",
              "Enabled": true,
            }
          ]
        },
        {
          "$type": "EndPointB",
          "Direction": "Target",
          "Enrichers": [
            {
              "$type": "EnricherA",
              "Enabled": true,
              "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
              "AttachmentMaxSize": 480000000
            }
          ]
        }
      ]
    }
  ]
}
```

### What was added here

- Moved to WorkItemData2 & RevisedItem2 as we needed more changes than the v1 architecture could support
- Enabled the configuration through Options and the loading of the objects for `Processors`, `ProcessorEnrichers`, `Endpoints`, `EndpointEnrichers`. 
- Moved all services setup to the project that holds it using extension methods. e.g. ` services.AddMigrationToolServices();`
- Created new IntegrationTests with logging that can be used to validate autonomously the Processors. Started with `TestTfsToTfsNoEnrichers` to get a migration of just ID, & ReflectedWorkItemId. Still needs actual code in `TfsWorkItemEndpoint` to connect to TFS but it runs, passes, and attaches the log to the test results.

While we still have a long way to go this is a strong move towards v2. It will add object confusion while we build within the context of the existing tool. However, I have marked many of the objects as `[Obsolite("This is v1 *", false)` so that we can distinguish in the confusing areas.

#### Legacy Folders

- `VstsSyncMigrator.Core` - Everything in here must go :)
- `MigrationTools\_EngineV1\*` - These will me refactored away and into v2.
- `MigrationTools.Clients.AzureDevops.ObjectModel\_EngineV1\*` - Clients model is being abandoned in favour of `Endpoints`
