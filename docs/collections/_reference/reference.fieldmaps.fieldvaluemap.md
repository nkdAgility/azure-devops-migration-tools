---
optionsClassName: FieldValueMapOptions
optionsClassFullName: MigrationTools.Tools.FieldValueMapOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
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
  order: 1
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
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
  order: 3
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
description: missing XML code comments
className: FieldValueMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: defaultValue
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: sourceField
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: targetField
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: valueMapping
  type: Dictionary
  description: missing XML code comments
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldValueMap.cs
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