---
optionsClassName: FieldMergeMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.FieldMergeMapConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FieldMergeMapConfig",
      "WorkItemTypeName": "*",
      "sourceFields": [
        "System.Description",
        "System.Status"
      ],
      "targetField": "System.Description",
      "formatExpression": "{0} \n {1}"
    }
  sampleFor: MigrationTools._EngineV1.Configuration.FieldMap.FieldMergeMapConfig
description: Ever wanted to merge two or three fields? This mapping will let you do just that.
className: FieldMergeMapConfig
typeName: FieldMaps
architecture: v2
options:
- parameterName: formatExpression
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: sourceFields
  type: List
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/FieldMaps/FieldMergeMapConfig/
title: FieldMergeMapConfig
categories:
- FieldMaps
- v2
notes: ''
introduction: ''

---