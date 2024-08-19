---
optionsClassName: TfsChangeSetMappingToolOptions
optionsClassFullName: MigrationTools.Tools.TfsChangeSetMappingToolOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorDefaults": {
          "TfsChangeSetMappingTool": {}
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsChangeSetMappingToolOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TfsChangeSetMappingToolOptions",
      "ChangeSetMappingFile": null
    }
  sampleFor: MigrationTools.Tools.TfsChangeSetMappingToolOptions
description: missng XML code comments
className: TfsChangeSetMappingTool
typeName: Tools
architecture: v1
options:
- parameterName: ChangeSetMappingFile
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsChangeSetMappingTool.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsChangeSetMappingToolOptions.cs

redirectFrom:
- /Reference/v1/Tools/TfsChangeSetMappingToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsChangeSetMappingTool/
title: TfsChangeSetMappingTool
categories:
- Tools
- v1
topics:
- topic: notes
  path: /Tools/TfsChangeSetMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Tools/TfsChangeSetMappingTool-introduction.md
  exists: false
  markdown: ''

---