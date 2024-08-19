---
optionsClassName: FieldLiteralMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldLiteralMapConfig
configurationSamples:
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldLiteralMapConfig",
      "WorkItemTypeName": "*",
      "targetField": "System.Status",
      "value": "New"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldLiteralMapConfig
description: Sets a field on the `target` to b a specific value.
className: FieldLiteralMapConfig
typeName: FieldMaps
architecture: v2
options:
- parameterName: targetField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: value
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldLiteralMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldLiteralMapConfig.cs

redirectFrom:
- /Reference/v2/FieldMaps/FieldLiteralMapConfig/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldLiteralMapConfig/
title: FieldLiteralMapConfig
categories:
- FieldMaps
- v2
topics:
- topic: notes
  path: /FieldMaps/FieldLiteralMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldLiteralMapConfig-introduction.md
  exists: false
  markdown: ''

---