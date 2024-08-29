---
optionsClassName: OutboundLinkCheckingProcessorOptions
optionsClassFullName: MigrationTools.Clients.AzureDevops.Rest.Processors.OutboundLinkCheckingProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Clients.AzureDevops.Rest.Processors.OutboundLinkCheckingProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Clients.AzureDevops.Rest.Processors.OutboundLinkCheckingProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "OutboundLinkCheckingProcessorOptions",
      "Enabled": false,
      "WIQLQuery": null,
      "ResultFileName": null,
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Clients.AzureDevops.Rest.Processors.OutboundLinkCheckingProcessorOptions
description: missng XML code comments
className: OutboundLinkCheckingProcessor
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
- parameterName: ResultFileName
  type: String
  description: missng XML code comments
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
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.Rest/Processors/OutboundLinkCheckingProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.Rest/Processors/OutboundLinkCheckingProcessorOptions.cs

redirectFrom:
- /Reference/Processors/OutboundLinkCheckingProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/OutboundLinkCheckingProcessor/
title: OutboundLinkCheckingProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/OutboundLinkCheckingProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/OutboundLinkCheckingProcessor-introduction.md
  exists: false
  markdown: ''

---