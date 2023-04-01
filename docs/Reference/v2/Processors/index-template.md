---
title: Processors
layout: default
template: <template>
pageType: index
toc: true
pageStatus: generated
discussionId: 
---

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**


We provide a number of Processors that can be used to migrate diferent sorts of data.

<ItemList>

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