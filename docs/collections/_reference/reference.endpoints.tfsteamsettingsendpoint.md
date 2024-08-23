---
optionsClassName: TfsTeamSettingsEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.TfsTeamSettingsEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "TfsTeamSettingsEndpoint": []
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamSettingsEndpointOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "TfsTeamSettingsEndpoint": []
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamSettingsEndpointOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsTeamSettingsEndpointOptions",
      "Organisation": null,
      "Project": null,
      "Enabled": false,
      "AuthenticationMode": "AccessToken",
      "AccessToken": null,
      "ReflectedWorkItemIdField": null,
      "LanguageMaps": null,
      "Name": null,
      "EndpointEnrichers": null
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamSettingsEndpointOptions
description: missng XML code comments
className: TfsTeamSettingsEndpoint
typeName: Endpoints
architecture: 
options:
- parameterName: AccessToken
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: AuthenticationMode
  type: AuthenticationMode
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: EndpointEnrichers
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: LanguageMaps
  type: TfsLanguageMapOptions
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Name
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Organisation
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Project
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ReflectedWorkItemIdField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Endpoints/TfsTeamSettingsEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Endpoints/TfsTeamSettingsEndpointOptions.cs

redirectFrom:
- /Reference/Endpoints/TfsTeamSettingsEndpointOptions/
layout: reference
toc: true
permalink: /Reference/Endpoints/TfsTeamSettingsEndpoint/
title: TfsTeamSettingsEndpoint
categories:
- Endpoints
- 
topics:
- topic: notes
  path: /docs/Reference/Endpoints/TfsTeamSettingsEndpoint-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Endpoints/TfsTeamSettingsEndpoint-introduction.md
  exists: false
  markdown: ''

---