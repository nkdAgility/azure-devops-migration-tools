---
optionsClassName: FieldToTagFieldMapOptions
optionsClassFullName: MigrationTools.Tools.FieldToTagFieldMapOptions
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
                "FieldMapType": "FieldToTagFieldMap",
                "sourceField": null,
                "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}",
                "ApplyTo": [
                  "SomeWorkItemType"
                ]
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToTagFieldMapOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldToTagFieldMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}",
                "sourceFields": [
                  "System.Description",
                  "Microsoft.VSTS.Common.AcceptanceCriteria"
                ],
                "targetField": "System.Description"
              }
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldToTagFieldMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldToTagFieldMapOptions",
      "sourceField": null,
      "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}",
      "ApplyTo": [
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldToTagFieldMapOptions
description: missng XML code comments
className: FieldToTagFieldMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: formatExpression
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: sourceField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldToTagFieldMap.cs
optionsClassFile: ''

redirectFrom:
- /Reference/FieldMaps/FieldToTagFieldMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldToTagFieldMap/
title: FieldToTagFieldMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /docs/Reference/FieldMaps/FieldToTagFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/FieldMaps/FieldToTagFieldMap-introduction.md
  exists: false
  markdown: ''

---