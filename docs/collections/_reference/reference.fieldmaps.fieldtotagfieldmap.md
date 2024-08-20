---
optionsClassName: FieldToTagFieldMapOptions
optionsClassFullName: MigrationTools.Tools.FieldToTagFieldMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldToTagFieldMap": []
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToTagFieldMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldToTagFieldMapOptions",
      "Enabled": false,
      "WorkItemTypeName": null,
      "sourceField": null,
      "formatExpression": null,
      "ApplyTo": null
    }
  sampleFor: MigrationTools.Tools.FieldToTagFieldMapOptions
description: missng XML code comments
className: FieldToTagFieldMap
typeName: FieldMaps
architecture: 
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
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldToTagFieldMap.cs
optionsClassFile: ''

redirectFrom:
- /Reference/FieldMaps/FieldToTagFieldMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldToTagFieldMap/
title: FieldToTagFieldMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /FieldMaps/FieldToTagFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldToTagFieldMap-introduction.md
  exists: false
  markdown: ''

---