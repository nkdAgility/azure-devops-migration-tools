---
optionsClassName: FieldToFieldMultiMapOptions
optionsClassFullName: MigrationTools.Tools.FieldToFieldMultiMapOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.FieldToFieldMultiMapOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMaps": [
              {
                "FieldMapType": "FieldToFieldMultiMap",
                "ApplyTo": [
                  "SomeWorkItemType",
                  "SomeOtherWorkItemType"
                ],
                "SourceToTargetMappings": {
                  "SourceField1": "TargetField1",
                  "SourceField2": "TargetField2"
                }
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMultiMapOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "FieldToFieldMultiMapOptions",
      "SourceToTargetMappings": {
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
- parameterName: SourceToTargetMappings
  type: Dictionary
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
  path: /docs/Reference/FieldMaps/FieldToFieldMultiMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldToFieldMultiMap-introduction.md
  exists: false
  markdown: ''

---