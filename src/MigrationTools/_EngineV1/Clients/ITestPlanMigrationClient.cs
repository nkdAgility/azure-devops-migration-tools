using System.Collections.Generic;
using MigrationTools.Configuration;

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