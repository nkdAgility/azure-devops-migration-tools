---
optionsClassName: TfsWorkItemEmbededLinkToolOptions
optionsClassFullName: MigrationTools.Tools.TfsWorkItemEmbededLinkToolOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsWorkItemEmbededLinkTool": {
            "Enabled": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsWorkItemEmbededLinkToolOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsWorkItemEmbededLinkTool": {
            "Enabled": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsWorkItemEmbededLinkToolOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsWorkItemEmbededLinkToolOptions",
      "Enabled": true
    }
  sampleFor: MigrationTools.Tools.TfsWorkItemEmbededLinkToolOptions
description: missing XML code comments
className: TfsWorkItemEmbededLinkTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemEmbededLinkTool.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemEmbededLinkToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsWorkItemEmbededLinkToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsWorkItemEmbededLinkTool/
title: TfsWorkItemEmbededLinkTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/TfsWorkItemEmbededLinkTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/TfsWorkItemEmbededLinkTool-introduction.md
  exists: false
  markdown: ''

---