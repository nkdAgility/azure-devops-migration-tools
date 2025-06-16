---
optionsClassName: ProcessDefinitionProcessorOptions
optionsClassFullName: MigrationTools.Processors.ProcessDefinitionProcessorOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.ProcessDefinitionProcessorOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.ProcessDefinitionProcessorOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "ProcessDefinitionProcessorOptions",
      "Enabled": false,
      "Processes": null,
      "ProcessMaps": null,
      "UpdateProcessDetails": false,
      "MaxDegreeOfParallelism": 0,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.ProcessDefinitionProcessorOptions
description: Process definition processor used to keep processes between two orgs in sync
className: ProcessDefinitionProcessor
typeName: Processors
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: MaxDegreeOfParallelism
  type: Int32
  description: Maximum number of parallel operations to execute simultaneously during process definition migration to optimize performance.
  defaultValue: missing XML code comments
- parameterName: Processes
  type: Dictionary
  description: Dictionary mapping process names to lists of work item type names to be included in the migration. If null, all work item types will be migrated.
  defaultValue: missing XML code comments
- parameterName: ProcessMaps
  type: Dictionary
  description: Dictionary mapping source process names to target process names for process template transformations during migration.
  defaultValue: missing XML code comments
- parameterName: SourceName
  type: String
  description: This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor.
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a read only processor.
  defaultValue: missing XML code comments
- parameterName: UpdateProcessDetails
  type: Boolean
  description: Indicates whether to update existing process details in the target organization or only create new processes.
  defaultValue: missing XML code comments
status: Beta
processingTarget: Pipelines
classFile: src/MigrationTools.Clients.AzureDevops.Rest/Processors/ProcessDefinitionProcessor.cs
optionsClassFile: src/MigrationTools.Clients.AzureDevops.Rest/Processors/ProcessDefinitionProcessorOptions.cs

redirectFrom:
- /Reference/Processors/ProcessDefinitionProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/ProcessDefinitionProcessor/
title: ProcessDefinitionProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: docs/Reference/Processors/ProcessDefinitionProcessor-notes.md
  exists: true
  markdown: >2

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
  path: docs/Reference/Processors/ProcessDefinitionProcessor-introduction.md
  exists: true
  markdown: >2-

    Source: https://github.com/nkdAgility/azure-devops-migration-tools/pull/918


    I've got a use case where I need to have a single inheritance process model that is standardized across organizations. My proposed solution to this is to build a processor that iterates all the source process definitions the processor has configured to synchronize and update the target process definitions accordingly.


    Below is a sample processor configuration that will synchronize a process model definition on the source called "Custom Agile Process", with a process model definition on the target called "Other Agile Process". It will only synchronize the work item types configured, in the below case, Bug. The synchronize will not destroy any target entities, but will move and update them according to the source. Meaning if the target has it's own custom fields, this sync process will not damage them, unless they are named the same in the source.


    It supports, new fields, updated fields, moved fields, new groups, updated groups, moved groups, new pages, updated pages, moved pages, behaviors and rules.

---