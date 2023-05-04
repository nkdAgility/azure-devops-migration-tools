---
optionsClassName: RegexFieldMapConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.FieldMap.RegexFieldMapConfig
configurationSamples:
- name: default
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/FieldMaps/RegexFieldMapConfig/
title: RegexFieldMapConfig
categories:
- FieldMaps
- v2
notes: ''
introduction: ''

---