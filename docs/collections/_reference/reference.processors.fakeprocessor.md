---
optionsClassName: FakeProcessorConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.FakeProcessorConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FakeProcessorConfig",
      "Enabled": false,
      "Enrichers": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.FakeProcessorConfig
description: "Note: this is only for internal usage. Don't use this in your configurations."
className: FakeProcessor
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
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/FakeProcessor.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/FakeProcessorConfig.cs

redirectFrom:
- /Reference/v1/Processors/FakeProcessorConfig/
layout: reference
toc: true
permalink: /Reference/Processors/FakeProcessor/
title: FakeProcessor
categories:
- Processors
- v1
topics:
- topic: notes
  path: /Processors/FakeProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/FakeProcessor-introduction.md
  exists: false
  markdown: ''

---