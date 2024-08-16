---
optionsClassName: GitRepoMappingToolOptions
optionsClassFullName: MigrationTools.Enrichers.GitRepoMappingToolOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "GitRepoMappingToolOptions",
      "Enabled": true,
      "Mappings": {
        "$type": "Dictionary`2",
        "Default": "Default2"
      }
    }
  sampleFor: MigrationTools.Enrichers.GitRepoMappingToolOptions
description: Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
className: GitRepoMappingTool
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: Mappings
  type: Dictionary
  description: List of work item mappings.
  defaultValue: '{}'
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/ProcessorEnrichers/WorkItemProcessorEnrichers/GitRepoMappingTool.cs
optionsClassFile: /src/MigrationTools/ProcessorEnrichers/WorkItemProcessorEnrichers/GitRepoMappingToolOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/GitRepoMappingTool/
title: GitRepoMappingTool
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/GitRepoMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/GitRepoMappingTool-introduction.md
  exists: false
  markdown: ''

---