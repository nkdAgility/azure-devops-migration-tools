---
optionsClassName: TestConfigurationsMigrationConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TestConfigurationsMigrationConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TestConfigurationsMigrationConfig",
      "Enabled": false
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

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/TestConfigurationsMigrationContext/
title: TestConfigurationsMigrationContext
categories:
- Processors
- v1
notes: ''
introduction: ''

---