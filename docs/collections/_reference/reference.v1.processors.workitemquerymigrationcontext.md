---
optionsClassName: WorkItemQueryMigrationConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.WorkItemQueryMigrationConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "WorkItemQueryMigrationConfig",
      "Enabled": false,
      "PrefixProjectToNodes": false,
      "SharedFolderName": "Shared Queries",
      "SourceToTargetFieldMappings": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.WorkItemQueryMigrationConfig
description: This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool.
className: WorkItemQueryMigrationContext
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: PrefixProjectToNodes
  type: Boolean
  description: Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
  defaultValue: false
- parameterName: SharedFolderName
  type: String
  description: The name of the shared folder, made a parameter incase it every needs to be edited
  defaultValue: none
- parameterName: SourceToTargetFieldMappings
  type: Dictionary
  description: Any field mappings
  defaultValue: none
status: preview
processingTarget: Shared Queries
classFile: /src/VstsSyncMigrator.Core/Execution/MigrationContext/WorkItemQueryMigrationContext.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/WorkItemQueryMigrationConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/WorkItemQueryMigrationContext/
title: WorkItemQueryMigrationContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: ../../../../../docs/Reference/v1/Processors/WorkItemQueryMigrationContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: ../../../../../docs/Reference/v1/Processors/WorkItemQueryMigrationContext-introduction.md
  exists: false
  markdown: ''

---