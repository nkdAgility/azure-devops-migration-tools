using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools._EngineV1.DataContracts;

namespace MigrationTools.Clients.AzureDevops.Rest
{
    public static class AzureDevOpsExtensions
    {
        public static WorkItemData ToWorkItemData(this WorkItem workItem)
        {
            var internalObject = new WorkItemData
            {
                Id = workItem.Id.ToString(),
                Type = "unknown",
                Title = "unknown",
                internalObject = workItem
            };
            return internalObject;
        }

        public static WorkItem ToWorkItem(this WorkItemData workItemData)
        {
            if (!(workItemData.internalObject is WorkItem))
            {
                throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(typeof(WorkItem)).FullName}");
            }
            return (WorkItem)workItemData.internalObject;
        }
    }
}