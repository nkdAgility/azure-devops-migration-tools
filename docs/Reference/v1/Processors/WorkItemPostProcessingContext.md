---
title: WorkItemPostProcessingContext
layout: default
template: default
pageType: reference
classType: Processors
architecture: v1
toc: true
pageStatus: generated
discussionId: 
redirect_to: https://nkdagility.com/learn/azure-devops-migration-tools/Reference/v1/Processors/WorkItemPostProcessingContext.html
---


>**_This documentation is for a preview version of the Azure DevOps Migration Tools._ If you are not using the preview version then please head over to the main [documentation](https://nkdagility.com/docs/azure-devops-migration-tools).**

Reapply field mappings after a migration. Does not migtate Work Items, only reapplied changes to filed mappings.

### Options

| Parameter name         | Type    | Description                              | Default Value                            |
|------------------------|---------|------------------------------------------|------------------------------------------|
| Enabled | Boolean | missng XML code comments | missng XML code comments |
| FilterWorkItemsThatAlreadyExistInTarget | Boolean | This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run. While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart. | true |
| PauseAfterEachWorkItem | Boolean | Pause after each work item is migrated | false |
| WIQLOrderBit | String | A work item query to affect the order in which the work items are migrated. Don't leave this empty. | [System.ChangedDate] desc |
| WIQLQueryBit | String | A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits) | AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') |
| WorkItemCreateRetryLimit | Int32 | **beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count. This allows for periodic network glitches not to end the process. | 5 |
| WorkItemIDs | IList | A list of work items to import | [] |


### Example JSON

```JSON
{
  "$type": "WorkItemPostProcessingConfig",
  "Enabled": false,
  "WorkItemIDs": null,
  "WIQLQueryBit": "AND [TfsMigrationTool.ReflectedWorkItemId] = '' ",
  "WIQLOrderBit": null,
  "FilterWorkItemsThatAlreadyExistInTarget": false,
  "PauseAfterEachWorkItem": false,
  "WorkItemCreateRetryLimit": 0
}
```