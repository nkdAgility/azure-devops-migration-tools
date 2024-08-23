using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.Endpoints;

namespace MigrationTools._EngineV1.Clients
{
    public interface ITestPlanMigrationClient
    {
        IEndpointOptions Config { get; }

        void Configure(IMigrationClient migrationClient, bool bypassRules = true);

        List<TestPlanData> GetTestPlans();

        TestPlanData CreateTestPlan();
    }
}