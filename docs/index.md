# Welcome to Visual Studio Team Services Bulk Data Editor Engine

Visual Studio Team Services Bulk Data Editor Engine allows you to bulk edit data in Microsoft Team Foundation Server (TFS) and Visual Studio Team Services (VSTS). It has many names depending on what you are trying to achive. You might call it a migration tool, or a bulk update tool, and both are correct. 

 There are two main purposes for this tooling:

- **Bulk Update** - You can builk update work items and apply processing rules against your server or account. Use the `WorkItemUpdate` class that takes only a target Team Project. 
- **Migration** - You can migrate work items, area & iterations, & test data from one Team Project to another. Use the `WorkItemMigrationContext` calss that takes both a Source and a Target Team Project

**Using the tools**

In order to use these tool you can create a new application in Visual Studio and add a reference to the [Nuget Package]() that is available. If you want a simple bulk update of your code then try:

```csharp
MigrationEngine engine = new MigrationEngine();
engine.SetTarget(new TeamProjectContext(new Uri("https://myaccount.visualstudio.com/"), "MyFirstTeamProject"));
engine.SetReflectedWorkItemIdFieldName("ReflectedWorkItemId");
Dictionary<string, string> stateMapping = new Dictionary<string, string>();
stateMapping.Add("New", "New");
stateMapping.Add("Approved", "New");
stateMapping.Add("Committed", "Active");
stateMapping.Add("In Progress", "Active");
stateMapping.Add("To Do", "New");
stateMapping.Add("Done", "Closed");
engine.AddFieldMap("*", new FieldValueMap("System.State", "System.State", stateMapping));
engine.AddFieldMap("*", new FieldToTagFieldMap("System.State", "ScrumState:{0}"));
engine.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Common.BacklogPriority", "Microsoft.VSTS.Common.StackRank"));
engine.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.Scheduling.Effort", "Microsoft.VSTS.Scheduling.StoryPoints"));
engine.AddFieldMap("*", new FieldMergeMap("System.Description", "Microsoft.VSTS.Common.AcceptanceCriteria", "System.Description", @"{0} <br/><br/><h3>Acceptance Criteria</h3>{1}"));
engine.AddFieldMap("*", new FieldToFieldMap("Microsoft.VSTS.CMMI.AcceptanceCriteria", "COMPANY.DEVISION.Analysis"));
engine.AddProcessor(new WorkItemUpdate(me, @" AND [System.Id]=3 "));
```

You can also use the pre built package with a configuration file:

Download from [executable](https://github.com/nkdAgility/vsts-data-bulk-editor/releases) and extract. Use `vstsbulkeditor.exe init` to create a reference `vstsbulkeditor.json` configration file.

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

There are for field mapping systems available:

- FieldValueMap
- FieldToTagFieldMap
- FieldToTagFieldMap
- RegexFieldMap

There are other processors that can be used to migrate, or process, different sorts of data:

- AttachementExportMigrationContext
- AttachementImportMigrationContext
- LinkMigrationContext
- NodeStructuresMigrationContext
- TestConfigurationsMigrationContext
- TestPlansAndSuitsMigrationContext
- TestVeriablesMigrationContext
- TestRunsMigrationContext
- WorkItemMigrationContext
- ImportProfilePictureContext
- WorkItemDelete


Beta Processors:

- CreateTeamFolders
- ExportProfilePictureFromADContext
- ExportTeamList
- FixGitCommitLinks

**Contributing**

If you wish to contribute then feel free to fork this repository and submit a pull request. If you would like to join the team please contact.

This project is primarily managed and maintained on Visual Studio Team Services and code checked into MASTER is automatically synched between VSTS and GitHub. There is no hidden published code, but not all branches are published.

If you want to sync your GitHub repository the check out [Open-source with VSTS or TFS and Github for better DevOps
](https://nkdagility.com/open-source-vsts-tfs-github-better-devops/).

**Terms**

naked Agility Limited creates and maintains the "Visual Studio Team Services Bulk Data Editor Engine" project under its [terms of business](https://nkdagility.com/company/consulting-terms-of-business/) and allows full access to the source code for customers and the general public. 


