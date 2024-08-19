---
optionsClassName: TfsUserMappingToolOptions
optionsClassFullName: MigrationTools.Tools.TfsUserMappingToolOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsUserMappingTool": {
            "Enabled": "False",
            "IdentityFieldsToCheck": {
              "0": "System.AssignedTo",
              "1": "System.ChangedBy",
              "2": "System.CreatedBy",
              "3": "Microsoft.VSTS.Common.ActivatedBy",
              "4": "Microsoft.VSTS.Common.ResolvedBy",
              "5": "Microsoft.VSTS.Common.ClosedBy"
            },
            "UserMappingFile": "C:\\temp\\userExport.json"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsUserMappingToolOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TfsUserMappingToolOptions",
      "IdentityFieldsToCheck": [
        "System.AssignedTo",
        "System.ChangedBy",
        "System.CreatedBy",
        "Microsoft.VSTS.Common.ActivatedBy",
        "Microsoft.VSTS.Common.ResolvedBy",
        "Microsoft.VSTS.Common.ClosedBy"
      ],
      "UserMappingFile": "C:\\temp\\userExport.json",
      "Enabled": false
    }
  sampleFor: MigrationTools.Tools.TfsUserMappingToolOptions
description: The TfsUserMappingTool is used to map users from the source to the target system. Run it with the ExportUsersForMappingContext to create a mapping file then with WorkItemMigrationContext to use the mapping file to update the users in the target system as you migrate the work items.
className: TfsUserMappingTool
typeName: Tools
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: IdentityFieldsToCheck
  type: List
  description: This is a list of the Identiy fields in the Source to check for user mapping purposes. You should list all identiy fields that you wan to map.
  defaultValue: missng XML code comments
- parameterName: UserMappingFile
  type: String
  description: This is the file that will be used to export or import the user mappings. Use the ExportUsersForMapping processor to create the file.
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsUserMappingTool.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsUserMappingToolOptions.cs

redirectFrom:
- /Reference/v1/Tools/TfsUserMappingToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsUserMappingTool/
title: TfsUserMappingTool
categories:
- Tools
- v1
topics:
- topic: notes
  path: /Tools/TfsUserMappingTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Tools/TfsUserMappingTool-introduction.md
  exists: false
  markdown: ''

---