using System.Collections.Generic;
using MigrationTools.Configuration;

namespace MigrationTools.Clients
{
    public interface ITestPlanMigrationClient
    {
        TeamProjectConfig Config { get; }

        void Configure(IMigrationClient migrationClient, bool bypassRules = true);

        List<TestPlanData> GetTestPlans();

        TestPlanData CreateTestPlan();


    }
}
