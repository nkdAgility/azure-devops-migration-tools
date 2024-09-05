---
optionsClassName: TreeToTagFieldMapOptions
optionsClassFullName: MigrationTools.Tools.TreeToTagFieldMapOptions
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
                "FieldMapType": "TreeToTagFieldMap",
                "ApplyTo": [
                  "*"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TreeToTagFieldMapOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.TreeToTagFieldMapOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TreeToTagFieldMapOptions",
      "toSkip": 0,
      "timeTravel": 0,
      "ApplyTo": [
        "*"
      ]
    }
  sampleFor: MigrationTools.Tools.TreeToTagFieldMapOptions
description: missng XML code comments
className: TreeToTagFieldMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: timeTravel
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: toSkip
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/TreeToTagFieldMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/TreeToTagFieldMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/TreeToTagFieldMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/TreeToTagFieldMap/
title: TreeToTagFieldMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/TreeToTagFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/TreeToTagFieldMap-introduction.md
  exists: false
  markdown: ''

---