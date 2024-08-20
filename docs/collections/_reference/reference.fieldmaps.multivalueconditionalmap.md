---
optionsClassName: MultiValueConditionalMapOptions
optionsClassFullName: MigrationTools.Tools.MultiValueConditionalMapOptions
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
                "FieldMapType": "MultiValueConditionalMap",
                "Enabled": false,
                "sourceFieldsAndValues": {
                  "Field1": "Value1",
                  "Field2": "Value2"
                },
                "targetFieldsAndValues": {
                  "Field1": "Value1",
                  "Field2": "Value2"
                },
                "ConfigurationCollectionItemPath": "MigrationTools:CommonTools:FieldMappingTool:FieldMaps:*:MultiValueConditionalMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.MultiValueConditionalMapOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "MultiValueConditionalMap": {
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
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.MultiValueConditionalMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "MultiValueConditionalMapOptions",
      "Enabled": false,
      "sourceFieldsAndValues": {
        "$type": "Dictionary`2",
        "Field1": "Value1",
        "Field2": "Value2"
      },
      "targetFieldsAndValues": {
        "$type": "Dictionary`2",
        "Field1": "Value1",
        "Field2": "Value2"
      },
      "ConfigurationCollectionItemPath": "MigrationTools:CommonTools:FieldMappingTool:FieldMaps:*:MultiValueConditionalMap",
      "ApplyTo": [
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
- parameterName: ConfigurationCollectionItemPath
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
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
  path: /FieldMaps/MultiValueConditionalMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/MultiValueConditionalMap-introduction.md
  exists: false
  markdown: ''

---