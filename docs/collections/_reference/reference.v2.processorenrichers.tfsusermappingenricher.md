---
optionsClassName: TfsUserMappingEnricherOptions
optionsClassFullName: MigrationTools.Enrichers.TfsUserMappingEnricherOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsUserMappingEnricherOptions",
      "Enabled": true
    }
  sampleFor: MigrationTools.Enrichers.TfsUserMappingEnricherOptions
description: missng XML code comments
className: TfsUserMappingEnricher
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
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsUserMappingEnricher.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsUserMappingEnricherOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/TfsUserMappingEnricher/
title: TfsUserMappingEnricher
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/TfsUserMappingEnricher-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/TfsUserMappingEnricher-introduction.md
  exists: false
  markdown: ''

---