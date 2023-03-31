---
title: TfsWorkItemAttachmentEnricher
layout: default
template: default
pageType: reference
classType: EndpointEnrichers
architecture: v2
toc: true
pageStatus: generated
discussionId: 
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

The `TfsWorkItemAttachmentEnricher` processes the attachements for a specific work item.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| MaxSize | Int32 | missng XML code comments | missng XML code comments |
| RefName | String | missng XML code comments | missng XML code comments |
| WorkingPath | String | missng XML code comments | missng XML code comments |
{: .table .table-striped .table-bordered .d-none .d-md-block}


### Example JSON

```JSON
{
  "$type": "TfsWorkItemAttachmentEnricherOptions",
  "Enabled": true,
  "WorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
  "MaxSize": 480000000
}
```