---
optionsClassName: TfsWorkItemAttachmentEnricherOptions
optionsClassFullName: MigrationTools.EndpointEnrichers.TfsWorkItemAttachmentEnricherOptions
configurationSamples:
- name: default
  description: 
  sample: >-
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

redirectFrom:
- /Reference/v2/EndpointEnrichers/TfsWorkItemAttachmentEnricherOptions/
permalink: /Reference/v2/EndpointEnrichers/TfsWorkItemAttachmentEnricher/

---