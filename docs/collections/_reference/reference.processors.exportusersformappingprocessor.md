---
optionsClassName: ExportUsersForMappingProcessorOptions
optionsClassFullName: MigrationTools.Processors.ExportUsersForMappingProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Processors": [
          []
        ]
      }
    }
  sampleFor: MigrationTools.Processors.ExportUsersForMappingProcessorOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Processors": [
          []
        ]
      }
    }
  sampleFor: MigrationTools.Processors.ExportUsersForMappingProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "ExportUsersForMappingProcessorOptions",
      "Enabled": false,
      "WIQLQuery": null,
      "OnlyListUsersInWorkItems": true,
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.ExportUsersForMappingProcessorOptions
description: ExportUsersForMappingContext is a tool used to create a starter mapping file for users between the source and target systems. Use `ExportUsersForMappingConfig` to configure.
className: ExportUsersForMappingProcessor
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
- parameterName: OnlyListUsersInWorkItems
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
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
- parameterName: WIQLQuery
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: ready
processingTarget: Work Items
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/ExportUsersForMappingProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/ExportUsersForMappingProcessorOptions.cs

redirectFrom:
- /Reference/Processors/ExportUsersForMappingProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/ExportUsersForMappingProcessor/
title: ExportUsersForMappingProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/ExportUsersForMappingProcessor-notes.md
  exists: true
  markdown: >-
    There was a request to have the ability to map users to try and maintain integrity across different systems. We added a `TfsUserMappingEnricher` that allows you to map users from Source to Target.


    ##How it works


    1. Run `ExportUsersForMappingConfig` which will export all of the Users in Source Mapped or not to target.

    2. Run `WorkItemMigrationConfig` which will run a validator by detail to warn you of missing users. If it finds a mapping it will convert the field... 


    ## ExportUsersForMappingConfig


    Running the `ExportUsersForMappingConfig` to get the list of users will produce something like:


    ```

    [
      {
        "Source": {
          "FriendlyName": "Martin Hinshelwood nkdAgility.com",
          "AccountName": "martin@nkdagility.com"
        },
        "target": {
          "FriendlyName": "Hinshelwood, Martin",
          "AccountName": "martin@othercompany.com"
        }
      },
      {
        "Source": {
          "FriendlyName": "Rollup Bot",
          "AccountName": "Bot@nkdagility.com"
        },
        "target": {
          "FriendlyName": "Service Account 4",
          "AccountName": "randoaccount@somecompany.com"
        }
      },
      {
        "Source": {
          "FriendlyName": "Another non mapped Account",
          "AccountName": "not-mapped@nkdagility.com"
        },
        "target": null
      }
    ]

    ```


    Any `null` in the target field means that the user is not mapped. You can then use this to create a mapping file will all of your users.


    IMPORTANT: The Friendly name in Azure DevOps / TFS is not nessesarily the AAD Friendly name as users can change this in the tool. We load all of the users from both systems, and match on "email" to ensure we only assume mapping for the same user. Non mapped users, or users listed as null, will not be mapped.


    ### Notes


    - On `ExportUsersForMappingConfig` you can set `OnlyListUsersInWorkItems` to filter the mapping based on the scope of the query. This is greater if you have many users.

    - Configured using the `TfsUserMappingEnricherOptions` setting in `CommonEnrichersConfig`


    ## WorkItemMigrationConfig


    When you run the `WorkItemMigrationContext`



    ```

    ...
      "LogLevel": "Debug",
      "CommonEnrichersConfig": [
        {
          "$type": "TfsUserMappingEnricherOptions",
          "Enabled": true,
          "UserMappingFile": "C:\\temp\\userExport.json",
          "IdentityFieldsToCheck": [
            "System.AssignedTo",
            "System.ChangedBy",
            "System.CreatedBy",
            "Microsoft.VSTS.Common.ActivatedBy",
            "Microsoft.VSTS.Common.ResolvedBy",
            "Microsoft.VSTS.Common.ClosedBy"
          ]
        }
      ],
    ...

    ```



    ### Notes


    - Configured using the `TfsUserMappingEnricherOptions` setting in `CommonEnrichersConfig`

    - Applies to all identity fields specified in the list
- topic: introduction
  path: /docs/Reference/Processors/ExportUsersForMappingProcessor-introduction.md
  exists: false
  markdown: ''

---