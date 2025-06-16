---
optionsClassName: FieldClearMapOptions
optionsClassFullName: MigrationTools.Tools.FieldClearMapOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "FieldClearMapOptions",
      "targetField": null,
      "ApplyTo": []
    }
  sampleFor: MigrationTools.Tools.FieldClearMapOptions
description: Clears a target field by setting its value to null, useful for removing data from specific fields during migration.
className: FieldClearMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: A list of Work Item Types that this Field Map will apply to. If the list is empty it will apply to all Work Item Types. You can use "*" to apply to all Work Item Types.
  defaultValue: missing XML code comments
- parameterName: targetField
  type: String
  description: Gets or sets the name of the target field to be cleared/set to null during work item migration.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldClearMap.cs
optionsClassFile: src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldClearMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/FieldClearMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldClearMap/
title: FieldClearMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: docs/Reference/FieldMaps/FieldClearMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/FieldMaps/FieldClearMap-introduction.md
  exists: false
  markdown: ''

---