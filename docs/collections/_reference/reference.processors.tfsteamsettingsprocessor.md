---
optionsClassName: TfsTeamSettingsProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsTeamSettingsProcessorOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorDefaults": {
          "TfsTeamSettingsProcessor": []
        }
      }
    }
  sampleFor: MigrationTools.Processors.TfsTeamSettingsProcessorOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorSamples": {
          "TfsTeamSettingsProcessor": []
        }
      }
    }
  sampleFor: MigrationTools.Processors.TfsTeamSettingsProcessorOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsTeamSettingsProcessorOptions",
      "Enabled": false,
      "MigrateTeamSettings": false,
      "UpdateTeamSettings": false,
      "PrefixProjectToNodes": false,
      "MigrateTeamCapacities": false,
      "Teams": null,
      "Enrichers": null,
      "ProcessorEnrichers": null,
      "SourceName": null,
      "TargetName": null,
      "RefName": null
    }
  sampleFor: MigrationTools.Processors.TfsTeamSettingsProcessorOptions
description: Native TFS Processor, does not work with any other Endpoints.
className: TfsTeamSettingsProcessor
typeName: Processors
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Enrichers
  type: List
  description: A list of enrichers that can augment the proccessing of the data
  defaultValue: missng XML code comments
- parameterName: MigrateTeamCapacities
  type: Boolean
  description: 'Migrate original team member capacities after their creation on the target team project. Note: It will only migrate team member capacity if the team member with same display name exists on the target collection otherwise it will be ignored.'
  defaultValue: false
- parameterName: MigrateTeamSettings
  type: Boolean
  description: Migrate original team settings after their creation on target team project
  defaultValue: false
- parameterName: PrefixProjectToNodes
  type: Boolean
  description: Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
  defaultValue: false
- parameterName: ProcessorEnrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
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
- parameterName: Teams
  type: List
  description: List of Teams to process. If this is `null` then all teams will be processed.
  defaultValue: missng XML code comments
- parameterName: UpdateTeamSettings
  type: Boolean
  description: Reset the target team settings to match the source if the team exists
  defaultValue: false
status: Beta
processingTarget: Teams
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TfsTeamSettingsProcessor.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Processors/TfsTeamSettingsProcessorOptions.cs

redirectFrom:
- /Reference/Processors/TfsTeamSettingsProcessorOptions/
layout: reference
toc: true
permalink: /Reference/Processors/TfsTeamSettingsProcessor/
title: TfsTeamSettingsProcessor
categories:
- Processors
- 
topics:
- topic: notes
  path: /docs/Reference/Processors/TfsTeamSettingsProcessor-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Processors/TfsTeamSettingsProcessor-introduction.md
  exists: false
  markdown: ''

---