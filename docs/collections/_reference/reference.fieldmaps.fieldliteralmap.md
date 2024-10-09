---
optionsClassName: FieldLiteralMapOptions
optionsClassFullName: MigrationTools.Tools.FieldLiteralMapOptions
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
                "FieldMapType": "FieldLiteralMap",
                "ApplyTo": [
                  "*"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldLiteralMapOptions
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
                "FieldMapType": "FieldLiteralMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "targetField": "Custom.SomeField",
                "value": "New field value"
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldLiteralMapOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "FieldLiteralMapOptions",
      "targetField": "Custom.SomeField",
      "value": "New field value",
      "ApplyTo": [
        "*",
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldLiteralMapOptions
description: missing XML code comments
className: FieldLiteralMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: targetField
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: value
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldLiteralMap.cs
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
  path: /docs/Reference/FieldMaps/FieldLiteralMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldLiteralMap-introduction.md
  exists: false
  markdown: ''

---