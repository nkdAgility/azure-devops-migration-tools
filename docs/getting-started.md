# Getting Started with the VSTS Sync Migrator

If you want to perform a bulk edit or a migration then you need to start here. This tool has been tested on updating from 100 to 250,000 work items by its users.

Watch the [Video Overview](https://youtu.be/RCJsST0xBCE) to get you started in 30 minutes. This tool is complicated and its not always easy to discover what you need to do.

## Install

In order to run the migration you will need to install the tools first.

1. Install Chocolatey from [https://chocolatey.org/install](https://chocolatey.org/install)
1. Run "**choco install vsts-sync-migrator**" to install the tools [source](https://chocolatey.org/packages/vsts-sync-migrator)

The tools are now installed and calling "vstssyncmigrator" from any command line will run the tools or it will not and you will need to switch to `c:\tools\vstssyncmigration\` and run `migrate.exe`.

## Server configuration and setup

Follow the [setup instructions](./server-configuration.md) to make sure that you can run the tool against your enviroments.


## Create a default configuration file

1. Open a command prompt or PowerShell window at `c:\tools\vstssyncmigration\`
2. Run "./migration.exe init" to create a default configuration
3. Open "configuration.json" from the current directory

You can now customise the configuration depending on what you need to do. However a basic config that you can use to migrate from one team project to another with the same process template is:

```
{
  "Version": "0.0",
  "TelemetryEnableTrace": false,
  "workaroundForQuerySOAPBugEnabled": false,
  "Source": {
    "Collection": "https://dev.azure.com/nkdagility-preview/",
    "Project": "migrationSource1",
    "ReflectedWorkItemIDFieldName": "Custom.ReflectedWorkItemId",
    "AllowCrossProjectLinking": false,
    "PersonalAccessToken": "",
    "LanguageMaps": {
      "AreaPath": "Area",
      "IterationPath": "Iteration"
    }
  },
  "Target": {
    "Collection": "https://dev.azure.com/nkdagility-preview/",
    "Project": "migrationTarget1",
    "ReflectedWorkItemIDFieldName": "Custom.ReflectedWorkItemId",
    "AllowCrossProjectLinking": false,
    "PersonalAccessToken": "",
    "LanguageMaps": {
      "AreaPath": "Area",
      "IterationPath": "Iteration"
    }
  },
  "FieldMaps": [
    {
      "ObjectType": "MultiValueConditionalMapConfig",
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
      "ObjectType": "FieldBlankMapConfig",
      "WorkItemTypeName": "*",
      "targetField": "TfsMigrationTool.ReflectedWorkItemId"
    },
    {
      "ObjectType": "FieldValueMapConfig",
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
      "ObjectType": "FieldtoFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
      "targetField": "Microsoft.VSTS.Common.StackRank"
    },
    {
      "ObjectType": "FieldtoFieldMultiMapConfig",
      "WorkItemTypeName": "*",
      "SourceToTargetMappings": {
        "SourceField1": "TargetField1",
        "SourceField2": "TargetField2"
      }
    },
    {
      "ObjectType": "FieldtoTagMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.State",
      "formatExpression": "ScrumState:{0}"
    },
    {
      "ObjectType": "FieldMergeMapConfig",
      "WorkItemTypeName": "*",
      "sourceField1": "System.Description",
      "sourceField2": "Microsoft.VSTS.Common.AcceptanceCriteria",
      "targetField": "System.Description",
      "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}",
      "doneMatch": "##DONE##"
    },
    {
      "ObjectType": "RegexFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "COMPANY.PRODUCT.Release",
      "targetField": "COMPANY.DEVISION.MinorReleaseVersion",
      "pattern": "PRODUCT \\d{4}.(\\d{1})",
      "replacement": "$1"
    },
    {
      "ObjectType": "FieldValuetoTagMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "Microsoft.VSTS.CMMI.Blocked",
      "pattern": "Yes",
      "formatExpression": "{0}"
    },
    {
      "ObjectType": "TreeToTagMapConfig",
      "WorkItemTypeName": "*",
      "toSkip": 3,
      "timeTravel": 1
    }
  ],
  "WorkItemTypeDefinition": {
    "sourceWorkItemTypeName": "targetWorkItemTypeName"
  },
  "GitRepoMapping": null,
  "Processors": [
    {
      "ObjectType": "NodeStructuresMigrationConfig",
      "PrefixProjectToNodes": false,
      "Enabled": false,
      "BasePaths": [
        "Product\\Area\\Path1",
        "Product\\Area\\Path2"
      ]
    },
    {
      "ObjectType": "WorkItemMigrationConfig",
      "ReplayRevisions": true,
      "PrefixProjectToNodes": false,
      "UpdateCreatedDate": true,
      "UpdateCreatedBy": true,
      "BuildFieldTable": false,
      "AppendMigrationToolSignatureFooter": false,
      "QueryBit": "AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
      "OrderBit": "[System.ChangedDate] desc",
      "Enabled": false,
      "LinkMigration": true,
      "AttachmentMigration": true,
      "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
      "FixHtmlAttachmentLinks": false,
      "SkipToFinalRevisedWorkItemType": false,
      "WorkItemCreateRetryLimit": 5,
      "FilterWorkItemsThatAlreadyExistInTarget": true,
      "PauseAfterEachWorkItem": false,
      "AttachmentMaxSize": 480000000,
      "CollapseRevisions": false,
      "LinkMigrationSaveEachAsAdded": false
    }
  ]
}

```

Here we are performing the following operations:

1. Set the Source and Target Team Projects
1. Set the field name of the Migration tracking field. I use "TfsMigrationTool.ReflectedWorkItemId" for TFS and "ReflectedWorkItemId" for VSTS
1. Set the mapping of Work Items that you want in "WorkItemTypeDefinition". This allows you to merge and change types..
1. NodeStructuresMigration - We need to create the same Area and Iteration paths in the Target Team Project as you have in the Source. Make sure you clean up your Area and Iteration paths first.
1. Work Item Migration - Move the work items. Use the QueryBit to scope to only the items that you want.
1. Link Migration - Once all of the work items are across you can then migrate the links. Links will only be re-created if both ends of the link are in the new system. The "ReflectedWorkItemId" field value is used to find the new work items easily.
1. Attachment Export & Import - Now we can export and then re-import the attachments. This just done separately to prevent errors

**Remember** if you want a processor to run it's `Enabled` property must be set to true. 

If you have Test Suits & Plans you can also migrate them using other processors, you would need to add them to your configuration file.




