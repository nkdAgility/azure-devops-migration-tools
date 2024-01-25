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
      "ReplicateAllExistingNodes": false
    }
  sampleFor: MigrationTools.Enrichers.TfsNodeStructureOptions
description: missng XML code comments
className: TfsNodeStructure
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: AreaMaps
  type: Dictionary
  description: Remapping rules for area paths, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`, that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied.
  defaultValue: '{}'
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: IterationMaps
  type: Dictionary
  description: Remapping rules for iteration paths, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`, that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied.
  defaultValue: '{}'
- parameterName: NodeBasePaths
  type: String[]
  description: The root paths of the Ares / Iterations you want migrate. See [NodeBasePath Configuration](#nodebasepath-configuration)
  defaultValue: '["/"]'
- parameterName: PrefixProjectToNodes
  type: Boolean
  description: Prefix the nodes with the new project name.
  defaultValue: false
- parameterName: RefName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ReplicateAllExistingNodes
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ShouldCreateMissingRevisionPaths
  type: Boolean
  description: When set to True the susyem will try to create any missing missing area or iteration paths from the revisions.
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