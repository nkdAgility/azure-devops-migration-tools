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
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Query
  type: QueryOptions
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/FilterWorkItemsThatAlreadyExistInTarget/
title: FilterWorkItemsThatAlreadyExistInTarget
categories:
- ProcessorEnrichers
- v2
notes: ''
introduction: ''

---