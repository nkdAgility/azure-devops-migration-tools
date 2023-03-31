---
title: ProcessDefinitionProcessor
layout: default
template: ProcessDefinitionProcessor-template.md
pageType: reference
classType: Processors
architecture: v2
toc: true
pageStatus: generated
discussionId: 
---


Source: https://github.com/nkdAgility/azure-devops-migration-tools/pull/918

I've got a use case where I need to have a single inheritance process model that is standardized across organizations. My proposed solution to this is to build a processor that iterates all the source process definitions the processor has configured to synchronize and update the target process definitions accordingly.

Below is a sample processor configuration that will synchronize a process model definition on the source called "Custom Agile Process", with a process model definition on the target called "Other Agile Process". It will only synchronize the work item types configured, in the below case, Bug. The synchronize will not destroy any target entities, but will move and update them according to the source. Meaning if the target has it's own custom fields, this sync process will not damage them, unless they are named the same in the source.

It supports, new fields, updated fields, moved fields, new groups, updated groups, moved groups, new pages, updated pages, moved pages, behaviors and rules.

### Description

Process definition processor used to keep processes between two orgs in sync

### Properties

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| MaxDegreeOfParallelism | Int32 | missng XML code comments | missng XML code comments |
| Processes | Dictionary | missng XML code comments | missng XML code comments |
| ProcessMaps | Dictionary | missng XML code comments | missng XML code comments |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |
| SourceName | String | missng XML code comments | missng XML code comments |
| TargetName | String | missng XML code comments | missng XML code comments |
| UpdateProcessDetails | Boolean | missng XML code comments | missng XML code comments |


### Supported Endpoints

- TfsWorkItemEndpoint

### Supported Processor Enrichers

- 


### Work Item Tracking Processor Options


#### Minimum Options to run

```JSON
{
  "$type": "ProcessDefinitionProcessorOptions",
  "Enabled": false,
  "Processes": {
    "$type": "Dictionary`2",
    "*": [
      "*"
    ]
  },
  "ProcessMaps": {
    "$type": "Dictionary`2"
  },
  "UpdateProcessDetails": true,
  "MaxDegreeOfParallelism": 1,
  "ProcessorEnrichers": null,
  "SourceName": null,
  "TargetName": null
}
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
{
  "Version": "12.8",
  "ChangeSetMappingFile": null,
  "Source": null,
  "Target": null,
  "FieldMaps": [],
  "GitRepoMapping": null,
  "LogLevel": "Debug",
  "CommonEnrichersConfig": null,
  "workaroundForQuerySOAPBugEnabled": false,
  "WorkItemTypeDefinition": null,
  "Endpoints": {
    "AzureDevOpsEndpoints": [
      {
        "name": "Source",
        "$type": "AzureDevOpsEndpointOptions",
        "Organisation": "https://dev.azure.com/xxxx",
        "Project": "SAM",
        "AuthenticationMode": "AccessToken",
        "AccessToken": "xxxxx",
        "EndpointEnrichers": null
      },
      {
        "Name": "Target",
        "$type": "AzureDevOpsEndpointOptions",
        "Organisation": "https://dev.azure.com/xxxx",
        "Project": "SAM",
        "AuthenticationMode": "AccessToken",
        "AccessToken": "xxxx",
        "EndpointEnrichers": null
      }
    ]
  },
  "Processors": [
    {
      "$type": "ProcessDefinitionProcessorOptions",
      "Enabled": true,
      "Processes": {
        "Agile SAM - Migration Template": [
          "User Story"
        ]
      },
      "ProcessMaps": {
        "Agile SAM - Migration Template": "SAM Agile Migrator"
      },
      "SourceName": "Source",
      "TargetName": "Target",
      "UpdateProcessDetails": true
    }
  ]
}
```
