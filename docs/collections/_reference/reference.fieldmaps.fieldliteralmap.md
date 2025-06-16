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
description: Maps a literal (static) value to a target field, useful for setting constant values across all migrated work items.
className: FieldLiteralMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: A list of Work Item Types that this Field Map will apply to. If the list is empty it will apply to all Work Item Types. You can use "*" to apply to all Work Item Types.
  defaultValue: missing XML code comments
- parameterName: targetField
  type: String
  description: Gets or sets the name of the target field that will be set to the specified literal value.
  defaultValue: missing XML code comments
- parameterName: value
  type: String
  description: Gets or sets the literal value that will be assigned to the target field during migration.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldLiteralMap.cs
optionsClassFile: src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldLiteralMapOptions.cs

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
  path: docs/Reference/FieldMaps/FieldLiteralMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/FieldMaps/FieldLiteralMap-introduction.md
  exists: false
  markdown: ''

---