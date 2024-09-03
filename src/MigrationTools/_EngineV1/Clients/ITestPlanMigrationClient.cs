using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;

namespace MigrationTools.Clients
{
    public interface ITestPlanMigrationClient
    {
        IEndpointOptions Options { get; set; }


        List<TestPlanData> GetTestPlans();

        TestPlanData CreateTestPlan();
    }
}