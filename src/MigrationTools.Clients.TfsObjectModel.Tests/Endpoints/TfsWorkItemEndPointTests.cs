using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                    Collection = options != null ? options.Collection : new Uri("https://dev.azure.com/nkdagility-preview/"),
                    Project = options != null ? options.Project : "migrationSource1",
                    Authentication = new TfsAuthenticationOptions()
                    {
                        AccessToken = options != null ? options.Authentication.AccessToken : TestingConstants.AccessToken,
                        AuthenticationMode = options != null ? options.Authentication.AuthenticationMode : AuthenticationMode.AccessToken
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
