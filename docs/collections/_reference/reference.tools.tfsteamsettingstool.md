---
optionsClassName: TfsTeamSettingsToolOptions
optionsClassFullName: MigrationTools.Tools.TfsTeamSettingsToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
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
  order: 1
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
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
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsTeamSettingsToolOptions",
      "Enabled": true,
      "MigrateTeamSettings": true,
      "UpdateTeamSettings": true,
      "MigrateTeamCapacities": true,
      "UseUserMapping": false,
      "Teams": [
        "Team 1",
        "Team 2"
      ]
    }
  sampleFor: MigrationTools.Tools.TfsTeamSettingsToolOptions
description: Tool for migrating team settings including team configurations, area paths, iterations, and team-specific settings from source to target Team Foundation Server or Azure DevOps.
className: TfsTeamSettingsTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
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
  defaultValue: missing XML code comments
- parameterName: UpdateTeamSettings
  type: Boolean
  description: Reset the target team settings to match the source if the team exists
  defaultValue: false
- parameterName: UseUserMapping
  type: Boolean
  description: Use user mapping file from TfsTeamSettingsTool when matching users when migrating capacities. By default, users in source are matched in target users by current display name. When this is set to `true`, users are matched also by mapped name from user mapping file.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsTeamSettingsTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsTeamSettingsToolOptions.cs
notes:
  exists: false
  path: docs/Reference/Tools/TfsTeamSettingsTool-notes.md
  markdown: ''

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
  path: docs/Reference/Tools/TfsTeamSettingsTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsTeamSettingsTool-introduction.md
  exists: false
  markdown: ''

---