---
optionsClassName: TfsGitRepositoryToolOptions
optionsClassFullName: MigrationTools.Tools.TfsGitRepositoryToolOptions
configurationSamples:
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsGitRepositoryTool": {
            "Enabled": "True",
            "Mappings": null
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
- name: sample
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsGitRepositoryTool": {
            "Enabled": "True",
            "Mappings": {
              "RepoInSource": "RepoInTarget"
            }
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
- name: classic
  description: 
  code: >-
    {
      "$type": "TfsGitRepositoryToolOptions",
      "Enabled": true,
      "Mappings": {
        "RepoInSource": "RepoInTarget"
      }
    }
  sampleFor: MigrationTools.Tools.TfsGitRepositoryToolOptions
description: missng XML code comments
className: TfsGitRepositoryTool
typeName: Tools
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: Mappings
  type: Dictionary
  description: List of work item mappings.
  defaultValue: '{}'
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/TfsGitRepositoryTool.cs
optionsClassFile: /src/MigrationTools.Clients.TfsObjectModel/Tools/TfsGitRepositoryToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsGitRepositoryToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsGitRepositoryTool/
title: TfsGitRepositoryTool
categories:
- Tools
- 
topics:
- topic: notes
  path: /docs/Reference/Tools/TfsGitRepositoryTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /docs/Reference/Tools/TfsGitRepositoryTool-introduction.md
  exists: false
  markdown: ''

---