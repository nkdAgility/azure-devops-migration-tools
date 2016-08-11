# Visual Studio Team Services Bulk Data Editor Engine

Visual Studio Team Services Bulk Data Editor Engine allows you to bulk edit data in Microsoft Team Foundation Server (TFS) and Visual Studio Team Services (VSTS).

##BUILD
|         | Build           | Sync           |
| ------------- |:-------------:|:-------------:|
| VSTS      | ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/57/badge) | ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/58/badge) | 
| GITHUB      | ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/60/badge)     | ![Build on VSTS](https://nkdagility.visualstudio.com/_apis/public/build/definitions/1b52ce63-eccc-41c8-88f9-ae6ebeefdc63/59/badge)      |

##RELEASE

|         |  |  |
| ------------- |:-------------:|:-------------:|:-------------:|:-------------:|
| Nuget      | [![NuGet](https://img.shields.io/nuget/v/VSTS.DataBulkEditor.Engine.svg?maxAge=2592000)]() | [![NuGet Pre Release](https://img.shields.io/nuget/vpre/VSTS.DataBulkEditor.Engine.svg?maxAge=2592000)]() |

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


