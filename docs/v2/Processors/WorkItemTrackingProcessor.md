## Work Item Tracking Processor

This processor is intended, with the aid of [ProcessorEnrichers](./v2/ProcessorEnrichers/), to allow the migration of Work Items between two [Endpoints](./v2/Endpoints/). 

### Properties

Name | Type | Default | Description
-----|------|---------|-------------
Enabled | Boolean | true | Turns this processor on or off.
collapseRevisions | Boolean | false
ReplayRevisions | Boolean | true
WorkItemCreateRetryLimit | Int | 5 | Number of retries when we get a 503 web error
PrefixProjectToNodes | Boolean | false | Preixes the Source location to the catagorisation system of the target

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

```
    {
      "ObjectType": "WorkItemMigrationProcessorOptions",
      "Enabled": true,
      "ReplayRevisions": true,
      "PrefixProjectToNodes": false,
      "CollapseRevisions": false,
      "WorkItemCreateRetryLimit": 5,
    }
```

#### Full Example with Enpoints & Enrichers


```
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
