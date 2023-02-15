## Processors: WorkItemUpdate

>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.github.io/azure-devops-migration-tools).**

[Overview](../../../index.md) > [Reference](../../index.md) > [API v1](../index.md) > [Processors](index.md)> **WorkItemUpdate**

This processor allows you to make changes in place where we load from teh Target and update the Target. This is used for bulk updates with the most common reason being a process template change.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| FilterWorkItemsThatAlreadyExistInTarget | Boolean | This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run. While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart. | true |
| PauseAfterEachWorkItem | Boolean | Pause after each work item is migrated | false |
| WhatIf | Boolean | missng XML code comments | missng XML code comments |
| WIQLOrderBit | String | A work item query to affect the order in which the work items are migrated. Don't leave this empty. | [System.ChangedDate] desc |
| WIQLQueryBit | String | A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits) | AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') |
| WorkItemCreateRetryLimit | Int32 | **beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count. This allows for periodic network glitches not to end the process. | 5 |
| WorkItemIDs | IList | A list of work items to import | [] |


### Example JSON

```JSON
{
  "$type": "WorkItemUpdateConfig",
  "Enabled": false,
  "WhatIf": false,
  "WIQLQueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')",
  "WIQLOrderBit": null,
  "WorkItemIDs": null,
  "FilterWorkItemsThatAlreadyExistInTarget": false,
  "PauseAfterEachWorkItem": false,
  "WorkItemCreateRetryLimit": 0
}
```