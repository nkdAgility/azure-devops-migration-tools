---
optionsClassName: TfsSharedQueryProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsSharedQueryProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorDefaults": {
          "TfsSharedQueryProcessor": []
        }
      }
    }
  sampleFor: MigrationTools.Processors.TfsSharedQueryProcessorOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorDefaults": {
          "TfsSharedQueryProcessor": []
        }
      }
    }
  sampleFor: MigrationTools.Processors.TfsSharedQueryProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsSharedQueryProcessorOptions",
      "Enabled": false,
      "PrefixProjectToNodes": false,
      "SharedFolderName": "Shared Queries",
      "SourceToTargetFieldMappings": null,
      "Enrichers": null,
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.TfsSharedQueryProcessorOptions
description: The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another.
className: TfsSharedQueryProcessor
typeName: Processors
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
- parameterName: PrefixProjectToNodes
  type: Boolean
  description: Do we add the source project name into the folder path
  defaultValue: false
- parameterName: ProcessorEnrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missng XML code comments
- parameterName: SharedFolderName
  type: String
  description: The name of the shared folder, made a parameter incase it every needs to be edited
  defaultValue: Shared Queries
- parameterName: SourceName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: SourceToTargetFieldMappings
  type: Dictionary
  description: Mapping of the source to the target
  defaultValue: missng XML code comments
- parameterName: TargetName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: Beta
processingTarget: Queries
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TfsSharedQueryProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TfsSharedQueryProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TfsSharedQueryProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TfsSharedQueryProcessor/
title: TfsSharedQueryProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TfsSharedQueryProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TfsSharedQueryProcessor-introduction.md
  exists: false
  markdown: ''

---