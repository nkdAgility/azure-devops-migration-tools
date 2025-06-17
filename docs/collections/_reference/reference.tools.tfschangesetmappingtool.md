---
optionsClassName: TfsChangeSetMappingToolOptions
optionsClassFullName: MigrationTools.Tools.TfsChangeSetMappingToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.TfsChangeSetMappingToolOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.TfsChangeSetMappingToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsChangeSetMappingToolOptions",
      "Enabled": false,
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
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsChangeSetMappingTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsChangeSetMappingToolOptions.cs
notes:
  exists: false
  path: docs/Reference/Tools/TfsChangeSetMappingTool-notes.md
  markdown: ''
topics:
- topic: notes
  path: docs/Reference/Tools/TfsChangeSetMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsChangeSetMappingTool-introduction.md
  exists: false
  markdown: ''

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
  path: docs/Reference/Tools/TfsChangeSetMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsChangeSetMappingTool-introduction.md
  exists: false
  markdown: ''

---