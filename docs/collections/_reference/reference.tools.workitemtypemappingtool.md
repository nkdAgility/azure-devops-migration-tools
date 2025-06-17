---
optionsClassName: WorkItemTypeMappingToolOptions
optionsClassFullName: MigrationTools.Tools.WorkItemTypeMappingToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.WorkItemTypeMappingToolOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.WorkItemTypeMappingToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "WorkItemTypeMappingToolOptions",
      "Enabled": false,
      "Mappings": null
    }
  sampleFor: MigrationTools.Tools.WorkItemTypeMappingToolOptions
description: Provides mapping functionality for transforming work item types from source to target systems during migration, allowing different work item type names to be used in the target.
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
classFile: src/MigrationTools/Tools/WorkItemTypeMappingTool.cs
optionsClassFile: src/MigrationTools/Tools/WorkItemTypeMappingToolOptions.cs
notes:
  exists: false
  path: docs/Reference/Tools/WorkItemTypeMappingTool-notes.md
  markdown: ''
topics:
- topic: notes
  path: docs/Reference/Tools/WorkItemTypeMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/WorkItemTypeMappingTool-introduction.md
  exists: false
  markdown: ''

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
  path: docs/Reference/Tools/WorkItemTypeMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/WorkItemTypeMappingTool-introduction.md
  exists: false
  markdown: ''

---