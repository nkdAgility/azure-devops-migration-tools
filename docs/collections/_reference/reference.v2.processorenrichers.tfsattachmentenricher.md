---
optionsClassName: TfsAttachmentEnricherOptions
optionsClassFullName: MigrationTools.Enrichers.TfsAttachmentEnricherOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsAttachmentEnricherOptions",
      "Enabled": true,
      "ExportBasePath": "c:\\temp\\WorkItemAttachmentExport",
      "MaxAttachmentSize": "480000000"
    }
  sampleFor: MigrationTools.Enrichers.TfsAttachmentEnricherOptions
description: missng XML code comments
className: TfsAttachmentEnricher
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: ExportBasePath
  type: String
  description: '`AttachmentMigration` is set to true then you need to specify a working path for attachments to be saved locally.'
  defaultValue: C:\temp\Migration\
- parameterName: MaxAttachmentSize
  type: String
  description: '`AttachmentMigration` is set to true then you need to specify a max file size for upload in bites. For Azure DevOps Services the default is 480,000,000 bites (60mb), for TFS its 32,000,000 bites (4mb).'
  defaultValue: 480000000
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsAttachmentEnricher.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsAttachmentEnricherOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/TfsAttachmentEnricher/
title: TfsAttachmentEnricher
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/TfsAttachmentEnricher-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/TfsAttachmentEnricher-introduction.md
  exists: false
  markdown: ''

---