using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Core.Engine.Containers;
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



    }
}
