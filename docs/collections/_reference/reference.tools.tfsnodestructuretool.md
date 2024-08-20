---
optionsClassName: TfsNodeStructureToolOptions
optionsClassFullName: MigrationTools.Tools.TfsNodeStructureToolOptions
configurationSamples:
- name: confinguration.json
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsNodeStructureTool": {
            "NodeBasePaths": null,
            "AreaMaps": {
              "^migrationSource1([\\\\]?.*)$": "MigrationTest5$1"
            },
            "IterationMaps": {
              "^migrationSource1([\\\\]?.*)$": "MigrationTest5$1"
            },
            "ShouldCreateMissingRevisionPaths": true,
            "ReplicateAllExistingNodes": true
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsNodeStructureToolOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "CommonTools": {
          "TfsNodeStructureTool": {
            "AreaMaps": {
              "^migrationSource1([\\\\]?.*)$": "MigrationTest5$1"
            },
            "Enabled": "True",
            "IterationMaps": {
              "^migrationSource1([\\\\]?.*)$": "MigrationTest5$1"
            },
            "NodeBasePaths": null,
            "ReplicateAllExistingNodes": "True",
            "ShouldCreateMissingRevisionPaths": "True"
          }
        }
      }
    }
  sampleFor: MigrationTools.Tools.TfsNodeStructureToolOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "TfsNodeStructureToolOptions",
      "NodeBasePaths": null,
      "AreaMaps": {
        "$type": "Dictionary`2",
        "^migrationSource1([\\\\]?.*)$": "MigrationTest5$1"
      },
      "IterationMaps": {
        "$type": "Dictionary`2",
        "^migrationSource1([\\\\]?.*)$": "MigrationTest5$1"
      },
      "ShouldCreateMissingRevisionPaths": true,
      "ReplicateAllExistingNodes": true
    }
  sampleFor: MigrationTools.Tools.TfsNodeStructureToolOptions
description: The TfsNodeStructureToolEnricher is used to create missing nodes in the target project. To configure it add a `TfsNodeStructureToolOptions` section to `CommonEnrichersConfig` in the config file. Otherwise defaults will be applied.
className: TfsNodeStructureTool
typeName: Tools
architecture: 
options:
- parameterName: AreaMaps
  type: Dictionary
  description: Remapping rules for area paths, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`, that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied.
  defaultValue: '{}'
- parameterName: IterationMaps
  type: Dictionary
  description: Remapping rules for iteration paths, implemented with regular expressions. The rules apply with a higher priority than the `PrefixProjectToNodes`, that is, if no rule matches the path and the `PrefixProjectToNodes` option is enabled, then the old `PrefixProjectToNodes` behavior is applied.
  defaultValue: '{}'
- parameterName: NodeBasePaths
  type: String[]
  description: The root paths of the Ares / Iterations you want migrate. See [NodeBasePath Configuration](#nodebasepath-configuration)
  defaultValue: '["/"]'
- parameterName: ReplicateAllExistingNodes
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: ShouldCreateMissingRevisionPaths
  type: Boolean
  description: When set to True the susyem will try to create any missing missing area or iteration paths from the revisions.
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsNodeStructureTool.cs
optionsClassFile: /src/MigrationTools.Clients.AzureDevops.ObjectModel/Tools/TfsNodeStructureToolOptions.cs

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
  path: /Tools/TfsNodeStructureTool-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /Tools/TfsNodeStructureTool-introduction.md
  exists: false
  markdown: ''

---