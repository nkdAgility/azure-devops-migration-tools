---
optionsClassName: TfsWorkItemOverwriteAreasAsTagsProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsWorkItemOverwriteAreasAsTagsProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.TfsWorkItemOverwriteAreasAsTagsProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.TfsWorkItemOverwriteAreasAsTagsProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsWorkItemOverwriteAreasAsTagsProcessorOptions",
      "Enabled": false,
      "AreaIterationPath": null,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Processors.TfsWorkItemOverwriteAreasAsTagsProcessorOptions
description: A common issue with older *TFS/Azure DevOps* instances is the proliferation of `Area Paths`. With the use of `Area Path` for `Teams` and the addition of the `Node Name` column option these extensive tag hierarchies should instad be moved to tags.
className: TfsWorkItemOverwriteAreasAsTagsProcessor
typeName: Processors
architecture: 
options:
- parameterName: AreaIterationPath
  type: String
  description: This is a required parameter. That define the root path of the iteration. To get the full path use `\`
  defaultValue: '\'
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: SourceName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
status: Beta
processingTarget: Work Item
classFile: /src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemOverwriteAreasAsTagsProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Processors/TfsWorkItemOverwriteAreasAsTagsProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TfsWorkItemOverwriteAreasAsTagsProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TfsWorkItemOverwriteAreasAsTagsProcessor/
title: TfsWorkItemOverwriteAreasAsTagsProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TfsWorkItemOverwriteAreasAsTagsProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TfsWorkItemOverwriteAreasAsTagsProcessor-introduction.md
  exists: false
  markdown: ''

---