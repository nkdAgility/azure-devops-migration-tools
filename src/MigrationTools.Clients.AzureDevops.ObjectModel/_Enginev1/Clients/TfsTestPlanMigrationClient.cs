using System;
using System.Collections.Generic;
using MigrationTools.Configuration;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsTestPlanMigrationClient : ITestPlanMigrationClient
    {
        public IMigrationClientConfig Config => throw new NotImplementedException();

        public void Configure(IMigrationClient migrationClient, bool bypassRules = true)
        {
            // No current config
        }

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