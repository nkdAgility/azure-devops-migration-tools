---
optionsClassName: TreeToTagFieldMapOptions
optionsClassFullName: MigrationTools.Tools.TreeToTagFieldMapOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
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
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.TreeToTagFieldMapOptions
- name: classic
  order: 3
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
description: Maps work item area path or iteration path hierarchies to tags, allowing tree structures to be represented as flat tag collections.
className: TreeToTagFieldMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: A list of Work Item Types that this Field Map will apply to. If the list is empty it will apply to all Work Item Types. You can use "*" to apply to all Work Item Types.
  defaultValue: missing XML code comments
- parameterName: timeTravel
  type: Int32
  description: Gets or sets the number of months to travel back in time when looking up historical area path values. Use 0 for current values.
  defaultValue: missing XML code comments
- parameterName: toSkip
  type: Int32
  description: Gets or sets the number of levels to skip from the root when converting area path hierarchy to tags. For example, if set to 2, "ProjectName\Level1\Level2\Level3" would skip "ProjectName\Level1" and start from "Level2".
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/TreeToTagFieldMap.cs
optionsClassFile: src/MigrationTools/Tools/FieldMappingTool/FieldMaps/TreeToTagFieldMapOptions.cs
notes:
  exists: false
  path: docs/Reference/FieldMaps/TreeToTagFieldMap-notes.md
  markdown: ''

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
  path: docs/Reference/FieldMaps/TreeToTagFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/FieldMaps/TreeToTagFieldMap-introduction.md
  exists: false
  markdown: ''

---