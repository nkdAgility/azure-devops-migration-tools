---
optionsClassName: FieldValueMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldValueMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FieldValueMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.Status",
      "targetField": "System.Status",
      "defaultValue": "New",
      "valueMapping": {
        "$type": "Dictionary`2",
        "New": "New",
        "Active": "Committed",
        "Resolved": "Committed",
        "Closed": "Done"
      }
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldValueMapConfig
description: Need to map not just the field but also values? This is the default value mapper.
className: FieldValueMapConfig
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
- parameterName: valueMapping
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/FieldMaps/FieldValueMapConfig/
title: FieldValueMapConfig
categories:
- FieldMaps
- v1
notes: ''
introduction: ''

---