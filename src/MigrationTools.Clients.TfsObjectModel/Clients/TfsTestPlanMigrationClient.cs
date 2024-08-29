using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Options;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsTestPlanMigrationClient : ITestPlanMigrationClient
    {
     

        public TfsTestPlanMigrationClient(IOptions<TfsTeamProjectEndpointOptions> options)
        {
            Options = options.Value;
        }

        public IEndpointOptions Options { get; set ; }

        public TestPlanData CreateTestPlan()
        {
            throw new NotImplementedException();
        }

        public List<TestPlanData> GetTestPlans()
        {
            throw new NotImplementedException();
        }
    }
}