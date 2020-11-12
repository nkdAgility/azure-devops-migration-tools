## Work Item Tracking Processor


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](.././index.md) > [Reference](../index.md) > [Processors](./index.md) > **WorkItemTrackingProcessor**

This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md).

### Properties

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| ReplayRevisions | Boolean | missng XML code comments | missng XML code comments |
| PrefixProjectToNodes | Boolean | missng XML code comments | missng XML code comments |
| CollapseRevisions | Boolean | missng XML code comments | missng XML code comments |
| WorkItemCreateRetryLimit | Int32 | missng XML code comments | missng XML code comments |
| Processor | String | missng XML code comments | missng XML code comments |
| ToConfigure | Type | missng XML code comments | missng XML code comments |
| Enabled | Boolean | Active the processor if it true. | missng XML code comments |
| Endpoints | List | missng XML code comments | missng XML code comments |
| ProcessorEnrichers | List | missng XML code comments | missng XML code comments |


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
  "ObjectType": "WorkItemTrackingProcessorOptions",
  "ReplayRevisions": true,
  "PrefixProjectToNodes": false,
  "CollapseRevisions": false,
  "WorkItemCreateRetryLimit": 5,
  "Enabled": true,
  "Endpoints": null,
  "ProcessorEnrichers": [
    {
      "Enabled": true
    },
    {
      "Enabled": true
    }
  ]
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
