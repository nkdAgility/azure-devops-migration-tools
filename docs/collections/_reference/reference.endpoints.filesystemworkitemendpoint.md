---
optionsClassName: FileSystemWorkItemEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "FileSystemWorkItemEndpoint": []
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "EndpointDefaults": {
          "FileSystemWorkItemEndpoint": []
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "FileSystemWorkItemEndpointOptions",
      "Enabled": false,
      "FileStore": null,
      "Name": null,
      "EndpointEnrichers": null
    }
  sampleFor: MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions
description: missng XML code comments
className: FileSystemWorkItemEndpoint
typeName: Endpoints
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: EndpointEnrichers
  type: List
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: FileStore
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Name
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.FileSystem/Endpoints/FileSystemWorkItemEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.FileSystem/Endpoints/FileSystemWorkItemEndpointOptions.cs

redirectFrom:
- /Reference/Endpoints/FileSystemWorkItemEndpointOptions/
layout: reference
toc: true
permalink: /Reference/Endpoints/FileSystemWorkItemEndpoint/
title: FileSystemWorkItemEndpoint
categories:
- Endpoints
- 
topics:
- topic: notes
  path: /docs/Reference/Endpoints/FileSystemWorkItemEndpoint-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Endpoints/FileSystemWorkItemEndpoint-introduction.md
  exists: false
  markdown: ''

---