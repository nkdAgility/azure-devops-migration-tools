---
optionsClassName: FieldToFieldMultiMapOptions
optionsClassFullName: MigrationTools.Tools.FieldToFieldMultiMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldToFieldMultiMap": {
                "ApplyTo": [
                  "SomeWorkItemType",
                  "SomeOtherWorkItemType"
                ],
                "SourceToTargetMappings": {
                  "SourceField1": "TargetField1",
                  "SourceField2": "TargetField2"
                }
              }
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMultiMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldToFieldMultiMapOptions",
      "Enabled": false,
      "WorkItemTypeName": null,
      "SourceToTargetMappings": {
        "$type": "Dictionary`2",
        "SourceField1": "TargetField1",
        "SourceField2": "TargetField2"
      },
      "ApplyTo": [
        "SomeWorkItemType",
        "SomeOtherWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMultiMapOptions
description: missng XML code comments
className: FieldToFieldMultiMap
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
- parameterName: SourceToTargetMappings
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldtoFieldMultiMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldtoFieldMultiMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldToFieldMultiMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldToFieldMultiMap/
title: FieldToFieldMultiMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /FieldMaps/FieldToFieldMultiMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldToFieldMultiMap-introduction.md
  exists: false
  markdown: ''

---