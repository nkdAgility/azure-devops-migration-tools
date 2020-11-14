using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Tests;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsSharedQueryProcessorTests : TfsProcessorTests
    {
        public ServiceProvider Services { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            Services = ServiceProviderHelper.GetServices();
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsSharedQueryProcessorTest()
        {
            var x = Services.GetRequiredService<TfsSharedQueryProcessor>();
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0")]
        public void ConfigureTest()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                Source = GetTfsWorkItemEndPointOptions("source"),
                Target = GetTfsWorkItemEndPointOptions("target")
            };
            var x = Services.GetRequiredService<TfsSharedQueryProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0")]
        public void RunTest()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                Source = GetTfsWorkItemEndPointOptions("source"),
                Target = GetTfsWorkItemEndPointOptions("target")
            };
            var x = Services.GetRequiredService<TfsSharedQueryProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L3")]
        public void TestTfsSharedQueryProcessorNoEnrichers()
        {
            // Senario 1 Migration from Tfs to Tfs with no Enrichers.
            var migrationConfig = GetTfsSharedQueryProcessorOptions();
            var processor = Services.GetRequiredService<TfsSharedQueryProcessor>();
            processor.Configure(migrationConfig);
            //processor.Execute();
            Assert.AreEqual(ProcessingStatus.Complete, processor.Status);
        }

        private static TfsWorkItemEndpointOptions GetTfsWorkItemEndPointOptions(string project)
        {
            return new TfsWorkItemEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
                Query = new Options.QueryOptions()
                {
                    Query = "SELECT [System.Id], [System.Tags] " +
                            "FROM WorkItems " +
                            "WHERE [System.TeamProject] = @TeamProject " +
                                "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') " +
                            "ORDER BY [System.ChangedDate] desc",
                    Paramiters = new Dictionary<string, string>() { { "TeamProject", project } }
                }
            };
        }
    }
}