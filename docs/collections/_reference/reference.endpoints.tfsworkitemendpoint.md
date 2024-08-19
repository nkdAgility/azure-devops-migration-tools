---
optionsClassName: TfsWorkItemEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.TfsWorkItemEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "TfsWorkItemEndpoint": {}
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsWorkItemEndpointOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TfsWorkItemEndpointOptions",
      "Organisation": null,
      "Project": null,
      "Query": null,
      "AuthenticationMode": "AccessToken",
      "AccessToken": null,
      "ReflectedWorkItemIdField": null,
      "LanguageMaps": null,
      "Name": null,
      "EndpointEnrichers": null
    }
  sampleFor: MigrationTools.Endpoints.TfsWorkItemEndpointOptions
description: missng XML code comments
className: TfsWorkItemEndpoint
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
- parameterName: Query
  type: QueryOptions
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ReflectedWorkItemIdField
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Endpoints/TfsWorkItemEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Endpoints/TfsWorkItemEndpointOptions.cs

redirectFrom:
- /Reference/Endpoints/TfsWorkItemEndpointOptions/
layout: reference
toc: true
permalink: /Reference/Endpoints/TfsWorkItemEndpoint/
title: TfsWorkItemEndpoint
categories:
- Endpoints
- 
topics:
- topic: notes
  path: /Endpoints/TfsWorkItemEndpoint-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Endpoints/TfsWorkItemEndpoint-introduction.md
  exists: false
  markdown: ''

---