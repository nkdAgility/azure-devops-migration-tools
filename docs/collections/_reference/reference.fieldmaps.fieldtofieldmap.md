---
optionsClassName: FieldToFieldMapOptions
optionsClassFullName: MigrationTools.Tools.FieldToFieldMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldDefaults": {
              "FieldToFieldMap": {}
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldToFieldMapOptions",
      "WorkItemTypeName": null,
      "sourceField": null,
      "targetField": null,
      "defaultValue": null,
      "Enabled": false,
      "ApplyTo": null
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
description: missng XML code comments
className: FieldToFieldMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: defaultValue
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
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
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldToFieldMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldtoFieldMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldToFieldMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldToFieldMap/
title: FieldToFieldMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /FieldMaps/FieldToFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldToFieldMap-introduction.md
  exists: false
  markdown: ''

---