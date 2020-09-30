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
        IWorkItemMigrationClient workItemMigrationClient;

        public MigrationClientMock(IWorkItemMigrationClient workItemMigrationClient)
        {
            this.workItemMigrationClient = workItemMigrationClient;
        } 

        public TeamProjectConfig Config => throw new NotImplementedException();

        public object InternalCollection => throw new NotImplementedException();

        public IWorkItemMigrationClient WorkItems => workItemMigrationClient;

        public void Configure( TeamProjectConfig config, NetworkCredential credentials = null)
        {
           
        }

        public T GetService<T>()
        {
            return default(T);
        }

    }
}
