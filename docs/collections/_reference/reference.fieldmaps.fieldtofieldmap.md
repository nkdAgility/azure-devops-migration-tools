---
optionsClassName: FieldToFieldMapOptions
optionsClassFullName: MigrationTools.Tools.FieldToFieldMapOptions
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
                "FieldMapType": "FieldToFieldMap",
                "Enabled": false,
                "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
                "targetField": "Microsoft.VSTS.Common.StackRank",
                "defaultValue": "42",
                "ConfigurationCollectionItemPath": "MigrationTools:CommonTools:FieldMappingTool:FieldMaps:*:FieldToFieldMap",
                "ApplyTo": [
                  "SomeWorkItemType"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldToFieldMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "defaultValue": "42",
                "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
                "targetField": "Microsoft.VSTS.Common.StackRank"
              }
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldToFieldMapOptions",
      "Enabled": false,
      "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
      "targetField": "Microsoft.VSTS.Common.StackRank",
      "defaultValue": "42",
      "ConfigurationCollectionItemPath": "MigrationTools:CommonTools:FieldMappingTool:FieldMaps:*:FieldToFieldMap",
      "ApplyTo": [
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
description: missng XML code comments
className: FieldToFieldMap
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
- parameterName: defaultValue
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
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
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldToFieldMap.cs
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
  path: /FieldMaps/FieldToFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldToFieldMap-introduction.md
  exists: false
  markdown: ''

---