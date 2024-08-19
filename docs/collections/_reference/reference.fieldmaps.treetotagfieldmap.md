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
            "FieldDefaults": {
              "TreeToTagFieldMap": {}
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TreeToTagFieldMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TreeToTagFieldMapOptions",
      "WorkItemTypeName": null,
      "toSkip": 0,
      "timeTravel": 0,
      "Enabled": false,
      "ApplyTo": null
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
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: timeTravel
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: toSkip
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItemTypeName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/TreeToTagFieldMap.cs
optionsClassFile: ''

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
  path: /FieldMaps/TreeToTagFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/TreeToTagFieldMap-introduction.md
  exists: false
  markdown: ''

---