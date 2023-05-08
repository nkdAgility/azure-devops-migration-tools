---
title: Processors
layout: page
pageType: index
toc: true
pageStatus: published
discussionId: 
---

We provide a number of Processors that can be used to migrate diferent sorts of data. These processors are the original traditional processors.

{% include content-collection-table.html collection = "reference" typeName = "Processors" architecture = "v1" %}


### Processor Options

 All processors have a minimum set of options that are required to run. 

#### Minimum Options to run
The `Enabled` options is common to all processors.


```JSON
     {
      "$type": "ProcessorConfig",
      "Enabled": true,
    }
```