---
optionsClassName: TfsValidateRequiredFieldToolOptions
optionsClassFullName: MigrationTools.Tools.TfsValidateRequiredFieldToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.TfsValidateRequiredFieldToolOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.TfsValidateRequiredFieldToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsValidateRequiredFieldToolOptions",
      "Enabled": false
    }
  sampleFor: MigrationTools.Tools.TfsValidateRequiredFieldToolOptions
description: Tool for validating that required fields exist in target work item types before migration, preventing migration failures due to missing required fields.
className: TfsValidateRequiredFieldTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsValidateRequiredFieldTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsValidateRequiredFieldToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsValidateRequiredFieldToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsValidateRequiredFieldTool/
title: TfsValidateRequiredFieldTool
categories:
- Tools
- 
topics:
- topic: notes
  path: docs/Reference/Tools/TfsValidateRequiredFieldTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsValidateRequiredFieldTool-introduction.md
  exists: false
  markdown: ''

---