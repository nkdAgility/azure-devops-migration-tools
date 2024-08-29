using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;

namespace MigrationTools._EngineV1.Clients
{
    public interface ITestPlanMigrationClient
    {
        IEndpointOptions Options { get; set; }


        List<TestPlanData> GetTestPlans();

        TestPlanData CreateTestPlan();
    }
}