---
optionsClassName: TfsWorkItemAttachmentEnricherOptions
optionsClassFullName: MigrationTools.EndpointEnrichers.TfsWorkItemAttachmentEnricherOptions
configurationSamples:
- name: default
  description: 
  code: >-
    {
      "$type": "TfsWorkItemAttachmentEnricherOptions",
      "Enabled": true,
      "WorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
      "MaxSize": 480000000
    }
  sampleFor: MigrationTools.EndpointEnrichers.TfsWorkItemAttachmentEnricherOptions
description: The `TfsWorkItemAttachmentEnricher` processes the attachements for a specific work item.
className: TfsWorkItemAttachmentEnricher
typeName: EndpointEnrichers
architecture: v2
options:
- parameterName: Enabled
  type: Boolean
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: MaxSize
  type: Int32
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: RefName
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
- parameterName: WorkingPath
  type: String
  description: missng XML code comments
  defaultValue: missng XML code comments
status: missng XML code comments
processingTarget: WorkItem

redirectFrom: []
layout: reference
toc: true
permalink: /Reference/v2/EndpointEnrichers/TfsWorkItemAttachmentEnricher/
title: TfsWorkItemAttachmentEnricher
categories:
- EndpointEnrichers
- v2
notes: ''
introduction: ''

---