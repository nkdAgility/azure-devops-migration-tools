---
optionsClassName: TfsTeamProjectEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "TfsTeamProjectEndpoint": []
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "TfsTeamProjectEndpoint": []
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsTeamProjectEndpointOptions",
      "Enabled": false,
      "Collection": null,
      "Project": null,
      "ReflectedWorkItemIDFieldName": null,
      "AllowCrossProjectLinking": false,
      "AuthenticationMode": "AccessToken",
      "PersonalAccessToken": null,
      "PersonalAccessTokenVariableName": null,
      "LanguageMaps": null,
      "CollectionName": "https://dev.azure.com/sampleAccount",
      "Name": null,
      "EndpointEnrichers": null
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
description: missng XML code comments
className: TfsTeamProjectEndpoint
typeName: Endpoints
architecture: 
options:
- parameterName: AllowCrossProjectLinking
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: AuthenticationMode
  type: AuthenticationMode
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Collection
  type: Uri
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: CollectionName
  type: String
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
- parameterName: PersonalAccessToken
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: PersonalAccessTokenVariableName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Project
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ReflectedWorkItemIDFieldName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Endpoints/TfsTeamProjectEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Endpoints/TfsTeamProjectEndpointOptions.cs

redirectFrom:
- /Reference/Endpoints/TfsTeamProjectEndpointOptions/
layout: reference
toc: true
permalink: /Reference/Endpoints/TfsTeamProjectEndpoint/
title: TfsTeamProjectEndpoint
categories:
- Endpoints
- 
topics:
- topic: notes
  path: /docs/Reference/Endpoints/TfsTeamProjectEndpoint-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Endpoints/TfsTeamProjectEndpoint-introduction.md
  exists: false
  markdown: ''

---