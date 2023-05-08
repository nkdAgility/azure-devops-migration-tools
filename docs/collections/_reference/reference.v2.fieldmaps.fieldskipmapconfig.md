---
optionsClassName: FieldSkipMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldSkipMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FieldSkipMapConfig",
      "WorkItemTypeName": "*",
      "targetField": "System.Description"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldSkipMapConfig
description: Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue.
className: FieldSkipMapConfig
typeName: FieldMaps
architecture: v2
options:
- parameterName: targetField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Item
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldSkipMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldSkipMapConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/FieldMaps/FieldSkipMapConfig/
title: FieldSkipMapConfig
categories:
- FieldMaps
- v2
topics:
- topic: notes
  path: ../../../../../docs/Reference/v2/FieldMaps/FieldSkipMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: ../../../../../docs/Reference/v2/FieldMaps/FieldSkipMapConfig-introduction.md
  exists: false
  markdown: ''

---