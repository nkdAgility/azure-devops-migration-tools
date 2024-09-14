---
optionsClassName: FieldSkipMapOptions
optionsClassFullName: MigrationTools.Tools.FieldSkipMapOptions
configurationSamples:
- name: defaults
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
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.FieldSkipMapOptions
- name: classic
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
description: missing XML code comments
className: FieldSkipMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: targetField
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldSkipMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldSkipMapOptions.cs

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
  path: /docs/Reference/FieldMaps/FieldSkipMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldSkipMap-introduction.md
  exists: false
  markdown: ''

---