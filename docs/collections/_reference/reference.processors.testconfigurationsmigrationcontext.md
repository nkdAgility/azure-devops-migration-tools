---
optionsClassName: TestConfigurationsMigrationConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TestConfigurationsMigrationConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TestConfigurationsMigrationConfig",
      "Enabled": false,
      "Enrichers": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TestConfigurationsMigrationConfig
description: This processor can migrate `test configuration`. This should be run before `LinkMigrationConfig`.
className: TestConfigurationsMigrationContext
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
- /Reference/v1/Processors/TestConfigurationsMigrationConfig/
layout: reference
toc: true
permalink: /Reference/Processors/TestConfigurationsMigrationContext/
title: TestConfigurationsMigrationContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /Processors/TestConfigurationsMigrationContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/TestConfigurationsMigrationContext-introduction.md
  exists: false
  markdown: ''

---