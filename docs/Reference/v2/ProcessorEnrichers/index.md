---
title: Processor Enrichers
layout: page
template: index-template.md
pageType: index
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**


[Overview](.././index.md) > [Reference](../index.md) > [Processors](../Processors/index.md) > *Processor Enrichers*

Processor Enrichers are run within the context of the ProcessorProcessor that they are 
configured for. Many Processor Enrichers are flexable, however there are also enrichers that only 
work with certain Processors.

Processor Enricher | Processor(s) | Description
----------|-----------|------------
PauseAfterEachItem | Any | TBA
AppendMigrationToolSignatureFooter | WorkItem | TBA
FilterWorkItemsThatAlreadyExistInTarget | WorkItem | TBA
SkipToFinalRevisedWorkItemType | WorkItem | TBA

### Processor Pipline

  - ProcessorExecutionBegin
  - ProcessorExecutionAfterSource
  - ProcessorExecutionBeforeProcessWorkItem
  - ProcessorExecutionAfterProcessWorkItem
  - ProcessorExecutionEnd

### Processor Enricher Options

 All Processor Enrichers have a minimum set of options that are required to run. 

#### Minimum Options to run

The `Enabled` options is common to all Processor Enrichers.


```JSON
    {
      "ObjectType": "ProcessorEnrichersOptions",
      "Enabled": true,
    }
```
