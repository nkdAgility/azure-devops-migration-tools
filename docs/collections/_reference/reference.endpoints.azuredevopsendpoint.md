---
optionsClassName: AzureDevOpsEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.AzureDevOpsEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Endpoints": {
          "#KEY#": {
            "AzureDevOpsEndpoint": []
          }
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.AzureDevOpsEndpointOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Endpoints": {
          "#KEY#": {
            "AzureDevOpsEndpoint": []
          }
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.AzureDevOpsEndpointOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "AzureDevOpsEndpointOptions",
      "Enabled": false,
      "AuthenticationMode": 0,
      "AccessToken": null,
      "Organisation": null,
      "Project": null,
      "ReflectedWorkItemIdField": null,
      "EndpointEnrichers": null
    }
  sampleFor: MigrationTools.Endpoints.AzureDevOpsEndpointOptions
description: missng XML code comments
className: AzureDevOpsEndpoint
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
classFile: /src/MigrationTools.Clients.AzureDevops.Rest/Endpoints/AzureDevOpsEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.Rest/Endpoints/AzureDevOpsEndpointOptions.cs

redirectFrom:
- /Reference/Endpoints/AzureDevOpsEndpointOptions/
layout: reference
toc: true
permalink: /Reference/Endpoints/AzureDevOpsEndpoint/
title: AzureDevOpsEndpoint
categories:
- Endpoints
- 
topics:
- topic: notes
  path: /docs/Reference/Endpoints/AzureDevOpsEndpoint-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Endpoints/AzureDevOpsEndpoint-introduction.md
  exists: false
  markdown: ''

---