---
optionsClassName: FieldMergeMapOptions
optionsClassFullName: MigrationTools.Tools.FieldMergeMapOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "FieldMapDefaults": {
              "FieldMergeMap": {
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
  sampleFor: MigrationTools.Tools.FieldMergeMapOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldMergeMapOptions",
      "Enabled": false,
      "sourceFields": [
        "System.Description",
        "Microsoft.VSTS.Common.AcceptanceCriteria"
      ],
      "targetField": "System.Description",
      "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}",
      "ApplyTo": [
        "SomeWorkItemType"
      ]
    }
  sampleFor: MigrationTools.Tools.FieldMergeMapOptions
description: missng XML code comments
className: FieldMergeMap
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
- parameterName: formatExpression
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: sourceFields
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: targetField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/FieldMappingTool/FieldMaps/FieldMergeMap.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldMergeMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldMergeMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldMergeMap/
title: FieldMergeMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: /FieldMaps/FieldMergeMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /FieldMaps/FieldMergeMap-introduction.md
  exists: false
  markdown: ''

---