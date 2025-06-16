---
optionsClassName: TfsSharedQueryProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsSharedQueryProcessorOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.TfsSharedQueryProcessorOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.TfsSharedQueryProcessorOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsSharedQueryProcessorOptions",
      "Enabled": false,
      "PrefixProjectToNodes": false,
      "SharedFolderName": "Shared Queries",
      "SourceToTargetFieldMappings": null,
      "SourceName": null,
      "TargetName": null
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
  defaultValue: missing XML code comments
- parameterName: PrefixProjectToNodes
  type: Boolean
  description: Do we add the source project name into the folder path
  defaultValue: false
- parameterName: SharedFolderName
  type: String
  description: The name of the shared folder, made a parameter incase it every needs to be edited
  defaultValue: Shared Queries
- parameterName: SourceName
  type: String
  description: This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor.
  defaultValue: missing XML code comments
- parameterName: SourceToTargetFieldMappings
  type: Dictionary
  description: Mapping of the source to the target
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a read only processor.
  defaultValue: missing XML code comments
status: Beta
processingTarget: Queries
classFile: src/MigrationTools.Clients.TfsObjectModel/Processors/TfsSharedQueryProcessorOptions.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Processors/TfsSharedQueryProcessorOptions.cs

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