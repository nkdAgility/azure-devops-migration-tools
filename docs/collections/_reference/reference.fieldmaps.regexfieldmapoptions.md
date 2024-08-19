---
optionsClassName: RegexFieldMapOptions
optionsClassFullName: MigrationTools.Tools.RegexFieldMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldDefaults": {
              "RegexFieldMap": {}
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.RegexFieldMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "RegexFieldMapOptions",
      "WorkItemTypeName": null,
      "sourceField": null,
      "targetField": null,
      "pattern": null,
      "replacement": null,
      "Enabled": false,
      "ApplyTo": null
    }
  sampleFor: MigrationTools.Tools.RegexFieldMapOptions
description: I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done.
className: RegexFieldMapOptions
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
status: ready
processingTarget: Work Item Field
classFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/RegexFieldMapOptions.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/RegexFieldMapOptions.cs

redirectFrom:
- /Reference/v1/FieldMaps/RegexFieldMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/RegexFieldMapOptions/
title: RegexFieldMapOptions
categories:
- FieldMaps
- v1
topics:
- topic: notes
  path: /FieldMaps/RegexFieldMapOptions-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/RegexFieldMapOptions-introduction.md
  exists: false
  markdown: ''

---