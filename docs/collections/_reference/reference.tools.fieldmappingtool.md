---
optionsClassName: FieldMappingToolOptions
optionsClassFullName: MigrationTools.Tools.FieldMappingToolOptions
configurationSamples:
- name: confinguration.json
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "Enabled": false,
            "FieldMaps": []
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldMappingToolOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "FieldMappingTool": {
            "Enabled": "False",
            "FieldMapDefaults": {
              "FieldSkipMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "targetField": "Custom.ReflectedWorkItemId"
              },
              "FieldToFieldMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "defaultValue": "42",
                "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
                "targetField": "Microsoft.VSTS.Common.StackRank"
              },
              "FieldToFieldMultiMap": {
                "ApplyTo": [
                  "SomeWorkItemType",
                  "SomeOtherWorkItemType"
                ],
                "SourceToTargetMappings": {
                  "SourceField1": "TargetField1",
                  "SourceField2": "TargetField2"
                }
              },
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
              },
              "FieldToTagMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "formatExpression": "ScrumState:{0}",
                "sourceField": "System.State"
              },
              "FieldValueMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "defaultValue": "StateB",
                "sourceField": "System.State",
                "targetField": "System.State",
                "valueMapping": {
                  "StateA": "StateB"
                }
              },
              "FieldValueToTagMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "formatExpression": "{0}",
                "pattern": "Yes",
                "sourceField": "Microsoft.VSTS.CMMI.Blocked"
              },
              "MultiValueConditionalMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "sourceFieldsAndValues": {
                  "Field1": "Value1",
                  "Field2": "Value2"
                },
                "targetFieldsAndValues": {
                  "Field1": "Value1",
                  "Field2": "Value2"
                }
              },
              "RegexFieldMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "pattern": "PRODUCT \\d{4}.(\\d{1})",
                "replacement": "$1",
                "sourceField": "COMPANY.PRODUCT.Release",
                "targetField": "COMPANY.DEVISION.MinorReleaseVersion"
              },
              "TreeToTagMap": {
                "ApplyTo": [
                  "SomeWorkItemType"
                ],
                "timeTravel": "1",
                "toSkip": "3"
              }
            },
            "FieldMaps": null
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.FieldMappingToolOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "FieldMappingToolOptions",
      "Enabled": false,
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