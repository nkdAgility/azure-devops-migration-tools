---
optionsClassName: FieldClearMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldClearMapConfig
configurationSamples:
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldClearMapConfig",
      "WorkItemTypeName": "*",
      "targetField": "System.Description"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldClearMapConfig
description: Allows you to set an already populated field to Null. This will only work with fields that support null.
className: FieldClearMapConfig
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
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldClearMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldClearMapConfig.cs

redirectFrom:
- /Reference/v2/FieldMaps/FieldClearMapConfig/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldClearMapConfig/
title: FieldClearMapConfig
categories:
- FieldMaps
- v2
topics:
- topic: notes
  path: /FieldMaps/FieldClearMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldClearMapConfig-introduction.md
  exists: false
  markdown: ''

---