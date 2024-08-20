---
optionsClassName: FieldLiteralMapOptions
optionsClassFullName: MigrationTools.Tools.FieldLiteralMapOptions
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
                "FieldMapType": "FieldLiteralMap",
                "Enabled": false,
                "targetField": null,
                "value": null,
                "ConfigurationCollectionItemPath": "MigrationTools:CommonTools:FieldMappingTool:FieldMaps:*:FieldLiteralMap",
                "ApplyTo": null
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldLiteralMapOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldLiteralMap": []
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
      "Enabled": false,
      "targetField": null,
      "value": null,
      "ConfigurationCollectionItemPath": "MigrationTools:CommonTools:FieldMappingTool:FieldMaps:*:FieldLiteralMap",
      "ApplyTo": null
    }
  sampleFor: MigrationTools.Tools.FieldLiteralMapOptions
description: missng XML code comments
className: FieldLiteralMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ConfigurationCollectionItemPath
  type: String
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
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldLiteralMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldLiteralMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldLiteralMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldLiteralMap/
title: FieldLiteralMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /FieldMaps/FieldLiteralMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldLiteralMap-introduction.md
  exists: false
  markdown: ''

---