---
optionsClassName: TfsValidateRequiredFieldToolOptions
optionsClassFullName: MigrationTools.Tools.TfsValidateRequiredFieldToolOptions
configurationSamples:
- name: confinguration.json
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsValidateRequiredFieldTool": {
            "Enabled": false
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsValidateRequiredFieldToolOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsValidateRequiredFieldTool": []
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsValidateRequiredFieldToolOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TfsValidateRequiredFieldToolOptions",
      "Enabled": false
    }
  sampleFor: MigrationTools.Tools.TfsValidateRequiredFieldToolOptions
description: missng XML code comments
className: TfsValidateRequiredFieldTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsValidateRequiredFieldTool.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsValidateRequiredFieldToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsValidateRequiredFieldToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsValidateRequiredFieldTool/
title: TfsValidateRequiredFieldTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/TfsValidateRequiredFieldTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/TfsValidateRequiredFieldTool-introduction.md
  exists: false
  markdown: ''

---