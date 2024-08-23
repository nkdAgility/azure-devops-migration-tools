---
optionsClassName: TfsTeamSettingsToolOptions
optionsClassFullName: MigrationTools.Tools.TfsTeamSettingsToolOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsTeamSettingsTool": {
            "Enabled": "True",
            "MigrateTeamCapacities": "True",
            "MigrateTeamSettings": "True",
            "Teams": null,
            "UpdateTeamSettings": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsTeamSettingsToolOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonToolsSamples": {
          "TfsTeamSettingsTool": {
            "Enabled": "True",
            "MigrateTeamCapacities": "True",
            "MigrateTeamSettings": "True",
            "Teams": [
              "Team 1",
              "Team 2"
            ],
            "UpdateTeamSettings": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsTeamSettingsToolOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsTeamSettingsToolOptions",
      "Enabled": true,
      "MigrateTeamSettings": true,
      "UpdateTeamSettings": true,
      "MigrateTeamCapacities": true,
      "Teams": [
        "Team 1",
        "Team 2"
      ]
    }
  sampleFor: MigrationTools.Tools.TfsTeamSettingsToolOptions
description: The TfsUserMappingTool is used to map users from the source to the target system. Run it with the ExportUsersForMappingContext to create a mapping file then with WorkItemMigrationContext to use the mapping file to update the users in the target system as you migrate the work items.
className: TfsTeamSettingsTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: MigrateTeamCapacities
  type: Boolean
  description: 'Migrate original team member capacities after their creation on the target team project. Note: It will only migrate team member capacity if the team member with same display name exists on the target collection otherwise it will be ignored.'
  defaultValue: false
- parameterName: MigrateTeamSettings
  type: Boolean
  description: Migrate original team settings after their creation on target team project
  defaultValue: false
- parameterName: Teams
  type: List
  description: List of Teams to process. If this is `null` then all teams will be processed.
  defaultValue: missng XML code comments
- parameterName: UpdateTeamSettings
  type: Boolean
  description: Reset the target team settings to match the source if the team exists
  defaultValue: false
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsTeamSettingsTool.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsTeamSettingsToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsTeamSettingsToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsTeamSettingsTool/
title: TfsTeamSettingsTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/TfsTeamSettingsTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/TfsTeamSettingsTool-introduction.md
  exists: false
  markdown: ''

---