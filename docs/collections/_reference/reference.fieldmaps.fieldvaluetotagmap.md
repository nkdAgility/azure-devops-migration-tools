---
optionsClassName: FieldValuetoTagMapOptions
optionsClassFullName: MigrationTools.Tools.FieldValuetoTagMapOptions
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
                "FieldMapType": "FieldValuetoTagMap",
                "sourceField": "Microsoft.VSTS.CMMI.Blocked",
                "pattern": "Yes",
                "formatExpression": "{0}",
                "ApplyTo": [
                  "SomeWorkItemType"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldValuetoTagMapOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldValuetoTagMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "formatExpression": "{0}",
                "pattern": "Yes",
                "sourceField": "Microsoft.VSTS.CMMI.Blocked"
              }
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldValuetoTagMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldValuetoTagMapOptions",
      "sourceField": "Microsoft.VSTS.CMMI.Blocked",
      "pattern": "Yes",
      "formatExpression": "{0}",
      "ApplyTo": [
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldValuetoTagMapOptions
description: missng XML code comments
className: FieldValuetoTagMap
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
- parameterName: pattern
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: sourceField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldValuetoTagMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldValuetoTagMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldValuetoTagMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldValuetoTagMap/
title: FieldValuetoTagMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/FieldValuetoTagMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldValuetoTagMap-introduction.md
  exists: false
  markdown: ''

---