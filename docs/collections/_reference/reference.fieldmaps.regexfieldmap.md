---
optionsClassName: RegexFieldMapOptions
optionsClassFullName: MigrationTools.Tools.RegexFieldMapOptions
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
  order: 1
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
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
  order: 3
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
description: Applies regular expression transformations to map values from a source field to a target field using pattern matching and replacement.
className: RegexFieldMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: pattern
  type: String
  description: Gets or sets the regular expression pattern to match against the source field value.
  defaultValue: missing XML code comments
- parameterName: replacement
  type: String
  description: Gets or sets the replacement pattern that defines how matched groups should be used to construct the target value.
  defaultValue: missing XML code comments
- parameterName: sourceField
  type: String
  description: Gets or sets the name of the source field to read data from and apply regex pattern matching.
  defaultValue: missing XML code comments
- parameterName: targetField
  type: String
  description: Gets or sets the name of the target field to write the regex-transformed data to.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/RegexFieldMap.cs
optionsClassFile: src/MigrationTools/Tools/FieldMappingTool/FieldMaps/RegexFieldMapOptions.cs

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
  path: ../../docs/Reference/FieldMaps/RegexFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: ../../docs/Reference/FieldMaps/RegexFieldMap-introduction.md
  exists: false
  markdown: ''

---