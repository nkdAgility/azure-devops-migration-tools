---
optionsClassName: FieldClearMapOptions
optionsClassFullName: MigrationTools.Tools.FieldClearMapOptions
configurationSamples:
- name: confinguration.json
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "FieldClearMap",
                "targetField": null,
                "ApplyTo": []
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldClearMap": []
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
      "targetField": null,
      "ApplyTo": []
    }
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
description: missng XML code comments
className: FieldClearMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: targetField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldClearMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldClearMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldClearMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldClearMap/
title: FieldClearMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/FieldClearMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldClearMap-introduction.md
  exists: false
  markdown: ''

---