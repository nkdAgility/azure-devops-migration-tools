---
optionsClassName: TfsUserMappingToolOptions
optionsClassFullName: MigrationTools.Tools.TfsUserMappingToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.TfsUserMappingToolOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.TfsUserMappingToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsUserMappingToolOptions",
      "Enabled": false,
      "IdentityFieldsToCheck": null,
      "UserMappingFile": null,
      "MatchUsersByEmail": false
    }
  sampleFor: MigrationTools.Tools.TfsUserMappingToolOptions
description: The TfsUserMappingTool is used to map users from the source to the target system. Run it with the ExportUsersForMappingContext to create a mapping file then with WorkItemMigrationContext to use the mapping file to update the users in the target system as you migrate the work items.
className: TfsUserMappingTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: IdentityFieldsToCheck
  type: List
  description: This is a list of the Identiy fields in the Source to check for user mapping purposes. You should list all identiy fields that you want to map.
  defaultValue: missing XML code comments
- parameterName: MatchUsersByEmail
  type: Boolean
  description: By default, users in source are mapped to target users by their display name. If this is set to true, then the users will be mapped by their email address first. If no match is found, then the display name will be used.
  defaultValue: missing XML code comments
- parameterName: UserMappingFile
  type: String
  description: This is the file that will be used to export or import the user mappings. Use the ExportUsersForMapping processor to create the file.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsUserMappingTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsUserMappingToolOptions.cs
notes:
  exists: false
  path: docs/Reference/Tools/TfsUserMappingTool-notes.md
  markdown: ''
topics:
- topic: notes
  path: docs/Reference/Tools/TfsUserMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsUserMappingTool-introduction.md
  exists: false
  markdown: ''

redirectFrom:
- /Reference/Tools/TfsUserMappingToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsUserMappingTool/
title: TfsUserMappingTool
categories:
- Tools
- 
topics:
- topic: notes
  path: docs/Reference/Tools/TfsUserMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsUserMappingTool-introduction.md
  exists: false
  markdown: ''

---