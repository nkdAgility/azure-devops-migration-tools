---
optionsClassName: ImportProfilePictureConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.ImportProfilePictureConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "ImportProfilePictureConfig",
      "Enabled": false
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
status: alpha
processingTarget: Profiles
classFile: /src/VstsSyncMigrator.Core/Execution/ProcessingContext/ImportProfilePictureContext.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/ImportProfilePictureConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/ImportProfilePictureContext/
title: ImportProfilePictureContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/ImportProfilePictureContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/Processors/ImportProfilePictureContext-introduction.md
  exists: false
  markdown: ''

---