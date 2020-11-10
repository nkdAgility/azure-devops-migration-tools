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

        [TestMethod(), TestCategory("L3")]
        public void TfsWorkItemEndPointTest()
        {
            var endpoint = Services.GetRequiredService<TfsWorkItemEndPoint>();
            endpoint.Configure(GetTfsWorkItemEndPointOptions(EndpointDirection.Source, "migrationSource1"));
            endpoint.GetWorkItems();
            Assert.IsNotNull(endpoint);
        }

        [TestMethod(), TestCategory("L3")]
        public void ConfigureTest()
        {
            var endpoint = Services.GetRequiredService<TfsWorkItemEndPoint>();
            endpoint.Configure(GetTfsWorkItemEndPointOptions(EndpointDirection.Source, "migrationSource1"));
            Assert.IsNotNull(endpoint);
        }

        //[TestMethod()]
        //public void FilterTest()
        //{
        //    Assert.Fail();
        //}

        [TestMethod(), TestCategory("L3")]
        public void GetWorkItemsTest()
        {
            var endpoint = Services.GetRequiredService<TfsWorkItemEndPoint>();
            endpoint.Configure(GetTfsWorkItemEndPointOptions(EndpointDirection.Source, "migrationSource1"));
            IEnumerable<WorkItemData> result = endpoint.GetWorkItems();
            Assert.AreEqual(5, result.Count());
        }

        [TestMethod(), TestCategory("L3")]
        public void GetWorkItemsQueryTest()
        {
            var endpoint = Services.GetRequiredService<TfsWorkItemEndPoint>();
            endpoint.Configure(GetTfsWorkItemEndPointOptions(EndpointDirection.Source, "migrationSource1"));
            QueryOptions qo = new QueryOptions()
            {
                Query = "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject",
                Paramiters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
            };
            IEnumerable<WorkItemData> result = endpoint.GetWorkItems(qo);
            Assert.AreEqual(5, result.Count());
        }

        //[TestMethod()]
        //public void PersistWorkItemTest()
        //{
        //    Assert.Fail();
        //}

        private static TfsWorkItemEndPointOptions GetTfsWorkItemEndPointOptions(EndpointDirection direction, string project)
        {
            return new TfsWorkItemEndPointOptions()
            {
                Direction = direction,
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = "migrationSource1",
                AccessToken = "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
                Query = new Options.QueryOptions()
                {
                    Query = "SELECT [System.Id], [System.Tags] " +
                            "FROM WorkItems " +
                            "WHERE [System.TeamProject] = @TeamProject " +
                                "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') " +
                            "ORDER BY [System.ChangedDate] desc",
                    Paramiters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
                }
            };
        }
    }
}