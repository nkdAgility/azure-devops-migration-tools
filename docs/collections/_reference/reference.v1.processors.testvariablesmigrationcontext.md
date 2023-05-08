---
optionsClassName: TestVariablesMigrationConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TestVariablesMigrationConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TestVariablesMigrationConfig",
      "Enabled": false
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
status: Beta
processingTarget: Suites & Plans
classFile: /src/VstsSyncMigrator.Core/Execution/MigrationContext/TestVariablesMigrationContext.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/TestVariablesMigrationConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/TestVariablesMigrationContext/
title: TestVariablesMigrationContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: ../../../../../docs/Reference/v1/Processors/TestVariablesMigrationContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: ../../../../../docs/Reference/v1/Processors/TestVariablesMigrationContext-introduction.md
  exists: false
  markdown: ''

---