---
optionsClassName: TfsChangeSetMappingToolOptions
optionsClassFullName: MigrationTools.Tools.TfsChangeSetMappingToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsChangeSetMappingTool": {
            "Enabled": "False",
            "File": ""
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsChangeSetMappingToolOptions
- name: sample
  order: 1
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsChangeSetMappingTool": {
            "Enabled": "True",
            "File": "c:\\changesetmappings.json"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsChangeSetMappingToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsChangeSetMappingToolOptions",
      "Enabled": true,
      "ChangeSetMappingFile": null
    }
  sampleFor: MigrationTools.Tools.TfsChangeSetMappingToolOptions
description: missing XML code comments
className: TfsChangeSetMappingTool
typeName: Tools
architecture: 
options:
- parameterName: ChangeSetMappingFile
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/TfsChangeSetMappingTool.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/TfsChangeSetMappingToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsChangeSetMappingToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsChangeSetMappingTool/
title: TfsChangeSetMappingTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/TfsChangeSetMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/TfsChangeSetMappingTool-introduction.md
  exists: false
  markdown: ''

---