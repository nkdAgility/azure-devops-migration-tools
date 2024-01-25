---
optionsClassName: FixGitCommitLinksConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.FixGitCommitLinksConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "FixGitCommitLinksConfig",
      "Enabled": false,
      "TargetRepository": null,
      "Query": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.FixGitCommitLinksConfig
description: missng XML code comments
className: FixGitCommitLinks
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Query
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TargetRepository
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/VstsSyncMigrator.Core/Execution/ProcessingContext/FixGitCommitLinks.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/FixGitCommitLinksConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/FixGitCommitLinks/
title: FixGitCommitLinks
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/FixGitCommitLinks-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/Processors/FixGitCommitLinks-introduction.md
  exists: false
  markdown: ''

---