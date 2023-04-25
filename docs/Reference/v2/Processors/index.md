---
title: Processors
layout: default
template: index-template.md
pageType: index
toc: true
pageStatus: generated
discussionId: 
---

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**


We provide a number of Processors that can be used to migrate diferent sorts of data.

| Processors | Status | Target    | Usage                              |
|------------------------|---------|---------|------------------------------------------|
| [AzureDevOpsPipelineProcessor](AzureDevOpsPipelineProcessor.md) | Beta | Pipelines | Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines. |
| [ProcessDefinitionProcessor](ProcessDefinitionProcessor.md) | Beta | Pipelines | Process definition processor used to keep processes between two orgs in sync |
| [TfsAreaAndIterationProcessor](TfsAreaAndIterationProcessor.md) | Beta | Work Items | The `TfsAreaAndIterationProcessor` migrates all of the Area nd Iteraion paths. |
| [TfsSharedQueryProcessor](TfsSharedQueryProcessor.md) | Beta | Queries | The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another. |
| [TfsTeamSettingsProcessor](TfsTeamSettingsProcessor.md) | Beta | Teams | Native TFS Processor, does not work with any other Endpoints. |
| [WorkItemTrackingProcessor](WorkItemTrackingProcessor.md) | missng XML code comments | missng XML code comments | This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md). |


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