---
optionsClassName: FileSystemWorkItemEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "FileSystemWorkItemEndpointOptions",
      "FileStore": null
    }
  sampleFor: MigrationTools.Endpoints.FileSystemWorkItemEndpointOptions
description: missing XML code comments
className: FileSystemWorkItemEndpoint
typeName: Endpoints
architecture: 
options:
- parameterName: FileStore
  type: String
  description: Path to the directory where work item data will be stored or read from. This should be a valid local file system path with appropriate read/write permissions.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.FileSystem/Endpoints/FileSystemWorkItemEndpoint.cs
optionsClassFile: src/MigrationTools.Clients.FileSystem/Endpoints/FileSystemWorkItemEndpointOptions.cs

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
  path: docs/Reference/Endpoints/FileSystemWorkItemEndpoint-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Endpoints/FileSystemWorkItemEndpoint-introduction.md
  exists: false
  markdown: ''

---