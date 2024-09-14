---
optionsClassName: AzureDevOpsPipelineProcessorOptions
optionsClassFullName: MigrationTools.Processors.AzureDevOpsPipelineProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "Processors": [
          {
            "ProcessorType": "AzureDevOpsPipelineProcessor",
            "BuildPipelines": "",
            "Enabled": "False",
            "MigrateBuildPipelines": "True",
            "MigrateReleasePipelines": "True",
            "MigrateServiceConnections": "True",
            "MigrateTaskGroups": "True",
            "MigrateVariableGroups": "True",
            "ReleasePipelines": "",
            "SourceName": "sourceName",
            "TargetName": "targetName"
          }
        ]
      }
    }
  sampleFor: MigrationTools.Processors.AzureDevOpsPipelineProcessorOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "Processors": [
          {
            "ProcessorType": "AzureDevOpsPipelineProcessor",
            "BuildPipelines": "",
            "Enabled": "False",
            "MigrateBuildPipelines": "True",
            "MigrateReleasePipelines": "True",
            "MigrateServiceConnections": "True",
            "MigrateTaskGroups": "True",
            "MigrateVariableGroups": "True",
            "ReleasePipelines": "",
            "SourceName": "sourceName",
            "TargetName": "targetName"
          }
        ]
      }
    }
  sampleFor: MigrationTools.Processors.AzureDevOpsPipelineProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "AzureDevOpsPipelineProcessorOptions",
      "Enabled": false,
      "MigrateBuildPipelines": true,
      "MigrateReleasePipelines": true,
      "MigrateTaskGroups": true,
      "MigrateVariableGroups": true,
      "MigrateServiceConnections": true,
      "BuildPipelines": null,
      "ReleasePipelines": null,
      "RepositoryNameMaps": null,
      "Enrichers": null,
      "SourceName": "sourceName",
      "TargetName": "targetName",
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.AzureDevOpsPipelineProcessorOptions
description: Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines.
className: AzureDevOpsPipelineProcessor
typeName: Processors
architecture: 
options:
- parameterName: BuildPipelines
  type: List
  description: List of Build Pipelines to process. If this is `null` then all Build Pipelines will be processed.
  defaultValue: missing XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: Enrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missing XML code comments
- parameterName: MigrateBuildPipelines
  type: Boolean
  description: Migrate Build Pipelines
  defaultValue: true
- parameterName: MigrateReleasePipelines
  type: Boolean
  description: Migrate Release Pipelines
  defaultValue: true
- parameterName: MigrateServiceConnections
  type: Boolean
  description: Migrate Service Connections **secrets need to be entered manually**
  defaultValue: true
- parameterName: MigrateTaskGroups
  type: Boolean
  description: Migrate Task Groups
  defaultValue: true
- parameterName: MigrateVariableGroups
  type: Boolean
  description: Migrate Valiable Groups
  defaultValue: true
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missing XML code comments
- parameterName: ReleasePipelines
  type: List
  description: List of Release Pipelines to process. If this is `null` then all Release Pipelines will be processed.
  defaultValue: missing XML code comments
- parameterName: RepositoryNameMaps
  type: Dictionary
  description: Map of Source Repository to Target Repository Names
  defaultValue: missing XML code comments
- parameterName: SourceName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: TargetName
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
status: Beta
processingTarget: Pipelines
classFile: /src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.Rest/Processors/AzureDevOpsPipelineProcessorOptions.cs

redirectFrom:
- /Reference/Processors/AzureDevOpsPipelineProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/AzureDevOpsPipelineProcessor/
title: AzureDevOpsPipelineProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/AzureDevOpsPipelineProcessor-notes.md
  exists: true
  markdown: >2-

    ### Example Full Migration from v12.0


    The following file is an example that can be used in your `configuration.json` file to migrate Azure DevOps pipelines.

    ```json

    {
        "GitRepoMapping": null,
        "LogLevel": "Information",
        "Processors": [
          {
            "$type": "AzureDevOpsPipelineProcessorOptions",
            "Enabled": true,
            "MigrateBuildPipelines": true,
            "MigrateReleasePipelines": true,
            "MigrateTaskGroups": true,
            "MigrateVariableGroups": true,
            "MigrateServiceConnections": true,
            "BuildPipelines": null,
            "ReleasePipelines": null,
            "RefName": null,
            "SourceName": "Source",
            "TargetName": "Target",
            "RepositoryNameMaps": {}
          }
        ],
        "Version": "12.0",
        "Endpoints": {
          "AzureDevOpsEndpoints": [
            {
              "name": "Source",
              "$type": "AzureDevOpsEndpointOptions",
              "Organisation": "https://dev.azure.com/source-org/",
              "Project": "Awesome project",
              "AuthenticationMode": "AccessToken",
              "AccessToken": "xxxxxx",
              "EndpointEnrichers": null
            },
            {
              "Name": "Target",
              "$type": "AzureDevOpsEndpointOptions",
              "Organisation": "https://dev.azure.com/target-org/",
              "Project": "Cool project",
              "AuthenticationMode": "AccessToken",
              "AccessToken": "xxxxxx",
              "EndpointEnrichers": null
            }
          ]
        }
      }
    ```


    If the repository in the target has a different name from the one that was used in the source project, you should map it.

    In the example above replace `"RepositoryNameMaps": {}` with the following:

    ```json

    "RepositoryNameMaps": {
        "Awesome project": "Cool project"
    }

    ```


    # Important note

    When the application is creating service connections make sure you have proper permissions on Azure Active Directory and you can grant Contributor role to the subscription that was chosen.
- topic: introduction
  path: /docs/Reference/Processors/AzureDevOpsPipelineProcessor-introduction.md
  exists: true
  markdown: >2-

    ## Features

    - Migrates service connections

    - Migrates variable groups

    - Migrates task groups

    - Migrates classic and yml build pipelines

    - Migrates release pipelines

---