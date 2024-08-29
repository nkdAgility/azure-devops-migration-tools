---
optionsClassName: MultiValueConditionalMapOptions
optionsClassFullName: MigrationTools.Tools.MultiValueConditionalMapOptions
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
                "FieldMapType": "MultiValueConditionalMap",
                "ApplyTo": [
                  "*"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.MultiValueConditionalMapOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "MultiValueConditionalMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "sourceFieldsAndValues": {
                  "Field1": "Value1",
                  "Field2": "Value2"
                },
                "targetFieldsAndValues": {
                  "Field1": "Value1",
                  "Field2": "Value2"
                }
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.MultiValueConditionalMapOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "MultiValueConditionalMapOptions",
      "sourceFieldsAndValues": {
        "Field1": "Value1",
        "Field2": "Value2"
      },
      "targetFieldsAndValues": {
        "Field1": "Value1",
        "Field2": "Value2"
      },
      "ApplyTo": [
        "*",
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.MultiValueConditionalMapOptions
description: missng XML code comments
className: MultiValueConditionalMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: sourceFieldsAndValues
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: targetFieldsAndValues
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/MultiValueConditionalMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/MultiValueConditionalMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/MultiValueConditionalMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/MultiValueConditionalMap/
title: MultiValueConditionalMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/MultiValueConditionalMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/MultiValueConditionalMap-introduction.md
  exists: false
  markdown: ''

---