---
optionsClassName: StringManipulatorToolOptions
optionsClassFullName: MigrationTools.Tools.StringManipulatorToolOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "StringManipulatorTool": {
            "Enabled": "True",
            "Manipulators": [
              {
                "$type": "RegexStringManipulator",
                "Description": "Remove invalid characters from the end of the string",
                "Enabled": "True",
                "Pattern": "[^( -~)\n\r\t]+",
                "Replacement": ""
              }
            ],
            "MaxStringLength": "1000000"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.StringManipulatorToolOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "StringManipulatorTool": {
            "Enabled": "True",
            "Manipulators": [
              {
                "$type": "RegexStringManipulator",
                "Description": "Remove invalid characters from the end of the string",
                "Enabled": "True",
                "Pattern": "[^( -~)\n\r\t]+",
                "Replacement": ""
              }
            ],
            "MaxStringLength": "1000000"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.StringManipulatorToolOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "StringManipulatorToolOptions",
      "Enabled": true,
      "MaxStringLength": 1000000,
      "Manipulators": [
        {
          "Enabled": true,
          "Pattern": "[^( -~)\n\r\t]+",
          "Replacement": "",
          "Description": "Remove invalid characters from the end of the string"
        },
        {
          "Enabled": true,
          "Pattern": "[^( -~)\n\r\t]+",
          "Replacement": "",
          "Description": "Remove invalid characters from the end of the string"
        }
      ]
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
  path: /docs/Reference/Tools/StringManipulatorTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/StringManipulatorTool-introduction.md
  exists: false
  markdown: ''

---