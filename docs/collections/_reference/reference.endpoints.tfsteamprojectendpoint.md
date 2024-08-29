---
optionsClassName: TfsTeamProjectEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.TfsTeamProjectEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Endpoints": {
          "#KEY#": {
            "TfsTeamProjectEndpoint": {
              "AllowCrossProjectLinking": "False",
              "Authentication": {
                "AccessToken": "12345",
                "AuthenticationMode": "AccessToken",
                "NetworkCredentials": {
                  "Domain": "",
                  "Password": "",
                  "UserName": ""
                }
              },
              "AuthenticationMode": "AccessToken",
              "Collection": "",
              "EndpointType": "TfsTeamProjectEndpoint",
              "LanguageMaps": {
                "AreaPath": "Area",
                "IterationPath": "Iteration"
              },
              "Project": ""
            }
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
        "Endpoints": {
          "#KEY#": {
            "TfsTeamProjectEndpoint": {
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
              "EndpointType": "TfsTeamProjectEndpoint",
              "LanguageMaps": {
                "AreaPath": "Area",
                "IterationPath": "Iteration"
              },
              "Project": "migrationSource1"
            }
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
          "Password": ""
        },
        "AccessToken": "jklsadhjksahfkjsdhjksahsadjhksadhsad"
      },
      "ReflectedWorkItemIDFieldName": null,
      "AllowCrossProjectLinking": false,
      "LanguageMaps": {
        "AreaPath": "Area",
        "IterationPath": "Iteration"
      },
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
- parameterName: Authentication
  type: TfsAuthenticationOptions
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Collection
  type: Uri
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
classFile: /src/MigrationTools.Clients.TfsObjectModel/Endpoints/TfsTeamProjectEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Endpoints/TfsTeamProjectEndPointOptions.cs

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