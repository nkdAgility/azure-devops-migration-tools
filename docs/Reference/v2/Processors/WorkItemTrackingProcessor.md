## Work Item Tracking Processor


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v2](../index.md) > [Processors](index.md)> **WorkItemTrackingProcessor**

This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md).

### Properties

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| CollapseRevisions | Boolean | missng XML code comments | missng XML code comments |
| Enabled | Boolean | If set to `true` then the processor will run. Set to `false` and the processor will not run. | missng XML code comments |
| PrefixProjectToNodes | Boolean | missng XML code comments | missng XML code comments |
| ProcessorEnrichers | List | List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors. | missng XML code comments |
| RefName | String | `Refname` will be used in the future to allow for using named Options without the need to copy all of the options. | missng XML code comments |
| ReplayRevisions | Boolean | missng XML code comments | missng XML code comments |
| SourceName | String | missng XML code comments | missng XML code comments |
| TargetName | String | missng XML code comments | missng XML code comments |
| WorkItemCreateRetryLimit | Int32 | missng XML code comments | missng XML code comments |

{: .table .table-striped .table-bordered .d-none .d-md-block}

### Supported Endpoints

- TfsWorkItemEndpoint
- FileSystemWorkItemEndpoint
- InMemoryWorkItemEndpoint

### Supported Processor Enrichers

- PauseAfterEachWorkItem
- AppendMigrationToolSignatureFooter
- FilterWorkItemsThatAlreadyExistInTarget
- SkipToFinalRevisedWorkItemType


### Work Item Tracking Processor Options


#### Minimum Options to run

```JSON
{
  "$type": "WorkItemTrackingProcessorOptions",
  "Enabled": true,
  "ReplayRevisions": true,
  "PrefixProjectToNodes": false,
  "CollapseRevisions": false,
  "WorkItemCreateRetryLimit": 5,
  "ProcessorEnrichers": [
    {
      "$type": "PauseAfterEachItemOptions",
      "Enabled": true
    },
    {
      "$type": "AppendMigrationToolSignatureFooterOptions",
      "Enabled": true
    }
  ],
  "SourceName": null,
  "TargetName": null
}
```

#### Full Example with Enpoints & Enrichers


```JSON
    {
      "ObjectType": "WorkItemMigrationProcessorOptions",
      "Enabled": true,
      "ReplayRevisions": true,
      "PrefixProjectToNodes": false,
      "CollapseRevisions": false,
      "WorkItemCreateRetryLimit": 5,
      "Enrichers": [
        {
          "ObjectType": "PauseAfterEachItemOptions",
          "Enabled": true
        },
        {
          "ObjectType": "FilterWorkItemsThatAlreadyExistInTargetOptions",
          "Enabled": true,
          "Query": {
            "WhereBit": "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
            "OrderBit": "[System.ChangedDate] desc"
          }
        },
        {
          "ObjectType": "AppendMigrationToolSignatureFooterOptions",
          "Enabled": false
        },
        {
          "ObjectType": "SkipToFinalRevisedWorkItemTypeOptions",
          "Enabled": false
        }
      ],
      "Endpoints": [
        {
          "ObjectType": "FileSystemWorkItemEndpointOptions",
          "Direction": "Source",
          "FileStore": ".\\Store\\Source\\",
          "Query": {
            "WhereBit": "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
            "OrderBit": "[System.ChangedDate] desc"
          },
          "Enrichers": [
            {
              "ObjectType": "WorkItemAttachmentEnricherOptions",
              "Enabled": true,
              "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
              "AttachmentMaxSize": 480000000
            },
            {
              "ObjectType": "WorkItemLinkEnricherOptions",
              "Enabled": true,
              "LinkMigrationSaveEachAsAdded": true
            }
          ]
        },
        {
          "ObjectType": "TfsWorkItemEndPointOptions",
          "Direction": "Target",
          "AccessToken": "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
          "Query": {
            "Query": "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc"
          },
          "Enrichers": [
            {
              "ObjectType": "WorkItemAttachmentEnricherOptions",
              "Enabled": true,
              "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
              "AttachmentMaxSize": 480000000
            },
            {
              "ObjectType": "WorkItemEmbedEnricherOptions",
              "Enabled": true,
              "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\"
            },
            {
              "ObjectType": "WorkItemLinkEnricherOptions",
              "Enabled": true,
              "LinkMigrationSaveEachAsAdded": true
            },
            {
              "ObjectType": "WorkItemCreatedEnricherOptions",
              "Enabled": true,
              "UpdateCreatedDate": true,
              "UpdateCreatedBy": true
            },
            {
              "ObjectType": "WorkItemFieldTableEnricherOptions",
              "Enabled": true
            }
          ]
        }
      ]
    }
```
