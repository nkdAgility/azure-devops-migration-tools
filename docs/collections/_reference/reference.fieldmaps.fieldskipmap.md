---
optionsClassName: FieldSkipMapOptions
optionsClassFullName: MigrationTools.Tools.FieldSkipMapOptions
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
                "FieldMapType": "FieldSkipMap",
                "ApplyTo": [
                  "*"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldSkipMapOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.FieldSkipMapOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "FieldSkipMapOptions",
      "targetField": null,
      "ApplyTo": [
        "*"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldSkipMapOptions
description: Skips field mapping for a specific target field, effectively leaving the field unchanged during migration.
className: FieldSkipMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: A list of Work Item Types that this Field Map will apply to. If the list is empty it will apply to all Work Item Types. You can use "*" to apply to all Work Item Types.
  defaultValue: missing XML code comments
- parameterName: targetField
  type: String
  description: Gets or sets the name of the target field that should be skipped during migration, resetting it to its original value.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldSkipMap.cs
optionsClassFile: src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldSkipMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldSkipMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldSkipMap/
title: FieldSkipMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: docs/Reference/FieldMaps/FieldSkipMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/FieldMaps/FieldSkipMap-introduction.md
  exists: false
  markdown: ''

---