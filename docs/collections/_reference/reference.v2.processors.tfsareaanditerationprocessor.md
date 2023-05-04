---
optionsClassName: TfsAreaAndIterationProcessorOptions
optionsClassFullName: MigrationTools.Processors.TfsAreaAndIterationProcessorOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsAreaAndIterationProcessorOptions",
      "Enabled": false,
      "PrefixProjectToNodes": false,
      "NodeBasePaths": null,
      "AreaMaps": {
        "$type": "Dictionary`2"
      },
      "IterationMaps": {
        "$type": "Dictionary`2"
      },
      "ProcessorEnrichers": null,
      "SourceName": "sourceName",
      "TargetName": "targetName"
    }
  sampleFor: MigrationTools.Processors.TfsAreaAndIterationProcessorOptions
description: The `TfsAreaAndIterationProcessor` migrates all of the Area nd Iteraion paths.
className: TfsAreaAndIterationProcessor
typeName: Processors
architecture: v2
options:
- parameterName: AreaMaps
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: Enabled
  type: Boolean
  description: If set to `true` then the processor will run. Set to `false` and the processor will not run.
  defaultValue: missng XML code comments
- parameterName: IterationMaps
  type: Dictionary
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: NodeBasePaths
  type: String[]
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: PrefixProjectToNodes
  type: Boolean
  description: Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
  defaultValue: false
- parameterName: ProcessorEnrichers
  type: List
  description: List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: '`Refname` will be used in the future to allow for using named Options without the need to copy all of the options.'
  defaultValue: missng XML code comments
- parameterName: SourceName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: TargetName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/Processors/TfsAreaAndIterationProcessor/
title: TfsAreaAndIterationProcessor
categories:
- Processors
- v2
notes: ''
introduction: ''

---