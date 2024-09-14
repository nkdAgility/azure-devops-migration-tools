---
optionsClassName: WorkItemTrackingProcessorOptions
optionsClassFullName: MigrationTools.Processors.WorkItemTrackingProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Processors.WorkItemTrackingProcessorOptions
- name: sample
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Processors.WorkItemTrackingProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "WorkItemTrackingProcessorOptions",
      "Enabled": false,
      "ReplayRevisions": false,
      "CollapseRevisions": false,
      "WorkItemCreateRetryLimit": 0,
      "Enrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.WorkItemTrackingProcessorOptions
description: This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md).
className: WorkItemTrackingProcessor
typeName: Processors
architecture: 
options:
- parameterName: CollapseRevisions
  type: Boolean
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: Enrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missing XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missing XML code comments
- parameterName: ReplayRevisions
  type: Boolean
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: SourceName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: WorkItemCreateRetryLimit
  type: Int32
  description: missing XML code comments
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools/Processors/WorkItemTrackingProcessor.cs
optionsClassFile: /src/MigrationTools/Processors/WorkItemTrackingProcessorOptions.cs

redirectFrom:
- /Reference/Processors/WorkItemTrackingProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/WorkItemTrackingProcessor/
title: WorkItemTrackingProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/WorkItemTrackingProcessor-notes.md
  exists: true
  markdown: >2+

    ### Supported Endpoints


    - TfsWorkItemEndpoint

    - FileSystemWorkItemEndpoint

    - InMemoryWorkItemEndpoint


    ### Supported Processor Enrichers


    - PauseAfterEachWorkItem

    - AppendMigrationToolSignatureFooter

    - FilterWorkItemsThatAlreadyExistInTarget

    - SkipToFinalRevisedWorkItemType


    #### Full Example with Enpoints & Enrichers



    ```JSON
        {
          "ObjectType": "WorkItemMigrationProcessorOptions",
          "Enabled": true,
          "ReplayRevisions": true,
          "PrefixProjectToNodes": false,
          "CollapseRevisions": false,
          "WorkItemCreateRetryLimit": 5,
          "Enrichers": [
            {
              "ObjectType": "PauseAfterEachItemOptions",
              "Enabled": true
            },
            {
              "ObjectType": "FilterWorkItemsThatAlreadyExistInTargetOptions",
              "Enabled": true,
              "Query": {
                "WhereBit": "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
                "OrderBit": "[System.ChangedDate] desc"
              }
            },
            {
              "ObjectType": "AppendMigrationToolSignatureFooterOptions",
              "Enabled": false
            },
            {
              "ObjectType": "SkipToFinalRevisedWorkItemTypeOptions",
              "Enabled": false
            }
          ],
          "Endpoints": [
            {
              "ObjectType": "FileSystemWorkItemEndpointOptions",
              "Direction": "Source",
              "FileStore": ".\\Store\\Source\\",
              "Query": {
                "WhereBit": "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
                "OrderBit": "[System.ChangedDate] desc"
              },
              "Enrichers": [
                {
                  "ObjectType": "WorkItemAttachmentEnricherOptions",
                  "Enabled": true,
                  "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
                  "AttachmentMaxSize": 480000000
                },
                {
                  "ObjectType": "WorkItemLinkEnricherOptions",
                  "Enabled": true,
                  "LinkMigrationSaveEachAsAdded": true
                }
              ]
            },
            {
              "ObjectType": "TfsWorkItemEndPointOptions",
              "Direction": "Target",
              "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
              "Query": {
                "Query": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc"
              },
              "Enrichers": [
                {
                  "ObjectType": "WorkItemAttachmentEnricherOptions",
                  "Enabled": true,
                  "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
                  "AttachmentMaxSize": 480000000
                },
                {
                  "ObjectType": "WorkItemEmbedEnricherOptions",
                  "Enabled": true,
                  "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\"
                },
                {
                  "ObjectType": "WorkItemLinkEnricherOptions",
                  "Enabled": true,
                  "LinkMigrationSaveEachAsAdded": true
                },
                {
                  "ObjectType": "WorkItemCreatedEnricherOptions",
                  "Enabled": true,
                  "UpdateCreatedDate": true,
                  "UpdateCreatedBy": true
                },
                {
                  "ObjectType": "WorkItemFieldTableEnricherOptions",
                  "Enabled": true
                }
              ]
            }
          ]
        }
    ```
- topic: introduction
  path: /docs/Reference/Processors/WorkItemTrackingProcessor-introduction.md
  exists: false
  markdown: ''

---