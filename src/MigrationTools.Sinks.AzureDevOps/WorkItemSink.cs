using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Sinks;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Sinks.AzureDevOps
{
    public class WorkItemSink : IWorkItemSink
    {
        public IEnumerable<WorkItemData> GetWorkItems()
        {
            throw new NotImplementedException();
        }

        public WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }
    }
}
