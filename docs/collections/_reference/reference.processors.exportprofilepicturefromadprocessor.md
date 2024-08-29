---
optionsClassName: ExportProfilePictureFromADProcessorOptions
optionsClassFullName: MigrationTools.Processors.ExportProfilePictureFromADProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.ExportProfilePictureFromADProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.ExportProfilePictureFromADProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "ExportProfilePictureFromADProcessorOptions",
      "Enabled": false,
      "Domain": null,
      "Username": null,
      "Password": null,
      "PictureEmpIDFormat": null,
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.ExportProfilePictureFromADProcessorOptions
description: Downloads corporate images and updates TFS/Azure DevOps profiles
className: ExportProfilePictureFromADProcessor
typeName: Processors
architecture: 
options:
- parameterName: Domain
  type: String
  description: The source domain where the pictures should be exported.
  defaultValue: String.Empty
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: Password
  type: String
  description: The password of the user that is used to export the pictures.
  defaultValue: String.Empty
- parameterName: PictureEmpIDFormat
  type: String
  description: 'TODO: You wpuld need to customise this for your system. Clone repo and run in Debug'
  defaultValue: String.Empty
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
- parameterName: Username
  type: String
  description: The user name of the user that is used to export the pictures.
  defaultValue: String.Empty
status: alpha
processingTarget: Profiles
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/ExportProfilePictureFromADProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/ExportProfilePictureFromADProcessorOptions.cs

redirectFrom:
- /Reference/Processors/ExportProfilePictureFromADProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/ExportProfilePictureFromADProcessor/
title: ExportProfilePictureFromADProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/ExportProfilePictureFromADProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/ExportProfilePictureFromADProcessor-introduction.md
  exists: false
  markdown: ''

---