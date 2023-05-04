---
optionsClassName: FieldSkipMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldSkipMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FieldSkipMapConfig",
      "WorkItemTypeName": "*",
      "targetField": "System.Description"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldSkipMapConfig
description: Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue.
className: FieldSkipMapConfig
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/FieldMaps/FieldSkipMapConfig/
title: FieldSkipMapConfig
categories:
- FieldMaps
- v1
notes: ''
introduction: ''

---