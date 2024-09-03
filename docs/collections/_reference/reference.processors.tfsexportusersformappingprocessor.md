---
optionsClassName: TfsExportUsersForMappingProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsExportUsersForMappingProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.TfsExportUsersForMappingProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.TfsExportUsersForMappingProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsExportUsersForMappingProcessorOptions",
      "Enabled": false,
      "WIQLQuery": null,
      "OnlyListUsersInWorkItems": true,
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.TfsExportUsersForMappingProcessorOptions
description: ExportUsersForMappingContext is a tool used to create a starter mapping file for users between the source and target systems. Use `ExportUsersForMappingConfig` to configure.
className: TfsExportUsersForMappingProcessor
typeName: Processors
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: OnlyListUsersInWorkItems
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missng XML code comments
- parameterName: SourceName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TargetName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WIQLQuery
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Items
classFile: /src/MigrationTools.Clients.TfsObjectModel/Processors/TfsExportUsersForMappingProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Processors/TfsExportUsersForMappingProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TfsExportUsersForMappingProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TfsExportUsersForMappingProcessor/
title: TfsExportUsersForMappingProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TfsExportUsersForMappingProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TfsExportUsersForMappingProcessor-introduction.md
  exists: false
  markdown: ''

---