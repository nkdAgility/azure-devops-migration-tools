## EndpointEnrichers: TfsWorkItemAttachmentEnricher

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v2](../index.md) > [EndpointEnrichers](index.md)> **TfsWorkItemAttachmentEnricher**

The `TfsWorkItemAttachmentEnricher` processes the attachements for a specific work item.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| MaxSize | Int32 | missng XML code comments | missng XML code comments |
| RefName | String | missng XML code comments | missng XML code comments |
| WorkingPath | String | missng XML code comments | missng XML code comments |


### Example JSON

```JSON
{
  "$type": "TfsWorkItemAttachmentEnricherOptions",
  "Enabled": true,
  "WorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
  "MaxSize": 480000000
}
```