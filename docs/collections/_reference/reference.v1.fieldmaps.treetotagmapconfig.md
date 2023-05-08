---
optionsClassName: TreeToTagMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.TreeToTagMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TreeToTagMapConfig",
      "WorkItemTypeName": "*",
      "toSkip": 2,
      "timeTravel": 0
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.TreeToTagMapConfig
description: Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...
className: TreeToTagMapConfig
typeName: FieldMaps
architecture: v1
options:
- parameterName: timeTravel
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: toSkip
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/TreeToTagMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/TreeToTagMapConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/FieldMaps/TreeToTagMapConfig/
title: TreeToTagMapConfig
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/FieldMaps/TreeToTagMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/FieldMaps/TreeToTagMapConfig-introduction.md
  exists: false
  markdown: ''

---