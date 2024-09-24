---
optionsClassName: WorkItemTypeMappingToolOptions
optionsClassFullName: MigrationTools.Tools.WorkItemTypeMappingToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "WorkItemTypeMappingTool": {
            "Enabled": "False",
            "Mappings": {
              "Source Work Item Type Name": "Target Work Item Type Name"
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.WorkItemTypeMappingToolOptions
- name: sample
  order: 1
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "WorkItemTypeMappingTool": {
            "Enabled": "True",
            "Mappings": {
              "User Story": "Product Backlog Item"
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.WorkItemTypeMappingToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "WorkItemTypeMappingToolOptions",
      "Enabled": true,
      "Mappings": {
        "Source Work Item Type Name": "Target Work Item Type Name",
        "User Story": "Product Backlog Item"
      }
    }
  sampleFor: MigrationTools.Tools.WorkItemTypeMappingToolOptions
description: Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
className: WorkItemTypeMappingTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: Mappings
  type: Dictionary
  description: List of work item mappings.
  defaultValue: '{}'
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools/Tools/WorkItemTypeMappingTool.cs
optionsClassFile: /src/MigrationTools/Tools/WorkItemTypeMappingToolOptions.cs

redirectFrom:
- /Reference/Tools/WorkItemTypeMappingToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/WorkItemTypeMappingTool/
title: WorkItemTypeMappingTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/WorkItemTypeMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/WorkItemTypeMappingTool-introduction.md
  exists: false
  markdown: ''

---