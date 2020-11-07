## How does the system fit together?

The system works by setting one or more `Processors` in the json configuration file. This `Processor` can have many `ProcessorEnrichers` that enable aditional features, and must have at least a *Source* `Endpoint` and a *Target* `Endpoint`. Each `Endpoint` may have additional `EndpointEnrichers` that add additional *Client* specific functionality.

- Processors
  - ProcessorA
    - ProcessprEnricher(s)
      - ProcessprEnricherA
      - ProcessprEnricherA
    - SourceEndpoint
      - ProcessprEnricher(s)
        - ProcessprEnricherA
        - ProcessprEnricherA
    - TargetEndpoint
      - ProcessprEnricher(s)
        - ProcessprEnricherA
        - ProcessprEnricherA

### What types of things do we have

- *Processors* - Processors allow you to move diferent types of data between `Endpoints` and does not care what `Endpoint` you have on each end.
- *ProcessorEnrichers* - Enrichers at the processor level allow you to add additional functionality to a processor without endangering the core functionality. Each Enricher should have a single responsabiity and can add funtionality to the following stages of the processor pipeline.
  - ProcessorExecutionBegin
  - ProcessorExecutionAfterSource
  - ProcessorExecutionBeforeProcessWorkItem
  - ProcessorExecutionAfterProcessWorkItem
  - ProcessorExecutionEnd
- *Endpoints* connect to the target system and load and save the data. Endpoint can load or save data from any system, but we are focusing on Azure DevOps & Github.
- *EndpointEnrichers* - Because systems likely have different data shapes we also have *EndpointEnrichers* that can be added to `Endpoints` that allow loading and saving of specific data.

We currently have a `WorkItemTrackingProcessor` with Endpoints for *InMemory* (for testing), *FileSystem*, and *Tfs*. You can mix-and-match Endpoints so that you would be able to migrate your `WorkItem` data from *Tfs* to *FileSystem* as needed.

The model should also work for other data `Teams`, `SharedQueries`, `PlansAndSuits`.

### What was added here

- Moved to WorkItemData2 & RevisedItem2 as we needed more changes than the v1 architecture could support
- Enabled the configuration through Options and the loading of the objects for `Processors`, `ProcessorEnrichers`, `Endpoints`, `EndpointEnrichers`. 
- Moved all services setup to the project that holds it using extension methods. e.g. ` services.AddMigrationToolServices();`
- Created new IntegrationTests with logging that can be used to validate autonomously the Processors. Started with `TestTfsToTfsNoEnrichers` to get a migration of just ID, & ReflectedWorkItemId. Still needs actual code in `TfsWorkItemEndpoint` to connect to TFS but it runs, passes, and attaches the log to the test results.

While we still have a long way to go this is a strong move towards v2. It will add object confusion while we build within the context of the existing tool. However, I have marked many of the objects as `[Obsolite("This is v1 *", false)` so that we can distinguish in the confusing areas.

#### Legacy Folders

- `VstsSyncMigrator.Core` - Everything in here must go :)
- `MigrationTools\Engine\` - These will me refactored away and into v2.
- `MigrationTools\Clients\` - Clients model is being abandoned in favour of `Endpoints`
- `MigrationTools.Clients.AzureDevops.ObjectModel\Clients\` - Clients model is being abandoned in favour of `Endpoints`
