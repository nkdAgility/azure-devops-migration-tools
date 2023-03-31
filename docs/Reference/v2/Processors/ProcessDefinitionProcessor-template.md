---
title: <ClassName>
layout: default
template: <template>
pageType: reference
classType: <TypeName>
architecture: <architecture>
toc: true
pageStatus: generated
discussionId: 
---


Source: https://github.com/nkdAgility/azure-devops-migration-tools/pull/918

I've got a use case where I need to have a single inheritance process model that is standardized across organizations. My proposed solution to this is to build a processor that iterates all the source process definitions the processor has configured to synchronize and update the target process definitions accordingly.

Below is a sample processor configuration that will synchronize a process model definition on the source called "Custom Agile Process", with a process model definition on the target called "Other Agile Process". It will only synchronize the work item types configured, in the below case, Bug. The synchronize will not destroy any target entities, but will move and update them according to the source. Meaning if the target has it's own custom fields, this sync process will not damage them, unless they are named the same in the source.

It supports, new fields, updated fields, moved fields, new groups, updated groups, moved groups, new pages, updated pages, moved pages, behaviors and rules.

### Description

<Description>

### Properties

<Options>
{: .table .table-striped .table-bordered .d-none .d-md-block}

### Supported Endpoints

- TfsWorkItemEndpoint

### Supported Processor Enrichers

- 


### Work Item Tracking Processor Options


#### Minimum Options to run

```JSON
<ExampleJson>
```

#### Example 


```JSON
   {
...
    "Processors": [
        {
            "$type": "ProcessDefinitionProcessorOptions",
            "Enabled": true,
            "Processes": {
                "Custom Agile Process": [
                    "Bug"
                ]
            },
            "ProcessMaps": {
                "Custom Agile Process": "Other Agile Process"
            },
            "SourceName": "Source",
            "TargetName": "Target",
            "UpdateProcessDetails": true
        }
    ]
...
}
```

#### Example Full

```
<Import:v2\processors\ProcessDefinitionProcessor-Full.json>
```
