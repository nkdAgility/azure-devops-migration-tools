---
optionsClassName: FieldSkipMapOptions
optionsClassFullName: MigrationTools.Tools.FieldSkipMapOptions
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
                "FieldMapType": "FieldSkipMap",
                "targetField": "Custom.ReflectedWorkItemId",
                "ApplyTo": [
                  "SomeWorkItemType"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldSkipMapOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldSkipMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "targetField": "Custom.ReflectedWorkItemId"
              }
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldSkipMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldSkipMapOptions",
      "targetField": "Custom.ReflectedWorkItemId",
      "ApplyTo": [
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldSkipMapOptions
description: missng XML code comments
className: FieldSkipMap
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
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldSkipMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldSkipMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldSkipMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldSkipMap/
title: FieldSkipMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/FieldSkipMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldSkipMap-introduction.md
  exists: false
  markdown: ''

---