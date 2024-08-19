---
optionsClassName: StringManipulatorToolOptions
optionsClassFullName: MigrationTools.Tools.StringManipulatorToolOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorDefaults": {
          "StringManipulatorTool": {}
        }
      }
    }
  sampleFor: MigrationTools.Tools.StringManipulatorToolOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "StringManipulatorToolOptions",
      "MaxStringLength": 0,
      "Manipulators": null
    }
  sampleFor: MigrationTools.Tools.StringManipulatorToolOptions
description: Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
className: StringManipulatorTool
typeName: Tools
architecture: 
options:
- parameterName: Manipulators
  type: List
  description: List of regex based string manipulations to apply to all string fields. Each regex replacement is applied in order and can be enabled or disabled.
  defaultValue: '{}'
- parameterName: MaxStringLength
  type: Int32
  description: Max number of chars in a string. Applied last, and set to 1000000 by default.
  defaultValue: 1000000
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/Tools/StringManipulatorTool.cs
optionsClassFile: /src/MigrationTools/Tools/StringManipulatorToolOptions.cs

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
  path: /Tools/StringManipulatorTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Tools/StringManipulatorTool-introduction.md
  exists: false
  markdown: ''

---