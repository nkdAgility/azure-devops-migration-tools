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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/FieldMaps/FieldClearMapConfig/
title: FieldClearMapConfig
categories:
- FieldMaps
- v2
notes: ''
introduction: ''

---