using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                MigrateTaskGroups = true,
                MigrateBuildPipelines = true,
                MigrateReleasePipelines = true,
                SourceName = "Source",
                TargetName = "Target"
            };
            return migrationConfig;
        }
    }
}