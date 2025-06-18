---
optionsClassName: TfsWorkItemEmbededLinkToolOptions
optionsClassFullName: MigrationTools.Tools.TfsWorkItemEmbededLinkToolOptions
configurationSamples:
- name: defaults
  order: 2
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
  order: 1
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
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsWorkItemEmbededLinkToolOptions",
      "Enabled": true
    }
  sampleFor: MigrationTools.Tools.TfsWorkItemEmbededLinkToolOptions
description: Tool for processing embedded links within work item fields, such as links in HTML fields and converting work item references between source and target systems.
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
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemEmbededLinkTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsWorkItemEmbededLinkToolOptions.cs
notes:
  exists: false
  path: docs/Reference/Tools/TfsWorkItemEmbededLinkTool-notes.md
  markdown: ''

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
  path: docs/Reference/Tools/TfsWorkItemEmbededLinkTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsWorkItemEmbededLinkTool-introduction.md
  exists: false
  markdown: ''

---