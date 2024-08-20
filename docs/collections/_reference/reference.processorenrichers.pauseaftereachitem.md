---
optionsClassName: PauseAfterEachItemOptions
optionsClassFullName: MigrationTools.Enrichers.PauseAfterEachItemOptions
configurationSamples:
- name: confinguration.json
  description: 
  code: >-
    {
      "MigrationTools": {
        "Processors": {
          "*": {
            "Enrichers": [
              {
                "ProcessorEnricherType": "PauseAfterEachItem",
                "Enabled": false,
                "RefName": null
              }
            ]
          }
        }
      }
    }
  sampleFor: MigrationTools.Enrichers.PauseAfterEachItemOptions
- name: defaults
  description: 
  code: >-
    {
      "MigrationTools": {
        "ProcessorEnricherDefaults": {
          "PauseAfterEachItem": []
        }
      }
    }
  sampleFor: MigrationTools.Enrichers.PauseAfterEachItemOptions
- name: Classic
  description: 
  code: >-
    {
      "$type": "PauseAfterEachItemOptions",
      "Enabled": false
    }
  sampleFor: MigrationTools.Enrichers.PauseAfterEachItemOptions
description: missng XML code comments
className: PauseAfterEachItem
typeName: ProcessorEnrichers
architecture: 
options:
- parameterName: Enabled
  type: Boolean
  description: If enabled this will run this migrator
  defaultValue: true
- parameterName: RefName
  type: String
  description: For internal use
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: missng XML code comments
classFile: /src/MigrationTools/Processors/Enrichers/PauseAfterEachItem.cs
optionsClassFile: /src/MigrationTools/Processors/Enrichers/PauseAfterEachItemOptions.cs

redirectFrom:
- /Reference/ProcessorEnrichers/PauseAfterEachItemOptions/
layout: reference
toc: true
permalink: /Reference/ProcessorEnrichers/PauseAfterEachItem/
title: PauseAfterEachItem
categories:
- ProcessorEnrichers
- 
topics:
- topic: notes
  path: /ProcessorEnrichers/PauseAfterEachItem-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: /ProcessorEnrichers/PauseAfterEachItem-introduction.md
  exists: false
  markdown: ''

---