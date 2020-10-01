using System;
using System.Collections.Generic;
using System.Text;
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
