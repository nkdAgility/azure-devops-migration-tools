---
title: Processors
layout: page
pageType: index
toc: true
pageStatus: published
discussionId:
---

We provide a number of Processors that can be used to migrate different sorts of data.

| Processors                                                      | Status                    | Target                    | Usage                                                                                                                                                                                  |
| --------------------------------------------------------------- | ------------------------- | ------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [AzureDevOpsPipelineProcessor](AzureDevOpsPipelineProcessor.md) | Beta                      | Pipelines                 | Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines.                                                                                                         |
| [ProcessDefinitionProcessor](ProcessDefinitionProcessor.md)     | Beta                      | Pipelines                 | Process definition processor used to keep processes between two orgs in sync                                                                                                           |
| [TfsAreaAndIterationProcessor](TfsAreaAndIterationProcessor.md) | Beta                      | Work Items                | The `TfsAreaAndIterationProcessor` migrates all of the Area nd Iteraion paths.                                                                                                         |
| [TfsSharedQueryProcessor](TfsSharedQueryProcessor.md)           | Beta                      | Queries                   | The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another.                                                                                               |
| [TfsTeamSettingsProcessor](TfsTeamSettingsProcessor.md)         | Beta                      | Teams                     | Native TFS Processor, does not work with any other Endpoints.                                                                                                                          |
| [WorkItemTrackingProcessor](WorkItemTrackingProcessor.md)       | missing XML code comments | missing XML code comments | This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md). |

### Processor Options

All processors have a minimum set of options that are required to run.

#### Minimum Options to run

The `Enabled` options is common to all processors.

```JSON
    {
      "ObjectType": "ProcessorOptions",
      "Enabled": true,
    }
```
