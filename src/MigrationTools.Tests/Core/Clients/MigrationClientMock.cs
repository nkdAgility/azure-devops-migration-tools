﻿using System;
using System.Net;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools.Tests.Core.Clients
{
    public class MigrationClientMock : IMigrationClient
    {
        private IWorkItemMigrationClient workItemMigrationClient;

        public MigrationClientMock(IWorkItemMigrationClient workItemMigrationClient)
        {
            this.workItemMigrationClient = workItemMigrationClient;
        }

        public IMigrationClientConfig Config => throw new NotImplementedException();

        public object InternalCollection => throw new NotImplementedException();

        public IWorkItemMigrationClient WorkItems => workItemMigrationClient;

        public ITestPlanMigrationClient TestPlans => throw new NotImplementedException();

        public VssCredentials Credentials => throw new NotImplementedException();

        public void Configure(IMigrationClientConfig config, NetworkCredential credentials = null)
        {
        }

        public T GetClient<T>() where T : IVssHttpClient
        {
            return default(T);
        }

        public T GetService<T>()
        {
            return default(T);
        }
    }
}