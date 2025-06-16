---
optionsClassName: TfsNodeStructureToolOptions
optionsClassFullName: MigrationTools.Tools.TfsNodeStructureToolOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsNodeStructureTool": {
            "Areas": {
              "Filters": null,
              "Mappings": null
            },
            "Enabled": "True",
            "Iterations": {
              "Filters": null,
              "Mappings": null
            },
            "ReplicateAllExistingNodes": "True",
            "ShouldCreateMissingRevisionPaths": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsNodeStructureToolOptions
- name: sample
  order: 1
  description: 
  code: >-
    {
      "MigrationTools": {
        "Version": "16.0",
        "CommonTools": {
          "TfsNodeStructureTool": {
            "Areas": {
              "Filters": [
                "*\\Team 1,*\\Team 1\\**"
              ],
              "Mappings": [
                {
                  "Match": "^migrationSource1([\\\\]?.*)$",
                  "Replacement": "MigrationTest5$1"
                },
                {
                  "Match": "^Skypoint Cloud([\\\\]?.*)$",
                  "Replacement": "MigrationTest5$1"
                },
                {
                  "Match": "^7473924d-c47f-4089-8f5c-077c728b576e([\\\\]?.*)$",
                  "Replacement": "MigrationTest5$1"
                }
              ]
            },
            "Enabled": "True",
            "Iterations": {
              "Filters": [
                "*\\Sprint*",
                "*\\Sprint*\\**"
              ],
              "Mappings": [
                {
                  "Match": "^migrationSource1([\\\\]?.*)$",
                  "Replacement": "MigrationTest5$1"
                },
                {
                  "Match": "^Skypoint Cloud([\\\\]?.*)$",
                  "Replacement": "MigrationTest5$1"
                },
                {
                  "Match": "^7473924d-c47f-4089-8f5c-077c728b576e([\\\\]?.*)$",
                  "Replacement": "MigrationTest5$1"
                }
              ]
            },
            "ReplicateAllExistingNodes": "True",
            "ShouldCreateMissingRevisionPaths": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsNodeStructureToolOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "TfsNodeStructureToolOptions",
      "Enabled": true,
      "Areas": {
        "Filters": [
          "*\\Team 1,*\\Team 1\\**"
        ],
        "Mappings": [
          {
            "Match": "^migrationSource1([\\\\]?.*)$",
            "Replacement": "MigrationTest5$1"
          },
          {
            "Match": "^Skypoint Cloud([\\\\]?.*)$",
            "Replacement": "MigrationTest5$1"
          },
          {
            "Match": "^7473924d-c47f-4089-8f5c-077c728b576e([\\\\]?.*)$",
            "Replacement": "MigrationTest5$1"
          }
        ]
      },
      "Iterations": {
        "Filters": [
          "*\\Sprint*",
          "*\\Sprint*\\**"
        ],
        "Mappings": [
          {
            "Match": "^migrationSource1([\\\\]?.*)$",
            "Replacement": "MigrationTest5$1"
          },
          {
            "Match": "^Skypoint Cloud([\\\\]?.*)$",
            "Replacement": "MigrationTest5$1"
          },
          {
            "Match": "^7473924d-c47f-4089-8f5c-077c728b576e([\\\\]?.*)$",
            "Replacement": "MigrationTest5$1"
          }
        ]
      },
      "ShouldCreateMissingRevisionPaths": true,
      "ReplicateAllExistingNodes": true
    }
  sampleFor: MigrationTools.Tools.TfsNodeStructureToolOptions
description: Tool for creating missing area and iteration path nodes in the target project during migration. Configurable through TfsNodeStructureToolOptions to specify which node types to create.
className: TfsNodeStructureTool
typeName: Tools
architecture: 
options:
- parameterName: Areas
  type: NodeOptions
  description: 'Rules to apply to the Area Path. Is an object of NodeOptions e.g. { "Filters": ["*/**"], "Mappings": [{"Match": "^oldProjectName([\\\\]?.*)$", "Replacement": "targetProjectA$1"}] }'
  defaultValue: '{"Filters": [], "Mappings": []}'
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the tool will run. Set to `false` and the processor will not run.
  defaultValue: missing XML code comments
- parameterName: Iterations
  type: NodeOptions
  description: 'Rules to apply to the Iteration Path. Is an object of NodeOptions e.g. { "Filters": ["*/**"], "Mappings": [{"Match": "^oldProjectName([\\\\]?.*)$", "Replacement": "targetProjectA$1"}] }'
  defaultValue: '{"Filters": [], "Mappings": []}'
- parameterName: ReplicateAllExistingNodes
  type: Boolean
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: ShouldCreateMissingRevisionPaths
  type: Boolean
  description: When set to True the susyem will try to create any missing missing area or iteration paths from the revisions.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsNodeStructureTool.cs
optionsClassFile: src/MigrationTools.Clients.TfsObjectModel/Tools/TfsNodeStructureToolOptions.cs

redirectFrom:
- /Reference/Tools/TfsNodeStructureToolOptions/
layout: reference
toc: true
permalink: /Reference/Tools/TfsNodeStructureTool/
title: TfsNodeStructureTool
categories:
- Tools
- 
topics:
- topic: notes
  path: docs/Reference/Tools/TfsNodeStructureTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/Tools/TfsNodeStructureTool-introduction.md
  exists: false
  markdown: ''

---