---
optionsClassName: MultiValueConditionalMapOptions
optionsClassFullName: MigrationTools.Tools.MultiValueConditionalMapOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.MultiValueConditionalMapOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.MultiValueConditionalMapOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "MultiValueConditionalMapOptions",
      "sourceFieldsAndValues": null,
      "targetFieldsAndValues": null,
      "ApplyTo": []
    }
  sampleFor: MigrationTools.Tools.MultiValueConditionalMapOptions
description: missing XML code comments
className: MultiValueConditionalMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: A list of Work Item Types that this Field Map will apply to. If the list is empty it will apply to all Work Item Types. You can use "*" to apply to all Work Item Types.
  defaultValue: missing XML code comments
- parameterName: sourceFieldsAndValues
  type: Dictionary
  description: missing XML code comments
  defaultValue: missing XML code comments
- parameterName: targetFieldsAndValues
  type: Dictionary
  description: missing XML code comments
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/MultiValueConditionalMap.cs
optionsClassFile: src/MigrationTools/Tools/FieldMappingTool/FieldMaps/MultiValueConditionalMapOptions.cs

redirectFrom:
- /Reference/FieldMaps/MultiValueConditionalMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/MultiValueConditionalMap/
title: MultiValueConditionalMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: docs/Reference/FieldMaps/MultiValueConditionalMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/FieldMaps/MultiValueConditionalMap-introduction.md
  exists: false
  markdown: ''

---