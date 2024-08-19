---
optionsClassName: FieldtoTagMapOptions
optionsClassFullName: MigrationTools.Tools.FieldtoTagMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldDefaults": {
              "FieldtoTagMap": {}
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldtoTagMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldtoTagMapOptions",
      "WorkItemTypeName": null,
      "sourceField": null,
      "formatExpression": null,
      "Enabled": false,
      "ApplyTo": null
    }
  sampleFor: MigrationTools.Tools.FieldtoTagMapOptions
description: Want to take a field and convert its value to a tag? Done...
className: FieldtoTagMapOptions
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
classFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldtoTagMapOptions.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldtoTagMapOptions.cs

redirectFrom:
- /Reference/v1/FieldMaps/FieldtoTagMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldtoTagMapOptions/
title: FieldtoTagMapOptions
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /FieldMaps/FieldtoTagMapOptions-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldtoTagMapOptions-introduction.md
  exists: false
  markdown: ''

---