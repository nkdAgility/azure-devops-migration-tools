# Using the Command Line

Download from [executable](https://github.com/nkdAgility/azure-devops-migration-tools/releases) and extract. Use `migration.exe init` to create a reference `configuration.json` configuration file, Or you can [install from Chocolatey](https://chocolatey.org/packages/vsts-sync-migrator/).

Init has the following options:

--config : which specifies the path and filename of the config file to create, will default to configuration.json
--options : This is an Enum with valid values of `Full` and `WorkItemTracking`. The default is `WorkItemTracking` and this will only create a shortened config for Work Item migration only.

Note that:

- The created reference `configuration.json` shows all the various options available, so is probably more complex than the final edited version you will use.
- All the `Processors` have their `Enabled` property set to `false`.
 
 **This means they are not run. So the default behavior of the generate template is to do nothing. You need to enable the processors you require.**.


```json
{
  "Version": "0.0.0",
  "TelemetryEnableTrace": false,
  "workaroundForQuerySOAPBugEnabled": false,
  "Source": {
    "Collection": "https://dev.azure.com/psd45",
    "Name": "DemoProjs",
    "ReflectedWorkItemIDFieldName": "TfsMigrationTool.ReflectedWorkItemId"
  },
  "Target": {
    "Collection": "https://dev.azure.com/psd46",
    "Name": "DemoProjt",
    "ReflectedWorkItemIDFieldName": "ProcessName.ReflectedWorkItemId"
  },
  "FieldMaps": [],
  "WorkItemTypeDefinition": {
    "Bug": "Bug",
    "Product Backlog Item": "Product Backlog Item"
  },
  "Processors": [
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.NodeStructuresMigrationConfig",
      "PrefixProjectToNodes": false,
      "Enabled": false,
      "BasePaths": [
        "Product\\Area\\Path1",
        "Product\\Area\\Path2"
      ]
    },
    {
      "ObjectType": "VstsSyncMigrator.Engine.Configuration.Processing.WorkItemMigrationConfig",
      "ReplayRevisions": true,
      "PrefixProjectToNodes": false,
      "UpdateCreatedDate": true,
      "UpdateCreatedBy": true,
      "UpdateSourceReflectedId": true,
      "BuildFieldTable": false,
      "AppendMigrationToolSignatureFooter": false,
      "QueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')",
      "OrderBit": "[System.ChangedDate] desc",
      "Enabled": false,
      "LinkMigration": true,
      "AttachmentMigration": true,
      "AttachmentWorkingPath": "c:\\temp\\WorkItemAttachmentWorkingFolder\\",
      "FixHtmlAttachmentLinks": false,
      "WorkItemCreateRetryLimit": 5,
      "FilterWorkItemsThatAlreadyExistInTarget": true
    }
  ]
}


```
