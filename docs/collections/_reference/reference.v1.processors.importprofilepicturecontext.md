---
optionsClassName: ImportProfilePictureConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.ImportProfilePictureConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "ImportProfilePictureConfig",
      "Enabled": false
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.ImportProfilePictureConfig
description: Downloads corporate images and updates TFS/Azure DevOps profiles
className: ImportProfilePictureContext
typeName: Processors
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
status: alpha
processingTarget: Profiles

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/ImportProfilePictureContext/
title: ImportProfilePictureContext
categories:
- Processors
- v1
notes: ''
introduction: ''

---