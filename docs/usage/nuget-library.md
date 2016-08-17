# Using the API

The VSTS Bulk Data Editor tool has a public API that is available on Nuget as a package to download. This is for advanced users that want to configure more complicated patterns or even create their own FieldMaps and Processors.

## Getting Started

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
