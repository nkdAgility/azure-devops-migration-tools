---
optionsClassName: FieldMergeMapOptions
optionsClassFullName: MigrationTools.Tools.FieldMergeMapOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.FieldMergeMapOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "FieldMergeMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "formatExpression": "{0} \n {1}",
                "sourceFields": [
                  "Custom.FieldA",
                  "Custom.FieldB"
                ],
                "targetField": "Custom.FieldC"
              }
            ]
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
      "sourceFields": [
        "Custom.FieldA",
        "Custom.FieldB"
      ],
      "targetField": "Custom.FieldC",
      "formatExpression": "{0} \n {1}",
      "ApplyTo": [
        "SomeWorkItemType"
      ]
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