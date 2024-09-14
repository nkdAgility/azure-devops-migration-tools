---
optionsClassName: TfsEndpointOptions
optionsClassFullName: MigrationTools.Endpoints.TfsEndpointOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "Endpoints": {
          "#KEY#": {
            "TfsEndpoint": {
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
  sampleFor: MigrationTools.Endpoints.TfsEndpointOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "Endpoints": {
          "#KEY#": {
            "TfsEndpoint": {
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
  sampleFor: MigrationTools.Endpoints.TfsEndpointOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsEndpointOptions",
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
      "ReflectedWorkItemIdField": null,
      "AllowCrossProjectLinking": false,
      "LanguageMaps": {
        "AreaPath": "Area",
        "IterationPath": "Iteration"
      },
      "EndpointEnrichers": null
    }
  sampleFor: MigrationTools.Endpoints.TfsEndpointOptions
description: missing XML code comments
className: TfsEndpoint
typeName: Endpoints
architecture: 
options:
- parameterName: AllowCrossProjectLinking
  type: Boolean
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Authentication
  type: TfsAuthenticationOptions
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: Collection
  type: Uri
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: EndpointEnrichers
  type: List
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
classFile: /src/MigrationTools.Clients.TfsObjectModel/EndPoints/TfsEndpoint.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/EndPoints/TfsEndpointOptions.cs

redirectFrom:
- /Reference/Endpoints/TfsEndpointOptions/
layout: reference
toc: true
permalink: /Reference/Endpoints/TfsEndpoint/
title: TfsEndpoint
categories:
- Endpoints
- 
topics:
- topic: notes
  path: /docs/Reference/Endpoints/TfsEndpoint-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Endpoints/TfsEndpoint-introduction.md
  exists: false
  markdown: ''

---