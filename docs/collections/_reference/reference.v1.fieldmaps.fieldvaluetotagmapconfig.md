---
optionsClassName: FieldValuetoTagMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldValuetoTagMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FieldValuetoTagMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.Status",
      "pattern": "(Active|Resolved)",
      "formatExpression": "Status: {0}"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldValuetoTagMapConfig
description: Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target.
className: FieldValuetoTagMapConfig
typeName: FieldMaps
architecture: v1
options:
- parameterName: formatExpression
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: pattern
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: sourceField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldValuetoTagMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldValuetoTagMapConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/FieldMaps/FieldValuetoTagMapConfig/
title: FieldValuetoTagMapConfig
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/FieldMaps/FieldValuetoTagMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/FieldMaps/FieldValuetoTagMapConfig-introduction.md
  exists: false
  markdown: ''

---