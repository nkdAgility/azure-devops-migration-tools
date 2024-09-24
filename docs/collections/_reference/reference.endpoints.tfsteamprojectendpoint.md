---
optionsClassName: TfsTeamProjectEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "Endpoints": {
          "#KEY#": {
            "EndpointType": "TfsTeamProjectEndpoint",
            "AllowCrossProjectLinking": "False",
            "Authentication": {
              "AccessToken": "",
              "AuthenticationMode": "AccessToken",
              "NetworkCredentials": {
                "Domain": "",
                "Password": "",
                "UserName": ""
              }
            },
            "AuthenticationMode": "AccessToken",
            "Collection": "",
            "LanguageMaps": {
              "AreaPath": "Area",
              "IterationPath": "Iteration"
            },
            "Project": "",
            "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId"
          }
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "Endpoints": {
          "#KEY#": {
            "EndpointType": "TfsTeamProjectEndpoint",
            "AllowCrossProjectLinking": "False",
            "Authentication": {
              "AccessToken": "jklsadhjksahfkjsdhjksahsadjhksadhsad",
              "AuthenticationMode": "AccessToken",
              "NetworkCredentials": {
                "Domain": "",
                "Password": "",
                "UserName": ""
              }
            },
            "Collection": "https://dev.azure.com/nkdagility-preview/",
            "Project": "migrationSource1",
            "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId"
          }
        }
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsTeamProjectEndpointOptions",
      "Collection": "https://dev.azure.com/nkdagility-preview/",
      "Project": "migrationSource1",
      "Authentication": {
        "AuthenticationMode": "AccessToken",
        "NetworkCredentials": {
          "Domain": "",
          "UserName": "",
          "Password": "** removed as a secret ***"
        },
        "AccessToken": "** removed as a secret ***"
      },
      "ReflectedWorkItemIdField": "Custom.ReflectedWorkItemId",
      "LanguageMaps": {
        "AreaPath": "Area",
        "IterationPath": "Iteration"
      }
    }
  sampleFor: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
description: missing XML code comments
className: TfsTeamProjectEndpoint
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
- parameterName: ReflectedWorkItemIdField
  type: String
  description: missing XML code comments
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/EndPoints/TfsTeamProjectEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/EndPoints/TfsTeamProjectEndPointOptions.cs

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