## Work Item Tracking Processor


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

<Breadcrumbs>

<Description>

### Properties

<Options>

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
<ExampleJson>
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
