---
optionsClassName: ExportUsersForMappingConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.ExportUsersForMappingConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "ExportUsersForMappingConfig",
      "Enabled": false,
      "Enrichers": null,
      "WIQLQuery": null,
      "OnlyListUsersInWorkItems": true
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.ExportUsersForMappingConfig
description: ExportUsersForMappingContext is a tool used to create a starter mapping file for users between the source and target systems. Use `ExportUsersForMappingConfig` to configure.
className: ExportUsersForMappingContext
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
- parameterName: OnlyListUsersInWorkItems
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WIQLQuery
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Items
classFile: ''
optionsClassFile: ''

redirectFrom:
- /Reference/v1/Processors/ExportUsersForMappingConfig/
layout: reference
toc: true
permalink: /Reference/Processors/ExportUsersForMappingContext/
title: ExportUsersForMappingContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /Processors/ExportUsersForMappingContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/ExportUsersForMappingContext-introduction.md
  exists: false
  markdown: ''

---