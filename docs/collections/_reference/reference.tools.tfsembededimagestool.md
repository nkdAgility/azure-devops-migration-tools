---
optionsClassName: TfsEmbededImagesToolOptions
optionsClassFullName: MigrationTools.Tools.TfsEmbededImagesToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsEmbededImagesTool": {
            "Enabled": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsEmbededImagesToolOptions
- name: sample
  order: 1
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsEmbededImagesTool": {
            "Enabled": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsEmbededImagesToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsEmbededImagesToolOptions",
      "Enabled": true
    }
  sampleFor: MigrationTools.Tools.TfsEmbededImagesToolOptions
description: missing XML code comments
className: TfsEmbededImagesTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsEmbededImagesTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsEmbededImagesToolOptions.cs
notes:
  exists: false
  path: docs/Reference/Tools/TfsEmbededImagesTool-notes.md
  markdown: ''

redirectFrom:
- /Reference/Tools/TfsEmbededImagesToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsEmbededImagesTool/
title: TfsEmbededImagesTool
categories:
- Tools
- 
topics:
- topic: notes
  path: docs/Reference/Tools/TfsEmbededImagesTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsEmbededImagesTool-introduction.md
  exists: false
  markdown: ''

---