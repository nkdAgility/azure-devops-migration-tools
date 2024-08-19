---
optionsClassName: FieldLiteralMapOptions
optionsClassFullName: MigrationTools.Tools.FieldLiteralMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldDefaults": {
              "FieldLiteralMap": {}
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldLiteralMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldLiteralMapOptions",
      "WorkItemTypeName": null,
      "targetField": null,
      "value": null,
      "Enabled": false,
      "ApplyTo": null
    }
  sampleFor: MigrationTools.Tools.FieldLiteralMapOptions
description: Sets a field on the `target` to b a specific value.
className: FieldLiteralMapOptions
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
- parameterName: value
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldLiteralMapOptions.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldLiteralMapOptions.cs

redirectFrom:
- /Reference/v1/FieldMaps/FieldLiteralMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldLiteralMapOptions/
title: FieldLiteralMapOptions
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /FieldMaps/FieldLiteralMapOptions-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldLiteralMapOptions-introduction.md
  exists: false
  markdown: ''

---