# Getting Started with the VSTS Sync Migrator

If you want to perform a bulk edit or a migration then you need to start here. This tool has been tested on updating from 100 to 250,000 work items by its users.

## Install

In order to run the migration you will need to install the tools first.

1. Install Chocolatey from [https://chocolatey.org/install](https://chocolatey.org/install)
1. Run "**choco install vsts-sync-migrator**" to install the tools [source](https://chocolatey.org/packages/vsts-sync-migrator)

The tools are now installed and calling "vsts-sm" from any command line will run the tools.

## Create a default configuration file

1. Open a command prompt or PowerShell window
2. Run "vsts-sm init" to create a default configuration
3. Open "vstsbulkeditor.json" from the current directory

You can now customise the configuration depending on what you need to do. However a basic config that you can use to migrate from one team project to another with the same process template is:

```
{
  "TelemetryEnableTrace": true,
  "Source": {
    "Collection": "https://sdd2016.visualstudio.com/",
    "Name": "DemoProjs"
  },
  "Target": {
    "Collection": "https://sdd2016.visualstudio.com/",
    "Name": "DemoProjt"
  },
  "ReflectedWorkItemIDFieldName": "TfsMigrationTool.ReflectedWorkItemId",
  "WorkItemTypeDefinition": {
    "Bug": "Bug",
    "Epic": "Epic",
    "Feature": "Feature",
    "Product Backlog Item": "Product Backlog Item",
    "Shared Parameter": "Shared Parameter",
    "Shared Steps": "Shared Steps",
    "Task": "Task",
    "Test Case": "Test Case"
  },
  "Processors": [
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.NodeStructuresMigrationConfig",
      "Enabled": false,
      "PrefixProjectToNodes": false
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.WorkItemMigrationConfig",
      "Enabled": false,
      "PrefixProjectToNodes": true,
      "UpdateCreatedDate": true,
      "UpdateCreatedBy": true,
      "UpdateSoureReflectedId": true,
      "QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Product Backlog Item', 'Task', 'Feature', 'Epic', 'Bug')"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.LinkMigrationConfig",
      "Enabled": false,
      "QueryBit": "AND [System.ExternalLinkCount] > 0 AND [System.RelatedLinkCount] > 0"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.AttachementExportMigrationConfig",
      "Enabled": false,
      "QueryBit": "AND [System.AttachedFileCount] > 0"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.AttachementImportMigrationConfig",
      "Enabled": false
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

If you have Test Suits & Plans you can also migrate them using other processors.




