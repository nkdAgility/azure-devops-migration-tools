# Getting Started with the VSTS Sync Migrator

If you want to perform a bulk edit or a migration then you need to start here. This tool has been tested on updating from 100 to 250,000 work items by its users.

Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get you started in 30 minutes. This tool is complicated and it's not always easy to discover what you need to do.

## Install

In order to run the migration you will need to install the tools first.

1. Install Chocolatey from [https://chocolatey.org/install](https://chocolatey.org/install)
1. Run `choco install vsts-sync-migrator` to install the tools [source](https://chocolatey.org/packages/vsts-sync-migrator)

The tools are now installed. To run them you will need to switch to `c:\tools\MigrationTools\` and run `migration.exe`.

## Upgrade

1. Run `choco upgrade  vsts-sync-migrator` to upgrade the tools [source](https://chocolatey.org/packages/vsts-sync-migrator)

## Server configuration and setup

Follow the [setup instructions](./server-configuration.md) to make sure that you can run the tool against your environments and importantly add the required custom field 'ReflectedWorkItemId'

## Create a default configuration file

1. Open a command prompt or PowerShell window at `C:\tools\MigrationTools\`
2. Run `./migration.exe init` to create a default configuration
3. Open `configuration.json` from the current directory

You can now customise the configuration depending on what you need to do. However a basic config that you can use to migrate from one team project to another with the same process template is:

```JSON
{
  "ChangeSetMappingFile": null,
  "Source": {
    "$type": "TfsTeamProjectConfig",
    "Collection": "https://dev.azure.com/nkdagility-preview/",
    "Project": "myProjectName",
    "ReflectedWorkItemIDFieldName": "Custom.ReflectedWorkItemId",
    "AllowCrossProjectLinking": false,
    "AuthenticationMode": "Prompt",
    "PersonalAccessToken": "",
    "LanguageMaps": {
      "AreaPath": "Area",
      "IterationPath": "Iteration"
    }
  },
  "Target": {
    "$type": "TfsTeamProjectConfig",
    "Collection": "https://dev.azure.com/nkdagility-preview/",
    "Project": "myProjectName",
    "ReflectedWorkItemIDFieldName": "Custom.ReflectedWorkItemId",
    "AllowCrossProjectLinking": false,
    "AuthenticationMode": "Prompt",
    "PersonalAccessToken": "",
    "LanguageMaps": {
      "AreaPath": "Area",
      "IterationPath": "Iteration"
    }
  },
  "FieldMaps": [
    {
      "$type": "MultiValueConditionalMapConfig",
      "WorkItemTypeName": "*",
      "sourceFieldsAndValues": {
        "Field1": "Value1",
        "Field2": "Value2"
      },
      "targetFieldsAndValues": {
        "Field1": "Value1",
        "Field2": "Value2"
      }
    },
    {
      "$type": "FieldBlankMapConfig",
      "WorkItemTypeName": "*",
      "targetField": "TfsMigrationTool.ReflectedWorkItemId"
    },
    {
      "$type": "FieldValueMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.State",
      "targetField": "System.State",
      "defaultValue": "New",
      "valueMapping": {
        "Approved": "New",
        "New": "New",
        "Committed": "Active",
        "In Progress": "Active",
        "To Do": "New",
        "Done": "Closed",
        "Removed": "Removed"
      }
    },
    {
      "$type": "FieldtoFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
      "targetField": "Microsoft.VSTS.Common.StackRank",
      "defaultValue": null
    },
    {
      "$type": "FieldtoFieldMultiMapConfig",
      "WorkItemTypeName": "*",
      "SourceToTargetMappings": {
        "SourceField1": "TargetField1",
        "SourceField2": "TargetField2"
      }
    },
    {
      "$type": "FieldtoTagMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.State",
      "formatExpression": "ScrumState:{0}"
    },
    {
      "$type": "FieldMergeMapConfig",
      "WorkItemTypeName": "*",
      "sourceField1": "System.Description",
      "sourceField2": "Microsoft.VSTS.Common.AcceptanceCriteria",
      "sourceField3": null,
      "targetField": "System.Description",
      "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}",
      "doneMatch": "##DONE##"
    },
    {
      "$type": "RegexFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "COMPANY.PRODUCT.Release",
      "targetField": "COMPANY.DEVISION.MinorReleaseVersion",
      "pattern": "PRODUCT \\d{4}.(\\d{1})",
      "replacement": "$1"
    },
    {
      "$type": "FieldValuetoTagMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "Microsoft.VSTS.CMMI.Blocked",
      "pattern": "Yes",
      "formatExpression": "{0}"
    },
    {
      "$type": "TreeToTagMapConfig",
      "WorkItemTypeName": "*",
      "toSkip": 3,
      "timeTravel": 1
    }
  ],
  "GitRepoMapping": null,
  "LogLevel": "Information",
  "Processors": [
    {
      "$type": "WorkItemMigrationConfig",
      "Enabled": false,
      "ReplayRevisions": true,
      "PrefixProjectToNodes": false,
      "UpdateCreatedDate": true,
      "UpdateCreatedBy": true,
      "WIQLQueryBit": "AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
      "WIQLOrderBit": "[System.ChangedDate] desc",
      "LinkMigration": true,
      "AttachmentMigration": true,
      "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
      "FixHtmlAttachmentLinks": false,
      "SkipToFinalRevisedWorkItemType": true,
      "WorkItemCreateRetryLimit": 5,
      "FilterWorkItemsThatAlreadyExistInTarget": true,
      "PauseAfterEachWorkItem": false,
      "AttachmentMaxSize": 480000000,
      "AttachRevisionHistory": false,
      "LinkMigrationSaveEachAsAdded": false,
      "GenerateMigrationComment": true,
      "NodeStructureEnricherEnabled": null,
      "NodeBasePaths": [
        "Product\\Area\\Path1",
        "Product\\Area\\Path2"
      ],
      "WorkItemIDs": null,
      "MaxRevisions": 0
    }
  ],
  "Version": "11.12",
  "workaroundForQuerySOAPBugEnabled": false,
  "WorkItemTypeDefinition": {
    "sourceWorkItemTypeName": "targetWorkItemTypeName"
  },
  "Endpoints": {
    "InMemoryWorkItemEndpoints": [
      {
        "Name": "Source",
        "EndpointEnrichers": null
      },
      {
        "Name": "Target",
        "EndpointEnrichers": null
      }
    ]
  }
}
```

The default [WorkItemMigrationConfig](./Processors/WorkItemMigrationConfig.md) processor will perform the following operations:

* Migrate interations and sprints
* Attachments
* Links including for source code. Optionally clone the repositories before starting the migration to have links maintained on the initial pass.

## How to execute configuration.json with minimal adjustments

> Remember to add custom field ['ReflectedWorkItemId'](./server-configuration.md) to both, the source and the target team project before starting migration!

1. Adjust the value of the `Collection` attribute for Source and Target
1. Adjust the value of the `Project` attribute for Source and Target
1. Set the `AuthenticationMode` (`Prompt` or `AccessToken`) for Source and Target

    If you set Authentication mode to `AccessToken`, enter a valid PAT as value for the `PersonalAccessToken` attribute

1. Adjust the value of the `ReflectedWorkItemIDFieldName` attribute (field name of the migration tracking field) for Source and Target

   For example: `TfsMigrationTool.ReflectedWorkItemId` for TFS, `ReflectedWorkItemId` for VSTS or `Custom.ReflectedWorkItemId` for Azure DevOps

1. Enable the `WorkItemMigrationConfig` processor by setting `Enabled` to `true`
1. [OPTIONAL] Modify the `WIQLQueryBit` to migrate only the work items you want. The default WIQL will migrate all open work items and revisions excluding test suites and plans
1. Adjust the [`NodeBasePaths`](./Processors/WorkItemMigrationConfig.md) or leave empty to migrate all nodes
1. From the `C:\tools\MigrationTools\` path run `.\migration.exe execute --config .\configuration.json`

**Remember:** if you want a processor to run, it's `Enabled` attribute must be set to `true`. 
