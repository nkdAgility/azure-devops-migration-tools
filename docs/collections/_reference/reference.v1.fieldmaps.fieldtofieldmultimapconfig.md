---
optionsClassName: FieldtoFieldMultiMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldtoFieldMultiMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FieldtoFieldMultiMapConfig",
      "WorkItemTypeName": "*",
      "SourceToTargetMappings": {
        "$type": "Dictionary`2",
        "Custom.Field1": "Custom.Field4",
        "Custom.Field2": "Custom.Field5",
        "Custom.Field3": "Custom.Field6"
      }
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldtoFieldMultiMapConfig
description: Want to setup a bunch of field maps in a single go. Use this shortcut!
className: FieldtoFieldMultiMapConfig
typeName: FieldMaps
architecture: v1
options:
- parameterName: SourceToTargetMappings
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldtoFieldMultiMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldtoFieldMultiMapConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/FieldMaps/FieldtoFieldMultiMapConfig/
title: FieldtoFieldMultiMapConfig
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: ../../../../../docs/Reference/v1/FieldMaps/FieldtoFieldMultiMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: ../../../../../docs/Reference/v1/FieldMaps/FieldtoFieldMultiMapConfig-introduction.md
  exists: false
  markdown: ''

---