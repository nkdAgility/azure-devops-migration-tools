---
optionsClassName: FieldClearMapOptions
optionsClassFullName: MigrationTools.Tools.FieldClearMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldDefaults": {
              "FieldClearMap": {}
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldClearMapOptions",
      "WorkItemTypeName": null,
      "targetField": null,
      "Enabled": false,
      "ApplyTo": null
    }
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
description: Allows you to set an already populated field to Null. This will only work with fields that support null.
className: FieldClearMapOptions
typeName: FieldMaps
architecture: v1
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
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
processingTarget: Work Item
classFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldClearMapOptions.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldClearMapOptions.cs

redirectFrom:
- /Reference/v1/FieldMaps/FieldClearMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldClearMapOptions/
title: FieldClearMapOptions
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /FieldMaps/FieldClearMapOptions-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldClearMapOptions-introduction.md
  exists: false
  markdown: ''

---