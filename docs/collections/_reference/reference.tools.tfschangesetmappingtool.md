---
optionsClassName: TfsChangeSetMappingToolOptions
optionsClassFullName: MigrationTools.Tools.TfsChangeSetMappingToolOptions
configurationSamples:
- name: defaults
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
  description: 
  code: >-
    {
      "$type": "TfsChangeSetMappingToolOptions",
      "Enabled": true,
      "ChangeSetMappingFile": null
    }
  sampleFor: MigrationTools.Tools.TfsChangeSetMappingToolOptions
description: missng XML code comments
className: TfsChangeSetMappingTool
typeName: Tools
architecture: 
options:
- parameterName: ChangeSetMappingFile
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
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