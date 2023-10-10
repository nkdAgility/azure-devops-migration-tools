---
optionsClassName: ExportUsersForMappingConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.ExportUsersForMappingConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "ExportUsersForMappingConfig",
      "Enabled": false,
      "LocalExportJsonFile": null,
      "WIQLQuery": null,
      "IdentityFieldsToCheck": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.ExportUsersForMappingConfig
description: missng XML code comments
className: ExportUsersForMapping
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: IdentityFieldsToCheck
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: LocalExportJsonFile
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WIQLQuery
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/VstsSyncMigrator.Core/Execution/MigrationContext/ExportUsersForMapping.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/ExportUsersForMappingConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/ExportUsersForMapping/
title: ExportUsersForMapping
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/ExportUsersForMapping-notes.md
  exists: true
  markdown: >-
    ## Additional Samples & Info


    ```

    {
        "$type": "ExportUsersForMappingConfig",
        "Enabled": false,
        "LocalExportJsonFile": "c:\\temp\\ExportUsersForMappingConfig.json",
        "WIQLQuery": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc",
        "IdentityFieldsToCheck": [
        "System.AssignedTo",
        "System.ChangedBy",
        "System.CreatedBy",
        "Microsoft.VSTS.Common.ActivatedBy",
        "Microsoft.VSTS.Common.ResolvedBy",
        "Microsoft.VSTS.Common.ClosedBy"
        ]
    }

    ```
- topic: introduction
  path: /docs/Reference/v1/Processors/ExportUsersForMapping-introduction.md
  exists: false
  markdown: ''

---