---
optionsClassName: ExportProfilePictureFromADConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.ExportProfilePictureFromADConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "ExportProfilePictureFromADConfig",
      "Enabled": false,
      "Domain": null,
      "Username": null,
      "Password": null,
      "PictureEmpIDFormat": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.ExportProfilePictureFromADConfig
description: Downloads corporate images and updates TFS/Azure DevOps profiles
className: ExportProfilePictureFromADContext
typeName: Processors
architecture: v1
options:
- parameterName: Domain
  type: String
  description: The source domain where the pictures should be exported.
  defaultValue: String.Empty
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Password
  type: String
  description: The password of the user that is used to export the pictures.
  defaultValue: String.Empty
- parameterName: PictureEmpIDFormat
  type: String
  description: 'TODO: You wpuld need to customise this for your system. Clone repo and run in Debug'
  defaultValue: String.Empty
- parameterName: Username
  type: String
  description: The user name of the user that is used to export the pictures.
  defaultValue: String.Empty

redirectFrom: []
layout: reference
toc: true
permalink: /Reference2/v1/Processors/ExportProfilePictureFromADContext/
title: ExportProfilePictureFromADContext
categories:
- Processors
- v1

---