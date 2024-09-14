---
optionsClassName: FieldClearMapOptions
optionsClassFullName: MigrationTools.Tools.FieldClearMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "FieldClearMap",
                "ApplyTo": [
                  "*"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "FieldClearMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "targetField": "Custom.FieldC"
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "FieldClearMapOptions",
      "targetField": "Custom.FieldC",
      "ApplyTo": [
        "*",
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
description: missing XML code comments
className: FieldClearMap
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
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldClearMap.cs
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