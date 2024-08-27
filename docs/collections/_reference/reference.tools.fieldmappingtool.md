---
optionsClassName: FieldMappingToolOptions
optionsClassFullName: MigrationTools.Tools.FieldMappingToolOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "Enabled": "False",
            "FieldMapDefaults": null,
            "FieldMaps": null
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldMappingToolOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "Enabled": "True",
            "FieldMaps": [
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "FieldMergeMap",
                "formatExpression": "{0} \n {1}",
                "sourceFields": [
                  "Custom.FieldA",
                  "Custom.FieldB"
                ],
                "targetField": "Custom.FieldC"
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "FieldLiteralMap",
                "targetField": "Custom.SomeField",
                "value": "New field value"
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "MultiValueConditionalMap",
                "sourceFieldsAndValues": {
                  "Field1": "Value1",
                  "Field2": "Value2"
                },
                "targetFieldsAndValues": {
                  "Field1": "Value1",
                  "Field2": "Value2"
                }
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "FieldSkipMap",
                "targetField": "Custom.ReflectedWorkItemId"
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "defaultValue": "StateB",
                "FieldMapType": "FieldValueMap",
                "sourceField": "System.State",
                "targetField": "System.State",
                "valueMapping": {
                  "StateA": "StateB"
                }
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "defaultValue": "42",
                "FieldMapType": "FieldToFieldMap",
                "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
                "targetField": "Microsoft.VSTS.Common.StackRank"
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType",
                  "SomeOtherWorkItemType"
                ],
                "FieldMapType": "FieldToFieldMultiMap",
                "SourceToTargetMappings": {
                  "SourceField1": "TargetField1",
                  "SourceField2": "TargetField2"
                }
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "FieldToTagMap",
                "formatExpression": "ScrumState:{0}",
                "sourceField": "System.State"
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "FieldToTagFieldMap",
                "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}",
                "sourceFields": [
                  "System.Description",
                  "Microsoft.VSTS.Common.AcceptanceCriteria"
                ],
                "targetField": "System.Description"
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "RegexFieldMap",
                "pattern": "PRODUCT \\d{4}.(\\d{1})",
                "replacement": "$1",
                "sourceField": "COMPANY.PRODUCT.Release",
                "targetField": "COMPANY.DEVISION.MinorReleaseVersion"
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "FieldValueToTagMap",
                "formatExpression": "{0}",
                "pattern": "Yes",
                "sourceField": "Microsoft.VSTS.CMMI.Blocked"
              },
              {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "FieldMapType": "TreeToTagMap",
                "timeTravel": "1",
                "toSkip": "3"
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldMappingToolOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "FieldMappingToolOptions",
      "Enabled": true,
      "FieldMaps": []
    }
  sampleFor: MigrationTools.Tools.FieldMappingToolOptions
description: missng XML code comments
className: FieldMappingTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: FieldMaps
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/Tools/FieldMappingTool.cs
optionsClassFile: /src/MigrationTools/Tools/FieldMappingToolOptions.cs

redirectFrom:
- /Reference/Tools/FieldMappingToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/FieldMappingTool/
title: FieldMappingTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/FieldMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/FieldMappingTool-introduction.md
  exists: false
  markdown: ''

---