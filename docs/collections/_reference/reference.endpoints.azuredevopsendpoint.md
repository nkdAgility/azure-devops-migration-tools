---
optionsClassName: AzureDevOpsEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.AzureDevOpsEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Endpoints.AzureDevOpsEndpointOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "Endpoints": {
          "#KEY#": {
            "AzureDevOpsEndpoint": {
              "AccessToken": "jklsadhjksahfkjsdhjksahsadjhksadhsad",
              "AuthenticationMode": "AccessToken",
              "EndpointType": "AzureDevOpsEndpoint",
              "Organisation": "https://dev.azure.com/xxx/",
              "Project": "myProject"
            }
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
      "AuthenticationMode": "AccessToken",
      "AccessToken": "jklsadhjksahfkjsdhjksahsadjhksadhsad",
      "Organisation": "https://dev.azure.com/xxx/",
      "Project": "myProject",
      "ReflectedWorkItemIdField": null,
      "EndpointEnrichers": null
    }
  sampleFor: MigrationTools.Endpoints.AzureDevOpsEndpointOptions
description: missing XML code comments
className: AzureDevOpsEndpoint
typeName: Endpoints
architecture: 
options:
- parameterName: AccessToken
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: AuthenticationMode
  type: AuthenticationMode
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: EndpointEnrichers
  type: List
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Organisation
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Project
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: ReflectedWorkItemIdField
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
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