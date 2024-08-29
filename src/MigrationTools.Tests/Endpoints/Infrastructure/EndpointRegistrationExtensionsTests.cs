using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Shadows;
using MigrationTools.Shadows;

namespace MigrationTool.Endpoints.Tests
{
    [TestClass()]
    public class EndpointRegistrationExtensionsTests
    {
        [TestMethod(), TestCategory("L1")]
        public void EndpointRegistrationExtensions_BasicTest()
        {
            IConfigurationBuilder configBuilder = GetSourceTargetBasicConfig();
            var configuration = configBuilder.Build();
            // Create services
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<EndpointEnricherContainer>();

            serviceCollection.AddMigrationToolServicesForUnitTests();

            serviceCollection.AddConfiguredEndpoints(configuration);
            // Create a service provider from the service collection
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var endpoint = serviceProvider.GetKeyedService<IEndpoint>("Source");
            Assert.IsNotNull(endpoint, "Endpoint not found.");
            Endpoint<FakeEndpointOptions> endpoint1 = endpoint as Endpoint<FakeEndpointOptions>;           
            // Validate that the correct number of endpoints are registered
            Assert.AreEqual("123456", endpoint1.Options.Token, "Token not passed.");
  
        }

        [TestMethod(), TestCategory("L1")]
        public void EndpointRegistrationExtensions_EnvironmentOverrideTest()
        {
            Environment.SetEnvironmentVariable("MigrationTools__Endpoints__Source__Token", "654321");
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
            Endpoint<FakeEndpointOptions> endpoint1 = endpoint as Endpoint<FakeEndpointOptions>;
            // Validate that the correct number of endpoints are registered
            Assert.AreEqual("654321", endpoint1.Options.Token, "Token not passed.");

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
                    ""EndpointType"": ""FakeEndpoint"",
                    ""Token"": ""123456""
                  },
                  ""Target"": {
                    ""EndpointType"": ""FakeEndpoint"",
                    ""Token"": """"
                  }
                }
              }
            }";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var configBuilder = new ConfigurationBuilder().AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            return configBuilder;
        }
    }
}
