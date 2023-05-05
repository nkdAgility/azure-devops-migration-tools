---
optionsClassName: WorkItemUpdateAreasAsTagsConfig
optionsClassFullName: MigrationTools._EngineV1.Configuration.Processing.WorkItemUpdateAreasAsTagsConfig
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "WorkItemUpdateAreasAsTagsConfig",
      "Enabled": false,
      "AreaIterationPath": null
    }
  sampleFor: MigrationTools._EngineV1.Configuration.Processing.WorkItemUpdateAreasAsTagsConfig
description: A common issue with older *TFS/Azure DevOps* instances is the proliferation of `Area Paths`. With the use of `Area Path` for `Teams` and the addition of the `Node Name` column option these extensive tag hierarchies should instad be moved to tags.
className: WorkItemUpdateAreasAsTagsContext
typeName: Processors
architecture: v1
options:
- parameterName: AreaIterationPath
  type: String
  description: This is a required parameter. That define the root path of the iteration. To get the full path use `\`
  defaultValue: '\'
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
status: Beta
processingTarget: Work Item

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v1/Processors/WorkItemUpdateAreasAsTagsContext/
title: WorkItemUpdateAreasAsTagsContext
categories:
- Processors
- v1
notes: ''
introduction: ''

---