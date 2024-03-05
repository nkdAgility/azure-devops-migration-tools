---
optionsClassName: FilterWorkItemsThatAlreadyExistInTargetOptions
optionsClassFullName: MigrationTools.Enrichers.FilterWorkItemsThatAlreadyExistInTargetOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FilterWorkItemsThatAlreadyExistInTargetOptions",
      "Enabled": true,
      "Query": {
        "$type": "QueryOptions",
        "Query": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc",
        "Parameters": null
      }
    }
  sampleFor: MigrationTools.Enrichers.FilterWorkItemsThatAlreadyExistInTargetOptions
description: missng XML code comments
className: FilterWorkItemsThatAlreadyExistInTarget
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: Query
  type: QueryOptions
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/ProcessorEnrichers/WorkItemProcessorEnrichers/FilterWorkItemsThatAlreadyExistInTarget.cs
optionsClassFile: /src/MigrationTools/ProcessorEnrichers/WorkItemProcessorEnrichers/FilterWorkItemsThatAlreadyExistInTargetOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/FilterWorkItemsThatAlreadyExistInTarget/
title: FilterWorkItemsThatAlreadyExistInTarget
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/FilterWorkItemsThatAlreadyExistInTarget-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/FilterWorkItemsThatAlreadyExistInTarget-introduction.md
  exists: false
  markdown: ''

---