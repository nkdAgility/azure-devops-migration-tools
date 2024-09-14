---
optionsClassName: FieldToFieldMapOptions
optionsClassFullName: MigrationTools.Tools.FieldToFieldMapOptions
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
                "FieldMapType": "FieldToFieldMap",
                "ApplyTo": [
                  "*"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
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
                "FieldMapType": "FieldToFieldMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "defaultValue": "42",
                "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
                "targetField": "Microsoft.VSTS.Common.StackRank"
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "FieldToFieldMapOptions",
      "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
      "targetField": "Microsoft.VSTS.Common.StackRank",
      "defaultValue": "42",
      "ApplyTo": [
        "*",
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
description: missing XML code comments
className: FieldToFieldMap
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
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldToFieldMap.cs
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
  path: /docs/Reference/FieldMaps/FieldToFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldToFieldMap-introduction.md
  exists: false
  markdown: ''

---