---
optionsClassName: FieldMergeMapOptions
optionsClassFullName: MigrationTools.Tools.FieldMergeMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": {
              "FieldMergeMap": []
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldMergeMapOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": {
              "FieldMergeMap": []
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldMergeMapOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "FieldMergeMapOptions",
      "sourceFields": null,
      "targetField": null,
      "formatExpression": null,
      "ConfigurationOptionFor": "FieldMergeMap",
      "ApplyTo": []
    }
  sampleFor: MigrationTools.Tools.FieldMergeMapOptions
description: missng XML code comments
className: FieldMergeMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ConfigurationOptionFor
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: formatExpression
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: sourceFields
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: targetField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldMergeMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldMergeMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldMergeMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldMergeMap/
title: FieldMergeMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/FieldMergeMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldMergeMap-introduction.md
  exists: false
  markdown: ''

---