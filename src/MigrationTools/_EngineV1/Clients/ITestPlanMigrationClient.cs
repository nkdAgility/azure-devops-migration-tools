using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;

namespace MigrationTools._EngineV1.Clients
{
    public interface ITestPlanMigrationClient
    {
        IMigrationClientConfig Config { get; }

        void Configure(IMigrationClient migrationClient, bool bypassRules = true);

        List<TestPlanData> GetTestPlans();

        TestPlanData CreateTestPlan();
    }
}