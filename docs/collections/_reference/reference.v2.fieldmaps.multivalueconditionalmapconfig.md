---
optionsClassName: MultiValueConditionalMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.MultiValueConditionalMapConfig
configurationSamples:
- name: default
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/FieldMaps/MultiValueConditionalMapConfig/
title: MultiValueConditionalMapConfig
categories:
- FieldMaps
- v2
notes: ''
introduction: ''

---