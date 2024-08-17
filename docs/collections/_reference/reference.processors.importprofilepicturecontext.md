---
optionsClassName: ImportProfilePictureConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.ImportProfilePictureConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "ImportProfilePictureConfig",
      "Enabled": false,
      "Enrichers": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.ImportProfilePictureConfig
description: Downloads corporate images and updates TFS/Azure DevOps profiles
className: ImportProfilePictureContext
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
status: alpha
processingTarget: Profiles
classFile: /src/VstsSyncMigrator.Core/Execution/ProcessingContext/ImportProfilePictureContext.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/ImportProfilePictureConfig.cs

redirectFrom:
- /Reference/v1/Processors/ImportProfilePictureConfig/
layout: reference
toc: true
permalink: /Reference/Processors/ImportProfilePictureContext/
title: ImportProfilePictureContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /Processors/ImportProfilePictureContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Processors/ImportProfilePictureContext-introduction.md
  exists: false
  markdown: ''

---