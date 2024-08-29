---
optionsClassName: TfsMigrationClient
optionsClassFullName: MigrationTools._EngineV1.Clients.TfsMigrationClient
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "TfsMigrationClient": []
        }
      }
    }
  sampleFor: MigrationTools._EngineV1.Clients.TfsMigrationClient
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "TfsMigrationClient": []
        }
      }
    }
  sampleFor: MigrationTools._EngineV1.Clients.TfsMigrationClient
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsMigrationClient",
      "Enabled": false,
      "TfsConfig": null,
      "Config": null,
      "WorkItems": null,
      "TestPlans": null,
      "Credentials": {
        "PromptType": 0,
        "Federated": null,
        "Windows": {
          "Credentials": {
            "UserName": "",
            "Password": "",
            "SecurePassword": {
              "Length": 0
            },
            "Domain": ""
          },
          "CredentialType": 0,
          "UseDefaultCredentials": true
        },
        "Storage": null
      },
      "InternalCollection": null,
      "Name": null,
      "EndpointEnrichers": null
    }
  sampleFor: MigrationTools._EngineV1.Clients.TfsMigrationClient
description: missng XML code comments
className: TfsMigrationClient
typeName: Endpoints
architecture: 
options:
- parameterName: Config
  type: IEndpointOptions
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Credentials
  type: VssCredentials
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
- parameterName: InternalCollection
  type: Object
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Name
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TestPlans
  type: ITestPlanMigrationClient
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TfsConfig
  type: TfsTeamProjectEndpointOptions
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkItems
  type: IWorkItemMigrationClient
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Clients/TfsMigrationClient.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Clients/TfsMigrationClient.cs

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/Endpoints/TfsMigrationClient/
title: TfsMigrationClient
categories:
- Endpoints
- 
topics:
- topic: notes
  path: /docs/Reference/Endpoints/TfsMigrationClient-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Endpoints/TfsMigrationClient-introduction.md
  exists: false
  markdown: ''

---