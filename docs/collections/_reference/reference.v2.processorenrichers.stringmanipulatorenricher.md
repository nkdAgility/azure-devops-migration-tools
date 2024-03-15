---
optionsClassName: StringManipulatorEnricherOptions
optionsClassFullName: MigrationTools.Enrichers.StringManipulatorEnricherOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "StringManipulatorEnricherOptions",
      "Enabled": true,
      "MaxStringLength": 1000000,
      "Manipulators": [
        {
          "$type": "RegexStringManipulator",
          "Enabled": false,
          "Pattern": "[^( -~)\\n\\r\\t]+",
          "Replacement": "",
          "Description": "Remove all non-ASKI characters between ^ and ~."
        }
      ]
    }
  sampleFor: MigrationTools.Enrichers.StringManipulatorEnricherOptions
description: Used to process the String fields of a work item. This is useful for cleaning up data. It will limit fields to a max length and apply regex replacements based on what is configured. Each regex replacement is applied in order and can be enabled or disabled.
className: StringManipulatorEnricher
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: Manipulators
  type: List
  description: List of regex based string manipulations to apply to all string fields. Each regex replacement is applied in order and can be enabled or disabled.
  defaultValue: '{}'
- parameterName: MaxStringLength
  type: Int32
  description: Max number of chars in a string. Applied last, and set to 1000000 by default.
  defaultValue: 1000000
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/ProcessorEnrichers/WorkItemProcessorEnrichers/StringManipulatorEnricher.cs
optionsClassFile: /src/MigrationTools/ProcessorEnrichers/WorkItemProcessorEnrichers/StringManipulatorEnricherOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/StringManipulatorEnricher/
title: StringManipulatorEnricher
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/StringManipulatorEnricher-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/StringManipulatorEnricher-introduction.md
  exists: false
  markdown: ''

---