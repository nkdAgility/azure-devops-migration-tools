---
optionsClassName: TfsNodeStructureOptions
optionsClassFullName: MigrationTools.Enrichers.TfsNodeStructureOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsNodeStructureOptions",
      "Enabled": true,
      "PrefixProjectToNodes": false,
      "NodeBasePaths": null,
      "AreaMaps": {
        "$type": "Dictionary`2"
      },
      "IterationMaps": {
        "$type": "Dictionary`2"
      },
      "ShouldCreateMissingRevisionPaths": true,
      "ShouldCreateNodesUpFront": true
    }
  sampleFor: MigrationTools.Enrichers.TfsNodeStructureOptions
description: missng XML code comments
className: TfsNodeStructure
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: AreaMaps
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: IterationMaps
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: NodeBasePaths
  type: String[]
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: PrefixProjectToNodes
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ShouldCreateMissingRevisionPaths
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ShouldCreateNodesUpFront
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsNodeStructure.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsNodeStructureOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/TfsNodeStructure/
title: TfsNodeStructure
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/TfsNodeStructure-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/TfsNodeStructure-introduction.md
  exists: false
  markdown: ''

---