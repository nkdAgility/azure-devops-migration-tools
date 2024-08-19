---
optionsClassName: FieldValuetoTagMapOptions
optionsClassFullName: MigrationTools.Tools.FieldValuetoTagMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldDefaults": {
              "FieldValuetoTagMap": {}
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldValuetoTagMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldValuetoTagMapOptions",
      "WorkItemTypeName": null,
      "sourceField": null,
      "pattern": null,
      "formatExpression": null,
      "Enabled": false,
      "ApplyTo": null
    }
  sampleFor: MigrationTools.Tools.FieldValuetoTagMapOptions
description: Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target.
className: FieldValuetoTagMapOptions
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
- parameterName: formatExpression
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: pattern
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
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldValuetoTagMapOptions.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldValuetoTagMapOptions.cs

redirectFrom:
- /Reference/v1/FieldMaps/FieldValuetoTagMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldValuetoTagMapOptions/
title: FieldValuetoTagMapOptions
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /FieldMaps/FieldValuetoTagMapOptions-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldValuetoTagMapOptions-introduction.md
  exists: false
  markdown: ''

---