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
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/ExportUsersForMappingConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/ExportUsersForMappingContext/
title: ExportUsersForMappingContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/ExportUsersForMappingContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/Processors/ExportUsersForMappingContext-introduction.md
  exists: false
  markdown: ''

---