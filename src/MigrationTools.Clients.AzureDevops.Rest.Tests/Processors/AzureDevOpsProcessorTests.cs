using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tests;
using Serilog.Events;

namespace MigrationTools.Processors.Tests
{
    public class AzureDevOpsProcessorTests
    {
        protected ServiceProvider Services = ServiceProviderHelper.GetServices();

        [TestInitialize]
        public void Setup()
        {
            Serilog.Sinks.InMemory.InMemorySink.Instance?.Dispose();            
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

        public static object GetValueFromProperty(LogEventPropertyValue value) =>
            value switch
        {
            ScalarValue v => v.Value,
            _ => value.ToString(),
        };
    }
}