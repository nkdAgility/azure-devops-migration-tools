---
optionsClassName: TeamMigrationConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.TeamMigrationConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TeamMigrationConfig",
      "Enabled": false,
      "EnableTeamSettingsMigration": true,
      "FixTeamSettingsForExistingTeams": false
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.TeamMigrationConfig
description: 'Migrates Teams and Team Settings: This should be run after `NodeStructuresMigrationConfig` and before all other processors.'
className: TeamMigrationContext
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: EnableTeamSettingsMigration
  type: Boolean
  description: Migrate original team settings after their creation on target team project
  defaultValue: true
- parameterName: FixTeamSettingsForExistingTeams
  type: Boolean
  description: Reset the target team settings to match the source if the team exists
  defaultValue: true
status: preview
processingTarget: Teams
classFile: /src/VstsSyncMigrator.Core/Execution/MigrationContext/TeamMigrationContext.cs
optionsClassFile: /src/MigrationTools/_EngineV1/Configuration/Processing/TeamMigrationConfig.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/TeamMigrationContext/
title: TeamMigrationContext
categories:
- Processors
- v1
topics:
- topic: notes
  path: /docs/Reference/v1/Processors/TeamMigrationContext-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/v1/Processors/TeamMigrationContext-introduction.md
  exists: false
  markdown: ''

---