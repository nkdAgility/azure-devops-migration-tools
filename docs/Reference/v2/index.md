---
title: v2 Reference Overview
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
---


The system works by setting one or more [Processors](../v2/Processors/index.md) in the json 
configuration file. This processor can have many [ProcessorEnrichers](../v2/ProcessorEnrichers/index.md) that 
enable additional features, and must have at least two [Endpoints](../v2/Endpoints/index.md); 
a *Source* `Endpoint` and a *Target* `Endpoint`. Each `Endpoint` 
may have additional [EndpointEnrichers](../v2/EndpointEnrichers/index.md) that add 
additional *Client* specific functionality.

### What types of things do we have

- **[Processors](../v2/Processors/index.md)** - Processors allow you to move different types of data between `Endpoints` and does not care what `Endpoint` you have on each end.
- **[Processor Enrichers](../v2/ProcessorEnrichers/index.md)** - Enrichers at the processor level allow you to add additional functionality to a processor without endangering the core functionality. Each Enricher should have a single responsibility and can add functionality to the following stages of the processor pipeline.
- **[Endpoints](../v2/Endpoints/index.md)** connect to the target system and load and save the data. Endpoint can load or save data from any system, but we are focusing on Azure DevOps & Github.
- **[Endpoint Enrichers](../v2/EndpointEnrichers/index.md)** - Because systems likely have different data shapes we also have *EndpointEnrichers* that can be added to `Endpoints` that allow loading and saving of specific data.
- **[Mapping Tools](../v2/MappingTools/index.md)** - 

We currently have a `WorkItemTrackingProcessor` with Endpoints for *InMemory* (for testing), *FileSystem*, and *Tfs*. You can mix-and-match Endpoints so that you would be able to migrate your `WorkItem` data from *Tfs* to *FileSystem* as needed.

The model should also work for other data `Teams`, `SharedQueries`, `PlansAndSuits`.

### How the Configuration file flows

This config is for reference only. It has things configured that you will not need, and that may conflict with each other.

{% highlight JSON %}
{% include sampleConfig/configuration-Fullv2.json %}
{% endhighlight %}


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
