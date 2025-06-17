---
optionsClassName: TfsAttachmentToolOptions
optionsClassFullName: MigrationTools.Tools.TfsAttachmentToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.TfsAttachmentToolOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.TfsAttachmentToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsAttachmentToolOptions",
      "Enabled": false,
      "ExportBasePath": null,
      "MaxAttachmentSize": 0
    }
  sampleFor: MigrationTools.Tools.TfsAttachmentToolOptions
description: Tool for processing and migrating work item attachments between Team Foundation Server instances, handling file downloads, uploads, and attachment metadata.
className: TfsAttachmentTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: ExportBasePath
  type: String
  description: '`AttachmentMigration` is set to true then you need to specify a working path for attachments to be saved locally.'
  defaultValue: C:\temp\Migration\
- parameterName: MaxAttachmentSize
  type: Int32
  description: '`AttachmentMigration` is set to true then you need to specify a max file size for upload in bites. For Azure DevOps Services the default is 480,000,000 bites (60mb), for TFS its 32,000,000 bites (4mb).'
  defaultValue: 480000000
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsAttachmentTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsAttachmentToolOptions.cs
notes:
  exists: false
  path: docs/Reference/Tools/TfsAttachmentTool-notes.md
  markdown: ''

redirectFrom:
- /Reference/Tools/TfsAttachmentToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsAttachmentTool/
title: TfsAttachmentTool
categories:
- Tools
- 
topics:
- topic: notes
  path: docs/Reference/Tools/TfsAttachmentTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsAttachmentTool-introduction.md
  exists: false
  markdown: ''

---