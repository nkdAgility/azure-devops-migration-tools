---
optionsClassName: TfsRevisionManagerToolOptions
optionsClassFullName: MigrationTools.Tools.TfsRevisionManagerToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsRevisionManagerTool": {
            "Enabled": "True",
            "MaxRevisions": "0",
            "ReplayRevisions": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsRevisionManagerToolOptions
- name: sample
  order: 1
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsRevisionManagerTool": {
            "Enabled": "True",
            "MaxRevisions": "0",
            "ReplayRevisions": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsRevisionManagerToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsRevisionManagerToolOptions",
      "Enabled": true,
      "ReplayRevisions": true,
      "MaxRevisions": 0
    }
  sampleFor: MigrationTools.Tools.TfsRevisionManagerToolOptions
description: The TfsRevisionManagerTool manipulates the revisions of a work item to reduce the number of revisions that are migrated.
className: TfsRevisionManagerTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: MaxRevisions
  type: Int32
  description: Sets the maximum number of revisions that will be migrated. "First + Last N = Max". If this was set to 5 and there were 10 revisions you would get the first 1 (creation) and the latest 4 migrated.
  defaultValue: 0
- parameterName: ReplayRevisions
  type: Boolean
  description: You can choose to migrate the tip only (a single write) or all of the revisions (many writes). If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true`.
  defaultValue: true
status: missing XML code comments
processingTarget: missing XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/TfsRevisionManagerTool.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/TfsRevisionManagerToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsRevisionManagerToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsRevisionManagerTool/
title: TfsRevisionManagerTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/TfsRevisionManagerTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/TfsRevisionManagerTool-introduction.md
  exists: false
  markdown: ''

---