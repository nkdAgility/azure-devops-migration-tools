---
optionsClassName: TfsGitRepositoryToolOptions
optionsClassFullName: MigrationTools.Tools.TfsGitRepositoryToolOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsGitRepositoryTool": []
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonToolsSamples": {
          "TfsGitRepositoryTool": []
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsGitRepositoryToolOptions",
      "Enabled": false
    }
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
description: missng XML code comments
className: TfsGitRepositoryTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsGitRepositoryTool.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsGitRepositoryToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsGitRepositoryToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsGitRepositoryTool/
title: TfsGitRepositoryTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/TfsGitRepositoryTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/TfsGitRepositoryTool-introduction.md
  exists: false
  markdown: ''

---