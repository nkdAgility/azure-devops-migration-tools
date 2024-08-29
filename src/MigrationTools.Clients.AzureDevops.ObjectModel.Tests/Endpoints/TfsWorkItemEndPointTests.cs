using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Processors;
using MigrationTools.Tests;
using MigrationTools.Tools;
using MigrationTools.Tools.Interfaces;
using MigrationTools.Tools.Shadows;
using MigrationTools.Shadows;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class TfsWorkItemEndPointTests
    {

        [TestMethod(), TestCategory("L3")]
        public void TfsWorkItemEndPointTest()
        {
            var endpoint = GetTfsWorkItemEndPoint();
            endpoint.GetWorkItems();
            Assert.IsNotNull(endpoint);
        }

        [TestMethod(), TestCategory("L3")]
        public void TfsWorkItemEndPointConfigureTest()
        {
            var endpoint = GetTfsWorkItemEndPoint();
            Assert.IsNotNull(endpoint);
        }

        [TestMethod(), TestCategory("L3")]
        public void TfsWorkItemEndPointGetWorkItemsTest()
        {
            var endpoint = GetTfsWorkItemEndPoint();
            IEnumerable<WorkItemData> result = endpoint.GetWorkItems();
            Assert.AreEqual(13, result.Count());
        }

        [TestMethod(), TestCategory("L3")]
        public void TfsWorkItemEndPointGetWorkItemsQueryTest()
        {
            var endpoint = GetTfsWorkItemEndPoint();
            QueryOptions qo = new QueryOptions()
            {
                Query = "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
                Parameters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
            };
            IEnumerable<WorkItemData> result = endpoint.GetWorkItems(qo);
            Assert.AreEqual(13, result.Count());
        }

        protected TfsWorkItemEndpoint GetTfsWorkItemEndPoint(string key = "Source", TfsWorkItemEndpointOptions options = null)
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            // Add required DI Bits
            services.AddSingleton<ProcessorEnricherContainer>();
            services.AddSingleton<EndpointEnricherContainer>();
            services.AddSingleton<CommonTools>();
            services.AddSingleton<IFieldMappingTool, MockFieldMappingTool>();
            services.AddSingleton<IWorkItemTypeMappingTool, MockWorkItemTypeMappingTool>();
            services.AddSingleton<IStringManipulatorTool, StringManipulatorTool>();
            services.AddSingleton<IWorkItemQueryBuilderFactory, WorkItemQueryBuilderFactory>();
            services.AddSingleton<IWorkItemQueryBuilder, WorkItemQueryBuilder>();

            // Add the Endpoints
            services.AddKeyedSingleton(typeof(IEndpoint), key, (sp, key) =>
            {
                IOptions<TfsWorkItemEndpointOptions> wrappedOptions = Microsoft.Extensions.Options.Options.Create(new TfsWorkItemEndpointOptions()
                {
                    Organisation = options != null? options.Organisation : "https://dev.azure.com/nkdagility-preview/",
                    Project = options != null ? options.Project : "migrationSource1",
                    AuthenticationMode = options != null ? options.AuthenticationMode : AuthenticationMode.AccessToken,
                    AccessToken = options != null ? options.AccessToken : TestingConstants.AccessToken,
                    Query = options != null ? options.Query : new Options.QueryOptions()
                    {
                        Query = "SELECT [System.Id], [System.Tags] " +
                            "FROM WorkItems " +
                            "WHERE [System.TeamProject] = @TeamProject " +
                                "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') " +
                            "ORDER BY [System.ChangedDate] desc",
                        Parameters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
                    }
                });
                return ActivatorUtilities.CreateInstance(sp, typeof(TfsWorkItemEndpoint), wrappedOptions);
            });
           
            return (TfsWorkItemEndpoint)services.BuildServiceProvider().GetRequiredKeyedService<IEndpoint>(key);
        }

    }
}