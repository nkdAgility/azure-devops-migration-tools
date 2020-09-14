using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.Core.Execution.MigrationContext
{
    public static class MigrationContextBaseExtensions
    {
        public static void SaveWorkItem(this MigrationContextBase mcb, WorkItem workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            workItem.Fields["System.ChangedBy"].Value = "Migration";
            workItem.Save();
        }
    }
}
