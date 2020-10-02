using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.Configuration;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Clients
{
    public class TestPlanMigrationClient : ITestPlanMigrationClient
    {
        public TeamProjectConfig Config => throw new NotImplementedException();

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
