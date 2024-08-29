---
optionsClassName: RegexFieldMapOptions
optionsClassFullName: MigrationTools.Tools.RegexFieldMapOptions
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
                "FieldMapType": "RegexFieldMap",
                "ApplyTo": [
                  "*"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.RegexFieldMapOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "RegexFieldMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "pattern": "PRODUCT \\d{4}.(\\d{1})",
                "replacement": "$1",
                "sourceField": "COMPANY.PRODUCT.Release",
                "targetField": "COMPANY.DEVISION.MinorReleaseVersion"
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.RegexFieldMapOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "RegexFieldMapOptions",
      "sourceField": "COMPANY.PRODUCT.Release",
      "targetField": "COMPANY.DEVISION.MinorReleaseVersion",
      "pattern": "PRODUCT \\d{4}.(\\d{1})",
      "replacement": "$1",
      "ApplyTo": [
        "*",
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.RegexFieldMapOptions
description: missng XML code comments
className: RegexFieldMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: pattern
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: replacement
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
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/RegexFieldMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/RegexFieldMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/RegexFieldMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/RegexFieldMap/
title: RegexFieldMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/RegexFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/RegexFieldMap-introduction.md
  exists: false
  markdown: ''

---