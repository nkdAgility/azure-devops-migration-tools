# Using the Command Line

Download from [executable](https://github.com/nkdAgility/vsts-data-bulk-editor/releases) and extract. Use `vstsbulkeditor.exe init` to create a reference `vstsbulkeditor.json` configration file, Or you can [install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/).

```json
{
  "TelemetryEnableTrace": true,
  "Target": {
    "Collection": "https://sdd2016.visualstudio.com/",
    "Name": "DemoProjt"
  },
  "Source": {
    "Collection": "https://sdd2016.visualstudio.com/",
    "Name": "DemoProjs"
  },
  "ReflectedWorkItemIDFieldName": "TfsMigrationTool.ReflectedWorkItemId",
  "FieldMaps": [
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldValueMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.State",
      "targetField": "System.State",
      "valueMapping": {
        "Approved": "New",
        "New": "New",
        "Committed": "Active",
        "In Progress": "Active",
        "To Do": "New",
        "Done": "Closed"
      }
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldtoFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
      "targetField": "Microsoft.VSTS.Common.StackRank"
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldtoTagMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.State",
      "formatExpression": "ScrumState:{0}"
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.FieldMergeMapConfig",
      "WorkItemTypeName": "*",
      "sourceField1": "System.Description",
      "sourceField2": "Microsoft.VSTS.Common.AcceptanceCriteria",
      "targetField": "System.Description",
      "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}"
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.RegexFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "COMPANY.PRODUCT.Release",
      "targetField": "COMPANY.DEVISION.MinorReleaseVersion",
      "pattern": "PRODUCT \\d{4}.(\\d{1})",
      "replacement": "$1"
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.FieldMap.TreeToTagMapConfig",
      "WorkItemTypeName": "*",
      "toSkip": 3,
      "timeTravel": 1
    }
  ],
  "WorkItemTypeDefinition": {
    "Bug": "Bug",
    "Product Backlog Item": "Product Backlog Item"
  },
  "Processors": [
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemMigrationConfig",
      "Disabled": true,
      "QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')"
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemUpdateConfig",
      "WhatIf": false,
      "Disabled": true,
      "QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')"
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.LinkMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemPostProcessingConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemDeleteConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.AttachementExportMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.AttachementImportMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.TestVeriablesMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.TestConfigurationsMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.TestPlansAndSuitsMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.TestRunsMigrationConfig",
      "Disabled": true,
      "Status": "Experimental"
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.ImportProfilePictureConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.ExportProfilePictureFromADConfig",
      "Disabled": true,
      "Domain": null,
      "Username": null,
      "Password": null
    }
  ]
}
```
