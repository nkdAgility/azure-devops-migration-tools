---
optionsClassName: TfsWorkItemLinkEnricherOptions
optionsClassFullName: MigrationTools.Enrichers.TfsWorkItemLinkEnricherOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsWorkItemLinkEnricherOptions",
      "Enabled": true,
      "FilterIfLinkCountMatches": true,
      "SaveAfterEachLinkIsAdded": false
    }
  sampleFor: MigrationTools.Enrichers.TfsWorkItemLinkEnricherOptions
description: missng XML code comments
className: TfsWorkItemLinkEnricher
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: For internal use
  defaultValue: missng XML code comments
- parameterName: FilterIfLinkCountMatches
  type: Boolean
  description: Skip validating links if the number of links in the source and the target matches!
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
- parameterName: SaveAfterEachLinkIsAdded
  type: Boolean
  description: Save the work item after each link is added. This will slow the migration as it will cause many saves to the TFS database.
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsWorkItemLinkEnricher.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsWorkItemLinkEnricherOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/TfsWorkItemLinkEnricher/
title: TfsWorkItemLinkEnricher
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/TfsWorkItemLinkEnricher-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/TfsWorkItemLinkEnricher-introduction.md
  exists: false
  markdown: ''

---