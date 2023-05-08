---
optionsClassName: FieldtoTagMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldtoTagMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FieldtoTagMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "Custom.ProjectName",
      "formatExpression": "Project: {0}"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldtoTagMapConfig
description: Want to take a field and convert its value to a tag? Done...
className: FieldtoTagMapConfig
typeName: FieldMaps
architecture: v2
options:
- parameterName: formatExpression
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
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldtoTagMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/FieldtoTagMapConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/FieldMaps/FieldtoTagMapConfig/
title: FieldtoTagMapConfig
categories:
- FieldMaps
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/FieldMaps/FieldtoTagMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/FieldMaps/FieldtoTagMapConfig-introduction.md
  exists: false
  markdown: ''

---