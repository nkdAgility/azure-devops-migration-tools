using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Sinks.FileSystem
{
    public class WorkItemSink : IWorkItemMigrationClients
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
