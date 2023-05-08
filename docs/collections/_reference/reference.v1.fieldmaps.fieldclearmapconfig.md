---
optionsClassName: FieldClearMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldClearMapConfig
configurationSamples:
- name: default
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
architecture: v1
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/FieldMaps/FieldClearMapConfig/
title: FieldClearMapConfig
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/FieldMaps/FieldClearMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/FieldMaps/FieldClearMapConfig-introduction.md
  exists: false
  markdown: ''

---