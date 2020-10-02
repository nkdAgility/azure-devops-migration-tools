# Work item migration

That is the working horse. This processor migrates all the work items. That should be run after the following pre processors:

* `TeamMigrationConfig`
* `WorkItemQueryMigrationConfig`
 
It will migrate work items using a tip or replay migrator as well as Attachments, & Links.

## Features

- Migrate Work Items, Links, & Attachments
- Restart will skip completed work items in Tip, and will filter completed revisions in replay.

## Params

| Parameter name                       | Type    | Description                              | Default Value                            |
|--------------------------------------|---------|------------------------------------------|------------------------------------------|
| `Enabled`                            | Boolean | Active the processor if it true.         | false                                    |
| `ObjectType`                         | string  | The name of the processor                | VstsSyncMigrator. Engine. Configuration. Processing. WorkItemMigrationConfig |
| `PrefixProjectToNodes`               | Boolean | Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too. | false                                    |
| `UpdateCreatedDate`                  | Boolean | If this is enabled the creation process on the target project will create the items with the original creation date. (Important: The item history is always pointed to the date of the migration, it's change only the data column `CreateDate`, not the internal create date) | false                                    |
| `UpdateCreatedBy`                    | Boolean | If this is enabled the creation process on the target project will create the items with the original creation date. (Important: The item history is always pointed to the date of the migration, it's change only the data column `CreateDate`, not the internal create date) | false                                    |
| `BuildFieldTable`                    | Boolean | Add in the original field to value table in a history comment. So if you lost information with the field mapping you are on the save side without data lost. This table is searchable using a `CONTAINS` WIQL query | false                                    |
| `AppendMigrationToolSignatureFooter` | Boolean | Add a signature to the in the comment history of each work item. If you like this project please set this to true 😊 | false                                    |
| `ReplayRevisions` | Boolean | You can choose to migrate the tip only (a single write) or all of the revisions (many writes). If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true` | true |
| `LinkMigration` | Boolean | If enabled this will migrate the Links for the work item at the same time as the whole work item. | true |
| _{NEW}_ `LinkMigrationSaveEachAsAdded` | Boolean | If you have changed parents before re-running a sync you may get a `TF26194: unable to change the value of the 'Parent' field` error. This will resolve it, but will slow migration. | false                                    |
| `AttachmentMigration` | Boolean | If enabled this will migrate all of the attachments at the same time as the work item | true |
| `AttachmentWorkingPath` | String | `AttachmentMigration` is set to true then you need to specify a working path for attachments to be saved locally. | `C:\temp\Migration\` |
| `AttachmentMaxSize` | int | `AttachmentMigration` is set to true then you need to specify a max file size for upload in bites. For Azure DevOps Services the default is 480,000,000 bites (60mb), for TFS its 32,000,000 bites (4mb). | `480000000` |
| `FixHtmlAttachmentLinks` | Boolean | **beta** If enabled this will fix any image attachments URL's in the HTML fields. You must specify a PersonalAccessToken in the Source project for Azure DevOps; TFS should use integrated authentication.  |
| `WorkItemCreateRetryLimit` | Integer | *beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count. This allows for periodic network glitches not to end the process. | 5 |
| `FilterWorkItemsThatAlreadyExistInTarget` | Boolean | This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run. While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart. | true |
| `WIQLQueryBit`                           | string  | A work item query based on WIQL to select only important work items. To migrate all leave this empty. |                                          |
| `WIQLOrderBit` | string | A work item query to affect the order in which the work items are migrated. Don't leave this empty. | [System.ChangedDate] desc
| `SkipToFinalRevisedWorkItemType` | Boolean | If enabled, when a revision is found that changes the work item type it will use the most recent revision work item type when migrating the initial work item. This should only be enabled for migrations from Azure DevOps Service to Azure DevOps Server. | false
| `CollapseRevisions` | Boolean | If enabled, all revisions except the most recent are collapsed into a JSON format and attached as an attachment. Requires ReplayRevisions to be enabled. | false


## WIQL Query Bits

The Work Item queries are all built using Work Item [Query Language (WIQL)](https://docs.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax). 


### Examples

You can use the [WIQL Editor](https://marketplace.visualstudio.com/items?itemName=ottostreifel.wiql-editor) to craft a query in Azure DevOps.

Typical way that queris are built:

```
 var targetQuery = 
     string.Format(
         @"SELECT [System.Id], [{ReflectedWorkItemIDFieldName}] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {WIQLQueryBit} ORDER BY {WIQLOrderBit}",
         Engine.Target.Config.ReflectedWorkItemIDFieldName,
         _config.WIQLQueryBit,
         _config.WIQLOrderBit
                    );
var targetFoundItems = Engine.Target.WorkItems.GetWorkItems(targetQuery);
```

A simple example config:

```
"QueryBit": "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
"OrderBit": "[System.ChangedDate] desc",
```
Scope to Area Path (Team data):

```
"QueryBit": "AND [System.AreaPath] UNDER 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
"OrderBit": "[System.ChangedDate] desc",
```

```
"QueryBit": "AND [System.ChangedDate] > 'project\Team 1\' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
"OrderBit": "[System.ChangedDate] desc",
```
