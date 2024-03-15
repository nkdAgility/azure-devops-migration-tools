---
optionsClassName: TfsUserMappingEnricherOptions
optionsClassFullName: MigrationTools.Enrichers.TfsUserMappingEnricherOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsUserMappingEnricherOptions",
      "Enabled": false,
      "IdentityFieldsToCheck": [
        "System.AssignedTo",
        "System.ChangedBy",
        "System.CreatedBy",
        "Microsoft.VSTS.Common.ActivatedBy",
        "Microsoft.VSTS.Common.ResolvedBy",
        "Microsoft.VSTS.Common.ClosedBy"
      ],
      "UserMappingFile": "usermapping.json"
    }
  sampleFor: MigrationTools.Enrichers.TfsUserMappingEnricherOptions
description: The TfsUserMappingEnricher is used to map users from the source to the target system. Run it with the ExportUsersForMappingContext to create a mapping file then with WorkItemMigrationContext to use the mapping file to update the users in the target system as you migrate the work items.
className: TfsUserMappingEnricher
typeName: ProcessorEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: IdentityFieldsToCheck
  type: List
  description: This is a list of the Identiy fields in the Source to check for user mapping purposes. You should list all identiy fields that you wan to map.
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
- parameterName: UserMappingFile
  type: String
  description: This is the file that will be used to export or import the user mappings. Use the ExportUsersForMapping processor to create the file.
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsUserMappingEnricher.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/ProcessorEnrichers/TfsUserMappingEnricherOptions.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/ProcessorEnrichers/TfsUserMappingEnricher/
title: TfsUserMappingEnricher
categories:
- ProcessorEnrichers
- v2
topics:
- topic: notes
  path: /docs/Reference/v2/ProcessorEnrichers/TfsUserMappingEnricher-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v2/ProcessorEnrichers/TfsUserMappingEnricher-introduction.md
  exists: false
  markdown: ''

---