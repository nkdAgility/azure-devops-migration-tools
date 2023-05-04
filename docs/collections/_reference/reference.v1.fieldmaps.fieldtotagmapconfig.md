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
architecture: v1
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference2/v1/FieldMaps/FieldtoTagMapConfig/
title: FieldtoTagMapConfig
categories:
- FieldMaps
- v1

---