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
            "FieldMapDefaults": {
              "RegexFieldMap": {
                "pattern": "PRODUCT \\d{4}.(\\d{1})",
                "replacement": "$1",
                "sourceField": "COMPANY.PRODUCT.Release",
                "targetField": "COMPANY.DEVISION.MinorReleaseVersion",
                "WorkItemTypes": {
                  "0": "SomeWorkItemType"
                }
              }
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.RegexFieldMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "RegexFieldMapOptions",
      "WorkItemTypeName": null,
      "sourceField": "COMPANY.PRODUCT.Release",
      "targetField": "COMPANY.DEVISION.MinorReleaseVersion",
      "pattern": "PRODUCT \\d{4}.(\\d{1})",
      "replacement": "$1",
      "Enabled": false,
      "ApplyTo": null
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
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
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
- parameterName: WorkItemTypeName
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
  path: /FieldMaps/RegexFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/RegexFieldMap-introduction.md
  exists: false
  markdown: ''

---