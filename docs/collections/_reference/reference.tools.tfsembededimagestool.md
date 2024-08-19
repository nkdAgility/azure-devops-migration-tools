---
optionsClassName: TfsEmbededImagesToolOptions
optionsClassFullName: MigrationTools.Tools.TfsEmbededImagesToolOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsEmbededImagesTool": {
            "Enabled": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsEmbededImagesToolOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TfsEmbededImagesToolOptions",
      "Enabled": true
    }
  sampleFor: MigrationTools.Tools.TfsEmbededImagesToolOptions
description: missng XML code comments
className: TfsEmbededImagesTool
typeName: Tools
architecture: v1
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsEmbededImagesTool.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsEmbededImagesToolOptions.cs

redirectFrom:
- /Reference/v1/Tools/TfsEmbededImagesToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsEmbededImagesTool/
title: TfsEmbededImagesTool
categories:
- Tools
- v1
topics:
- topic: notes
  path: /Tools/TfsEmbededImagesTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Tools/TfsEmbededImagesTool-introduction.md
  exists: false
  markdown: ''

---