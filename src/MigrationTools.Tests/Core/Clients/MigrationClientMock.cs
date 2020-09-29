using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MigrationTools.Core.Clients;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;

namespace MigrationTools.Tests.Core.Clients
{
    public class MigrationClientMock : IMigrationClient
    {
        public TeamProjectConfig Config => throw new NotImplementedException();

        public object InternalCollection => throw new NotImplementedException();

        public void Configure(TeamProjectConfig config, NetworkCredential credentials = null)
        {
            //
        }

        public T GetService<T>()
        {
            return default(T);
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
