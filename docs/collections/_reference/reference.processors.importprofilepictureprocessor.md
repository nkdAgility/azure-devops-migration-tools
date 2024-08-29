---
optionsClassName: ImportProfilePictureProcessorOptions
optionsClassFullName: MigrationTools.Processors.ImportProfilePictureProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.ImportProfilePictureProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.ImportProfilePictureProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "ImportProfilePictureProcessorOptions",
      "Enabled": false,
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.ImportProfilePictureProcessorOptions
description: Downloads corporate images and updates TFS/Azure DevOps profiles
className: ImportProfilePictureProcessor
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
status: alpha
processingTarget: Profiles
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/ImportProfilePictureProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/ImportProfilePictureProcessorOptions.cs

redirectFrom:
- /Reference/Processors/ImportProfilePictureProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/ImportProfilePictureProcessor/
title: ImportProfilePictureProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/ImportProfilePictureProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/ImportProfilePictureProcessor-introduction.md
  exists: false
  markdown: ''

---