---
optionsClassName: FieldValueMapOptions
optionsClassFullName: MigrationTools.Tools.FieldValueMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "FieldValueMap",
                "ApplyTo": [
                  "*"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldValueMapOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "FieldValueMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "defaultValue": "StateB",
                "sourceField": "System.State",
                "targetField": "System.State",
                "valueMapping": {
                  "StateA": "StateB"
                }
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldValueMapOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "FieldValueMapOptions",
      "sourceField": "System.State",
      "targetField": "System.State",
      "defaultValue": "StateB",
      "valueMapping": {
        "StateA": "StateB"
      },
      "ApplyTo": [
        "*",
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldValueMapOptions
description: missng XML code comments
className: FieldValueMap
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
- parameterName: sourceField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: targetField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: valueMapping
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldValueMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldValueMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldValueMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldValueMap/
title: FieldValueMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/FieldValueMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldValueMap-introduction.md
  exists: false
  markdown: ''

---