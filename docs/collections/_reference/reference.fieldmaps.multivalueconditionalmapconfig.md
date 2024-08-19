---
optionsClassName: MultiValueConditionalMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.MultiValueConditionalMapConfig
configurationSamples:
- name: Classic
  description: 
  code: >-
    {
      "$type": "MultiValueConditionalMapConfig",
      "WorkItemTypeName": "*",
      "sourceFieldsAndValues": {
        "$type": "Dictionary`2",
        "Something": "SomethingElse"
      },
      "targetFieldsAndValues": {
        "$type": "Dictionary`2",
        "Something": "SomethingElse"
      }
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.MultiValueConditionalMapConfig
description: ??? If you know how to use this please send a PR :)
className: MultiValueConditionalMapConfig
typeName: FieldMaps
architecture: v2
options:
- parameterName: sourceFieldsAndValues
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: targetFieldsAndValues
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/MultiValueConditionalMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/MultiValueConditionalMapConfig.cs

redirectFrom:
- /Reference/v2/FieldMaps/MultiValueConditionalMapConfig/
layout: reference
toc: true
permalink: /Reference/FieldMaps/MultiValueConditionalMapConfig/
title: MultiValueConditionalMapConfig
categories:
- FieldMaps
- v2
topics:
- topic: notes
  path: /FieldMaps/MultiValueConditionalMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/MultiValueConditionalMapConfig-introduction.md
  exists: false
  markdown: ''

---