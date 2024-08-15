---
optionsClassName: WorkItemTypeMappingEnricherOptions
optionsClassFullName: MigrationTools.Enrichers.WorkItemTypeMappingEnricherOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "WorkItemTypeMappingEnricherOptions",
      "Enabled": true,
      "WorkItemTypeDefinition": {
        "$type": "Dictionary`2",
        "Default": "Default2"
      }
    }
  sampleFor: MigrationTools.Enrichers.WorkItemTypeMappingEnricherOptions
description: Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
className: WorkItemTypeMappingEnricher
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeDefinition
  type: Dictionary
  description: List of work item mappings.
  defaultValue: '{}'
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/ProcessorEnrichers/WorkItemProcessorEnrichers/WorkItemTypeMappingEnricher.cs
optionsClassFile: /src/MigrationTools/ProcessorEnrichers/WorkItemProcessorEnrichers/WorkItemTypeMappingEnricherOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/WorkItemTypeMappingEnricher/
title: WorkItemTypeMappingEnricher
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/WorkItemTypeMappingEnricher-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/WorkItemTypeMappingEnricher-introduction.md
  exists: false
  markdown: ''

---