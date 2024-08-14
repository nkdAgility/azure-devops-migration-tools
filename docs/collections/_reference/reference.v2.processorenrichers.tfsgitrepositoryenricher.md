---
optionsClassName: TfsGitRepositoryEnricherOptions
optionsClassFullName: MigrationTools.Enrichers.TfsGitRepositoryEnricherOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsGitRepositoryEnricherOptions",
      "Enabled": true
    }
  sampleFor: MigrationTools.Enrichers.TfsGitRepositoryEnricherOptions
description: missng XML code comments
className: TfsGitRepositoryEnricher
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Enrichers/TfsGitRepositoryEnricher.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Enrichers/TfsGitRepositoryEnricherOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/TfsGitRepositoryEnricher/
title: TfsGitRepositoryEnricher
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/TfsGitRepositoryEnricher-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/TfsGitRepositoryEnricher-introduction.md
  exists: false
  markdown: ''

---