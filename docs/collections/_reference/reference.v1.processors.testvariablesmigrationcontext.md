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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/TestVariablesMigrationContext/
title: TestVariablesMigrationContext
categories:
- Processors
- v1
notes: ''
introduction: ''

---