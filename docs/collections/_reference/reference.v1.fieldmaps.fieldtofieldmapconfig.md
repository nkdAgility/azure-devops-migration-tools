---
optionsClassName: FieldtoFieldMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldtoFieldMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FieldtoFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.StackRank",
      "targetField": "System.Rank",
      "defaultValue": "1000"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldtoFieldMapConfig
description: Just want to map one field to another? This is the one for you.
className: FieldtoFieldMapConfig
typeName: FieldMaps
architecture: v1
options:
- parameterName: defaultValue
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: sourceField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: targetField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldtoFieldMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldtoFieldMapConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/FieldMaps/FieldtoFieldMapConfig/
title: FieldtoFieldMapConfig
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/FieldMaps/FieldtoFieldMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/FieldMaps/FieldtoFieldMapConfig-introduction.md
  exists: false
  markdown: ''

---