# Configuration
VSTS Bulk Data Editor is mainly powered by configuration which allows you to control most aspects of the execution flow.

## Configuration tool
If you run `vstsbulkeditor.exe init` you will be launched into a configuration tool that will generate a default file. Using the `init` command will create a `vstsbulkeditor.yml` file in the
working directory. At run time you can specify the configuration file to use.

**Note:** VstsBulkEditor does not ship with internal default configuration and will not function without one.

To create your config file just type `vstsbulkeditor init` in the directory that you unziped the tools and a minimal `VstsBulkEditor.yml` configuration
file will be created. Modify this as you need.

## Global configuration
The global configuration look like this:

```json
{
  "TelemetryEnableTrace": true,
  "Source": {
    "Collection": "https://tfs.mycompany.com/tfs/collection1/",
    "Name": "MyProduct"
  },
  "Target": {
    "Collection": "https://mycompany.visualstudio.com/",
    "Name": "MyProduct"
  },
  "ReflectedWorkItemIDFieldName": "TfsMigrationTool.ReflectedWorkItemId",
  "FieldMaps": [
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.FieldMap.FieldBlankMapConfig",
      "WorkItemTypeName": "*",
      "targetField": "TfsMigrationTool.ReflectedWorkItemId"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.FieldMap.FieldValueMapConfig",
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
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.FieldMap.FieldtoFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "Microsoft.VSTS.Common.BacklogPriority",
      "targetField": "Microsoft.VSTS.Common.StackRank"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.FieldMap.FieldtoTagMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "System.State",
      "formatExpression": "ScrumState:{0}"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.FieldMap.FieldMergeMapConfig",
      "WorkItemTypeName": "*",
      "sourceField1": "System.Description",
      "sourceField2": "Microsoft.VSTS.Common.AcceptanceCriteria",
      "targetField": "System.Description",
      "formatExpression": "{0} <br/><br/><h3>Acceptance Criteria</h3>{1}"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.FieldMap.RegexFieldMapConfig",
      "WorkItemTypeName": "*",
      "sourceField": "COMPANY.PRODUCT.Release",
      "targetField": "COMPANY.DEVISION.MinorReleaseVersion",
      "pattern": "PRODUCT \\d{4}.(\\d{1})",
      "replacement": "$1"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.FieldMap.TreeToTagMapConfig",
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
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.WorkItemMigrationConfig",
      "Disabled": true,
      "QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.WorkItemUpdateConfig",
      "WhatIf": false,
      "Disabled": true,
      "QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.NodeStructuresMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.LinkMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.WorkItemPostProcessingConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.WorkItemDeleteConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.AttachementExportMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.AttachementImportMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.TestVeriablesMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.TestConfigurationsMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.TestPlansAndSuitsMigrationConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.TestRunsMigrationConfig",
      "Disabled": true,
      "Status": "Experimental"
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.ImportProfilePictureConfig",
      "Disabled": true
    },
    {
      "ObjectType": "VSTS.DataBulkEditor.Engine.Configuration.Processing.ExportProfilePictureFromADConfig",
      "Disabled": true,
      "Domain": null,
      "Username": null,
      "Password": null
    }
  ]
}
```

And the description of the available options are:

### TelemetryEnableTrace
Allows you to submit trace to Application Insights to allow the devellpment team to diagnose any issues that may be found. If you are submitting a support ticket then please include the Session GUID found in your log file for that run. This will help us find the problem.

**note:** All exceptions that you encounter will surface inside of Visual Studio as the developers are working on the source. This will make sure that they tackle issues as they arise.

### Source & Target
Both the `Source` and `Target` entries hold the collection URL and the Team Project name that you are connecting to. The `Source` is where the tool will read the data to migrate. The `Target` is where the tool will write the data.

### ReflectedWorkItemIDFieldName

This is the field that will be used to store the state for the migration. See [Server Configuration](server-configuration.md)

...
