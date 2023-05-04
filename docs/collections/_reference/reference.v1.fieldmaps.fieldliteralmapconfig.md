---
optionsClassName: FieldLiteralMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldLiteralMapConfig
configurationSamples:
- name: default
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
architecture: v1
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference2/v1/FieldMaps/FieldLiteralMapConfig/
title: FieldLiteralMapConfig
categories:
- FieldMaps
- v1

---