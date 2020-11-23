using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Tests;

namespace MigrationTools.Processors.Tests
{
    public class AzureDevOpsProcessorTests
    {
        protected ServiceProvider Services = ServiceProviderHelper.GetServices();

        [TestInitialize]
        public void Setup()
        {
        }

        protected static AzureDevOpsPipelineProcessorOptions GetAzureDevOpsPipelineProcessorOptions()
        {
            // Azure DevOps to Azure DevOps
            var migrationConfig = new AzureDevOpsPipelineProcessorOptions()
            {
                Enabled = true,
                Target = GetAzureDevOpsEndpointOptions("migrationTarget1"),
                Source = GetAzureDevOpsEndpointOptions("migrationSource1"),
            };
            return migrationConfig;
        }

        protected static AzureDevOpsEndpointOptions GetAzureDevOpsEndpointOptions(string project)
        {
            return new AzureDevOpsEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            };
        }
    }
}