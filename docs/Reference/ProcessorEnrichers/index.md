---
title: Processor Enrichers
layout: page
pageType: index
toc: true
pageStatus: published
---


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
