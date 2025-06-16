---
optionsClassName: KeepOutboundLinkTargetProcessorOptions
optionsClassFullName: MigrationTools.Clients.AzureDevops.Rest.Processors.KeepOutboundLinkTargetProcessorOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Clients.AzureDevops.Rest.Processors.KeepOutboundLinkTargetProcessorOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Clients.AzureDevops.Rest.Processors.KeepOutboundLinkTargetProcessorOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "KeepOutboundLinkTargetProcessorOptions",
      "Enabled": false,
      "WIQLQuery": "Select [System.Id] From WorkItems Where [System.TeamProject] = @project and not [System.WorkItemType] contains 'Test Suite, Test Plan,Shared Steps,Shared Parameter,Feedback Request'",
      "TargetLinksToKeepOrganization": "https://dev.azure.com/nkdagility",
      "TargetLinksToKeepProject": "df1a9cb1-115a-43f2-8034-d50128fee331",
      "CleanupFileName": "c:/temp/OutboundLinkTargets.bat",
      "PrependCommand": "start",
      "DryRun": true,
      "SourceName": null,
      "TargetName": null
    }
  sampleFor: MigrationTools.Clients.AzureDevops.Rest.Processors.KeepOutboundLinkTargetProcessorOptions
description: missing XML code comments
className: KeepOutboundLinkTargetProcessor
typeName: Processors
architecture: 
options:
- parameterName: CleanupFileName
  type: String
  description: File path where the cleanup script or batch file will be generated for removing unwanted outbound links.
  defaultValue: missing XML code comments
- parameterName: DryRun
  type: Boolean
  description: When true, performs a dry run without making actual changes, only generating the cleanup script for review.
  defaultValue: missing XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: PrependCommand
  type: String
  description: Command to prepend to each line in the cleanup script, such as "start" for Windows batch files.
  defaultValue: missing XML code comments
- parameterName: SourceName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: TargetLinksToKeepOrganization
  type: String
  description: URL of the Azure DevOps organization whose links should be preserved during cleanup operations.
  defaultValue: missing XML code comments
- parameterName: TargetLinksToKeepProject
  type: String
  description: Project name or GUID within the target organization whose links should be preserved.
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: WIQLQuery
  type: String
  description: WIQL (Work Item Query Language) query used to select the work items whose outbound links should be processed for preservation.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.AzureDevops.Rest/Processors/KeepOutboundLinkTargetProcessorOptions.cs
optionsClassFile: src/MigrationTools.Clients.AzureDevops.Rest/Processors/KeepOutboundLinkTargetProcessorOptions.cs

redirectFrom:
- /Reference/Processors/KeepOutboundLinkTargetProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/KeepOutboundLinkTargetProcessor/
title: KeepOutboundLinkTargetProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: ../../docs/Reference/Processors/KeepOutboundLinkTargetProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: ../../docs/Reference/Processors/KeepOutboundLinkTargetProcessor-introduction.md
  exists: false
  markdown: ''

---