using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Options;
using MigrationTools.Tests;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class TfsWorkItemEndPointTests
    {
        public ServiceProvider Services { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            Services = ServiceProviderHelper.GetServices();
        }

        [TestMethod(), TestCategory("L3"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsWorkItemEndPointTest()
        {
            var endpoint = Services.GetRequiredService<TfsWorkItemEndpoint>();
            endpoint.Configure(GetTfsWorkItemEndPointOptions("migrationSource1"));
            endpoint.GetWorkItems();
            Assert.IsNotNull(endpoint);
        }

        [TestMethod(), TestCategory("L3"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsWorkItemEndPointConfigureTest()
        {
            var endpoint = Services.GetRequiredService<TfsWorkItemEndpoint>();
            endpoint.Configure(GetTfsWorkItemEndPointOptions("migrationSource1"));
            Assert.IsNotNull(endpoint);
        }

        [TestMethod(), TestCategory("L3"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsWorkItemEndPointGetWorkItemsTest()
        {
            var endpoint = Services.GetRequiredService<TfsWorkItemEndpoint>();
            endpoint.Configure(GetTfsWorkItemEndPointOptions("migrationSource1"));
            IEnumerable<WorkItemData> result = endpoint.GetWorkItems();
            Assert.AreEqual(9, result.Count());
        }

        [TestMethod(), TestCategory("L3"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsWorkItemEndPointGetWorkItemsQueryTest()
        {
            TfsWorkItemEndpoint endpoint = Services.GetRequiredService<TfsWorkItemEndpoint>();
            endpoint.Configure(GetTfsWorkItemEndPointOptions("migrationSource1"));
            QueryOptions qo = new QueryOptions()
            {
                Query = "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
                Parameters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
            };
            IEnumerable<WorkItemData> result = endpoint.GetWorkItems(qo);
            Assert.AreEqual(9, result.Count());
        }

        private static TfsWorkItemEndpointOptions GetTfsWorkItemEndPointOptions(string project)
        {
            return new TfsWorkItemEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = "migrationSource1",
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
                Query = new Options.QueryOptions()
                {
                    Query = "SELECT [System.Id], [System.Tags] " +
                            "FROM WorkItems " +
                            "WHERE [System.TeamProject] = @TeamProject " +
                                "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') " +
                            "ORDER BY [System.ChangedDate] desc",
                    Parameters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
                }
            };
        }
    }
}