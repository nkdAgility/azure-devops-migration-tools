---
optionsClassName: GitRepoMappingToolOptions
optionsClassFullName: MigrationTools.Tools.GitRepoMappingToolOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.GitRepoMappingToolOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.GitRepoMappingToolOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "GitRepoMappingToolOptions",
      "Enabled": false,
      "Mappings": null
    }
  sampleFor: MigrationTools.Tools.GitRepoMappingToolOptions
description: Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
className: GitRepoMappingTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Mappings
  type: Dictionary
  description: List of work item mappings.
  defaultValue: '{}'
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/Tools/GitRepoMappingTool.cs
optionsClassFile: /src/MigrationTools/Tools/GitRepoMappingToolOptions.cs

redirectFrom:
- /Reference/Tools/GitRepoMappingToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/GitRepoMappingTool/
title: GitRepoMappingTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/GitRepoMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/GitRepoMappingTool-introduction.md
  exists: false
  markdown: ''

---