---
optionsClassName: TfsWorkItemEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.TfsWorkItemEndpointOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Endpoints.TfsWorkItemEndpointOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Endpoints.TfsWorkItemEndpointOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsWorkItemEndpointOptions",
      "Collection": null,
      "Project": null,
      "Query": null,
      "Authentication": null,
      "ReflectedWorkItemIdField": null,
      "LanguageMaps": {
        "AreaPath": "Area",
        "IterationPath": "Iteration"
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsWorkItemEndpointOptions
description: missing XML code comments
className: TfsWorkItemEndpoint
typeName: Endpoints
architecture: 
options:
- parameterName: Authentication
  type: TfsAuthenticationOptions
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Collection
  type: Uri
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: LanguageMaps
  type: TfsLanguageMapOptions
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Project
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Query
  type: QueryOptions
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: ReflectedWorkItemIdField
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/EndPoints/TfsWorkItemEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/EndPoints/TfsWorkItemEndpointOptions.cs

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
  path: /docs/Reference/Endpoints/TfsWorkItemEndpoint-notes.md
  exists: true
  markdown: >+
    The Work Item endpoint is super awesome.


    |Client  | WriteTo/ReadFrom | Endpoint | Data Target | Description |

    |:-:|:-:|:-:|:-:|:-:|

    AzureDevops.ObjectModel | Tfs Object Model | `TfsWorkItemEndPoint` | WorkItems | TBA

    AzureDevops.Rest | Azure DevOps REST | ?

    FileSystem | Local Files | `FileSystemWorkItemEndpoint` | WorkItems | TBA

- topic: introduction
  path: /docs/Reference/Endpoints/TfsWorkItemEndpoint-introduction.md
  exists: false
  markdown: ''

---