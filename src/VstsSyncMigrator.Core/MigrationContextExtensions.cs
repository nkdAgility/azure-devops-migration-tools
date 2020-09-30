using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Engine.Containers;
using System;
using VstsSyncMigrator.Core.Execution.OMatics;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.Core
{
    public static class MigrationContextBaseExtensions
    {
        public static void SaveWorkItem(this IProcessor context, WorkItem workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            workItem.Fields["System.ChangedBy"].Value = "Migration";
            workItem.Save();
        }

        public static void SaveWorkItem(this IProcessor context, WorkItemData workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            var wi = (WorkItem)workItem.InternalWorkItem;
            wi.Fields["System.ChangedBy"].Value = "Migration";
            wi.Save();
        }

        //public static WorkItem ToWorkItem(this WorkItemData workItemData)
        //{
        //    if (!(workItemData.InternalWorkItem is WorkItem))
        //    {
        //        throw new InvalidCastException($"The Work Item stored in the inner field must be of type {(typeof(WorkItem)).FullName}");
        //    }
        //    return (WorkItem)workItemData.InternalWorkItem;
        //}

    }
}
