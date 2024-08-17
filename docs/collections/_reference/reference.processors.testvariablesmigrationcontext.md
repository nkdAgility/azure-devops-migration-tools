---
optionsClassName: TestVariablesMigrationConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TestVariablesMigrationConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TestVariablesMigrationConfig",
      "Enabled": false,
      "Enrichers": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TestVariablesMigrationConfig
description: This processor can migrate test variables that are defined in the test plans / suites. This must run before `TestPlansAndSuitesMigrationConfig`.
className: TestVariablesMigrationContext
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
status: Beta
processingTarget: Suites & Plans
classFile: ''
optionsClassFile: ''

redirectFrom:
- /Reference/v1/Processors/TestVariablesMigrationConfig/
layout: reference
toc: true
permalink: /Reference/Processors/TestVariablesMigrationContext/
title: TestVariablesMigrationContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /Processors/TestVariablesMigrationContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/TestVariablesMigrationContext-introduction.md
  exists: false
  markdown: ''

---