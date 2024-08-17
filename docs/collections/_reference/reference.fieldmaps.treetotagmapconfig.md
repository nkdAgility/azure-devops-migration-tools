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
architecture: v2
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

redirectFrom:
- /Reference/v2/FieldMaps/TreeToTagMapConfig/
layout: reference
toc: true
permalink: /Reference/FieldMaps/TreeToTagMapConfig/
title: TreeToTagMapConfig
categories:
- FieldMaps
- v2
topics:
- topic: notes
  path: /FieldMaps/TreeToTagMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/TreeToTagMapConfig-introduction.md
  exists: false
  markdown: ''

---