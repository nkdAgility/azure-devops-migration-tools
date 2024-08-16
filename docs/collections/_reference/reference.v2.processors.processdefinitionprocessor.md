---
optionsClassName: ProcessDefinitionProcessorOptions
optionsClassFullName: MigrationTools.Processors.ProcessDefinitionProcessorOptions
configurationSamples:
- name: default
  description: 
  code: >-
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
      "Enrichers": null,
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.ProcessDefinitionProcessorOptions
description: Process definition processor used to keep processes between two orgs in sync
className: ProcessDefinitionProcessor
typeName: Processors
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
- parameterName: MaxDegreeOfParallelism
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Processes
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ProcessMaps
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ProcessorEnrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missng XML code comments
- parameterName: SourceName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TargetName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: UpdateProcessDetails
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
status: Beta
processingTarget: Pipelines
classFile: /src/MigrationTools.Clients.AzureDevops.Rest/Processors/ProcessDefinitionProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.Rest/Processors/ProcessDefinitionProcessorOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/Processors/ProcessDefinitionProcessor/
title: ProcessDefinitionProcessor
categories:
- Processors
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/Processors/ProcessDefinitionProcessor-notes.md
  exists: true
  markdown: >2+

    ## Example 



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


    ## Example Full


    ```

    {% include sampleConfig/ProcessDefinitionProcessor-Full.json %}

    ```
- topic: introduction
  path: /docs/Reference/v2/Processors/ProcessDefinitionProcessor-introduction.md
  exists: true
  markdown: >2-

    Source: https://github.com/nkdAgility/azure-devops-migration-tools/pull/918


    I've got a use case where I need to have a single inheritance process model that is standardized across organizations. My proposed solution to this is to build a processor that iterates all the source process definitions the processor has configured to synchronize and update the target process definitions accordingly.


    Below is a sample processor configuration that will synchronize a process model definition on the source called "Custom Agile Process", with a process model definition on the target called "Other Agile Process". It will only synchronize the work item types configured, in the below case, Bug. The synchronize will not destroy any target entities, but will move and update them according to the source. Meaning if the target has it's own custom fields, this sync process will not damage them, unless they are named the same in the source.


    It supports, new fields, updated fields, moved fields, new groups, updated groups, moved groups, new pages, updated pages, moved pages, behaviors and rules.

---