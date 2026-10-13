using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Shadows;
using MigrationTools.Tests;
using MigrationTools.Tools;
using MigrationTools.Tools.Interfaces;
using MigrationTools.Tools.Shadows;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class TfsWorkItemEndPointTests
    {

        [TestMethod(), TestCategory("L3")]
        [RequiresAzureDevOps]
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
        [RequiresAzureDevOps]
        public void TfsWorkItemEndPointGetWorkItemsTest()
        {
            var endpoint = GetTfsWorkItemEndPoint();
            var result = endpoint.GetWorkItems().ToList();
            AssertMatchesServer(result);
        }

        private const string SourceProjectQuery = "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = 'migrationSource1' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

        // migrationSource1 is shared and its contents change, so the expected set comes from the server at test time
        private static void AssertMatchesServer(IList<WorkItemData> result)
        {
            List<string> expectedIds;
            using (var connection = new VssConnection(TestingConstants.SourceCollection, new VssBasicCredential(string.Empty, TestingConstants.ResolveAccessToken())))
            using (var client = connection.GetClient<WorkItemTrackingHttpClient>())
            {
                expectedIds = client.QueryByWiqlAsync(new Wiql { Query = SourceProjectQuery }).Result.WorkItems.Select(r => r.Id.ToString()).ToList();
            }

            Assert.IsTrue(expectedIds.Count > 0, "Expected work items in migrationSource1");
            CollectionAssert.AreEquivalent(expectedIds, result.Select(wi => wi.Id).ToList(), "Endpoint should return exactly the work items the server returns for the same query");
            Assert.IsTrue(result.All(wi => wi.ProjectName == "migrationSource1"), "Every work item should come from migrationSource1");
            Assert.IsFalse(result.Any(wi => wi.Type == "Test Suite" || wi.Type == "Test Plan"), "Test Suites and Test Plans should be excluded");
        }

        [TestMethod(), TestCategory("L3")]
        [RequiresAzureDevOps]
        public void TfsWorkItemEndPointGetWorkItemsQueryTest()
        {
            var endpoint = GetTfsWorkItemEndPoint();
            QueryOptions qo = new QueryOptions()
            {
                Query = "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')",
                Parameters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
            };
            var result = endpoint.GetWorkItems(qo).ToList();
            AssertMatchesServer(result);
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
                    Collection = options != null ? options.Collection : TestingConstants.SourceCollection,
                    Project = options != null ? options.Project : "migrationSource1",
                    Authentication = new TfsAuthenticationOptions()
                    {
                        AccessToken = options != null ? options.Authentication.AccessToken : TestingConstants.AccessToken,
                        AuthenticationMode = options != null ? options.Authentication.AuthenticationMode : TestingConstants.AuthenticationMode
                    },
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


        [TestMethod(), TestCategory("L1")]
        public void TfsWorkItemEndPoint_EnvironmentOverrideTest()
        {
            Environment.SetEnvironmentVariable("MigrationTools__Endpoints__Source__Authentication__AccessToken", "654321");
            IConfigurationBuilder configBuilder = GetSourceTargetBasicConfig();
            var configuration = configBuilder.AddEnvironmentVariables().Build();
            // Create services
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<EndpointEnricherContainer>();
            serviceCollection.AddMigrationToolServicesForUnitTests();
            serviceCollection.AddConfiguredEndpoints(configuration);
            // Create a service provider from the service collection
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var endpoint = serviceProvider.GetKeyedService<IEndpoint>("Source");
            Assert.IsNotNull(endpoint, "Endpoint not found.");
            Endpoint<TfsTeamProjectEndpointOptions> endpoint1 = endpoint as Endpoint<TfsTeamProjectEndpointOptions>;
            // Validate that the correct number of endpoints are registered
            Assert.AreEqual("654321", endpoint1.Options.Authentication.AccessToken, "Token not passed.");

        }

        private static IConfigurationBuilder GetSourceTargetBasicConfig()
        {
            // Create Config
            var json = @"
            {
              ""MigrationTools"": {
                ""Version"": ""16.0"",
                ""Endpoints"": {
                  ""Source"": {
                    ""EndpointType"": ""TfsTeamProjectEndpoint"",
                    ""Collection"": ""https://dev.azure.com/nkdagility-preview/"",
                    ""Project"": ""migrationSource1"",
                    ""AllowCrossProjectLinking"": false,
                    ""ReflectedWorkItemIdField"": ""Custom.ReflectedWorkItemId"",
                    ""Authentication"": {
                      ""AuthenticationMode"": ""AccessToken"",
                      ""AccessToken"": ""123456"",
                      ""NetworkCredentials"": {
                        ""UserName"": """",
                        ""Password"": """",
                        ""Domain"": """"
                      }
                    },
                    ""LanguageMaps"": {
                      ""AreaPath"": ""Area"",
                      ""IterationPath"": ""Iteration""
                    }
                  },
                  ""Target"": {
                    ""EndpointType"": ""TfsTeamProjectEndpoint"",
                    ""Collection"": ""https://dev.azure.com/nkdagility-preview/"",
                    ""Project"": ""migrationTest5"",
                    ""TfsVersion"": ""AzureDevOps"",
                    ""Authentication"": {
                      ""AuthenticationMode"": ""AccessToken"",
                      ""AccessToken"": ""none"",
                      ""NetworkCredentials"": {
                        ""UserName"": """",
                        ""Password"": """",
                        ""Domain"": """"
                      }
                    },
                    ""ReflectedWorkItemIdField"": ""nkdScrum.ReflectedWorkItemId"",
                    ""AllowCrossProjectLinking"": false,
                    ""LanguageMaps"": {
                      ""AreaPath"": ""Area"",
                      ""IterationPath"": ""Iteration""
                    }
                  }
                },
              }
            }";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var configBuilder = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            return configBuilder;
        }

    }
}
