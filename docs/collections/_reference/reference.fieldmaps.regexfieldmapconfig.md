---
optionsClassName: RegexFieldMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.RegexFieldMapConfig
configurationSamples:
- name: Classic
  description: 
  code: >-
    {
      "$type": "RegexFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "Custom.MyVersion",
      "targetField": "Custom.MyVersionYearOnly",
      "pattern": "([0-9]{4})",
      "replacement": "$1"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.RegexFieldMapConfig
description: I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done.
className: RegexFieldMapConfig
typeName: FieldMaps
architecture: v2
options:
- parameterName: pattern
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: replacement
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
classFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/RegexFieldMapConfig.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/FieldMap/RegexFieldMapConfig.cs

redirectFrom:
- /Reference/v2/FieldMaps/RegexFieldMapConfig/
layout: reference
toc: true
permalink: /Reference/FieldMaps/RegexFieldMapConfig/
title: RegexFieldMapConfig
categories:
- FieldMaps
- v2
topics:
- topic: notes
  path: /FieldMaps/RegexFieldMapConfig-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/RegexFieldMapConfig-introduction.md
  exists: false
  markdown: ''

---