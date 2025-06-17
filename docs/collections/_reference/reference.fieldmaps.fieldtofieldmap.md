---
optionsClassName: FieldToFieldMapOptions
optionsClassFullName: MigrationTools.Tools.FieldToFieldMapOptions
configurationSamples:
- name: defaults
  order: 2
  description: 
  code: There are no defaults! Check the sample for options!
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
- name: sample
  order: 1
  description: 
  code: There is no sample, but you can check the classic below for a general feel.
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
- name: classic
  order: 3
  description: 
  code: >-
    {
      "$type": "FieldToFieldMapOptions",
      "sourceField": null,
      "targetField": null,
      "defaultValue": null,
      "ApplyTo": []
    }
  sampleFor: MigrationTools.Tools.FieldToFieldMapOptions
description: Maps the value from a source field to a target field directly, with optional default value substitution for empty or null values.
className: FieldToFieldMap
typeName: FieldMaps
architecture: 
options:
- parameterName: ApplyTo
  type: List
  description: A list of Work Item Types that this Field Map will apply to. If the list is empty it will apply to all Work Item Types. You can use "*" to apply to all Work Item Types.
  defaultValue: missing XML code comments
- parameterName: defaultValue
  type: String
  description: Gets or sets the default value to use when the source field is empty or null.
  defaultValue: missing XML code comments
- parameterName: sourceField
  type: String
  description: Gets or sets the name of the source field to copy data from during migration.
  defaultValue: missing XML code comments
- parameterName: targetField
  type: String
  description: Gets or sets the name of the target field to copy data to during migration.
  defaultValue: missing XML code comments
status: missing XML code comments
processingTarget: missing XML code comments
classFile: src/MigrationTools.Clients.TfsObjectModel/Tools/FieldMappingTool/FieldMaps/FieldToFieldMap.cs
optionsClassFile: src/MigrationTools/Tools/FieldMappingTool/FieldMaps/FieldtoFieldMapOptions.cs
notes:
  exists: false
  path: docs/Reference/FieldMaps/FieldToFieldMap-notes.md
  markdown: ''

redirectFrom:
- /Reference/FieldMaps/FieldToFieldMapOptions/
layout: reference
toc: true
permalink: /Reference/FieldMaps/FieldToFieldMap/
title: FieldToFieldMap
categories:
- FieldMaps
- 
topics:
- topic: notes
  path: docs/Reference/FieldMaps/FieldToFieldMap-notes.md
  exists: false
  markdown: ''
- topic: introduction
  path: docs/Reference/FieldMaps/FieldToFieldMap-introduction.md
  exists: false
  markdown: ''

---