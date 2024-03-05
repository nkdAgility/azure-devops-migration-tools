---
optionsClassName: TfsRevisionManagerOptions
optionsClassFullName: MigrationTools.Enrichers.TfsRevisionManagerOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsRevisionManagerOptions",
      "Enabled": true,
      "ReplayRevisions": true,
      "MaxRevisions": 0
    }
  sampleFor: MigrationTools.Enrichers.TfsRevisionManagerOptions
description: The TfsRevisionManager manipulates the revisions of a work item to reduce the number of revisions that are migrated.
className: TfsRevisionManager
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: MaxRevisions
  type: Int32
  description: Sets the maximum number of revisions that will be migrated. "First + Last N = Max". If this was set to 5 and there were 10 revisions you would get the first 1 (creation) and the latest 4 migrated.
  defaultValue: 0
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
- parameterName: ReplayRevisions
  type: Boolean
  description: You can choose to migrate the tip only (a single write) or all of the revisions (many writes). If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true`.
  defaultValue: true
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsRevisionManager.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsRevisionManagerOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/TfsRevisionManager/
title: TfsRevisionManager
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/TfsRevisionManager-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/TfsRevisionManager-introduction.md
  exists: false
  markdown: ''

---