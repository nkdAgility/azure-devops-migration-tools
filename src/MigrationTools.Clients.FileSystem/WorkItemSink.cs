using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Clients;
using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Core.Configuration;
using System.Net;

namespace MigrationTools.Sinks.FileSystem
{
    public class WorkItemMigrationClient : IMigrationClient
    {
        public TeamProjectConfig Config => throw new NotImplementedException();

        public object InternalCollection => throw new NotImplementedException();

        public void Configure(TeamProjectConfig config, NetworkCredential credentials = null)
        {
            throw new NotImplementedException();
        }

        public T GetService<T>()
        {
            throw new NotImplementedException();
        }

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
