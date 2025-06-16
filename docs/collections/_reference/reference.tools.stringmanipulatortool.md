---
optionsClassName: StringManipulatorToolOptions
optionsClassFullName: MigrationTools.Tools.StringManipulatorToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.StringManipulatorToolOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.StringManipulatorToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "StringManipulatorToolOptions",
      "Enabled": false,
      "MaxStringLength": 0,
      "Manipulators": null
    }
  sampleFor: MigrationTools.Tools.StringManipulatorToolOptions
description: Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
className: StringManipulatorTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: Manipulators
  type: List
  description: List of regex based string manipulations to apply to all string fields. Each regex replacement is applied in order and can be enabled or disabled.
  defaultValue: '{}'
- parameterName: MaxStringLength
  type: Int32
  description: Max number of chars in a string. Applied last, and set to 1000000 by default.
  defaultValue: 1000000
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools/Tools/StringManipulatorTool.cs
optionsClassFile: src/MigrationTools/Tools/StringManipulatorToolOptions.cs

redirectFrom:
- /Reference/Tools/StringManipulatorToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/StringManipulatorTool/
title: StringManipulatorTool
categories:
- Tools
- 
topics:
- topic: notes
  path: docs/Reference/Tools/StringManipulatorTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/StringManipulatorTool-introduction.md
  exists: false
  markdown: ''

---