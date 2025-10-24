using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Shadows;
using MigrationTools.Tests;
using MigrationTools.Tools;
using MigrationTools.Tools.Interfaces;
using MigrationTools.Tools.Shadows;
using Serilog.Events;

namespace MigrationTools.Processors.Tests
{
    public class AzureDevOpsProcessorTests
    {

        protected AzureDevOpsPipelineProcessor GetAzureDevOpsPipelineProcessor(AzureDevOpsPipelineProcessorOptions options = null)
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddSingleton<ProcessorEnricherContainer>();
            services.AddSingleton<EndpointEnricherContainer>();
            services.AddSingleton<CommonTools>();
            services.AddSingleton<IFieldMappingTool, MockFieldMappingTool>();
            services.AddSingleton<IWorkItemTypeMappingTool, MockWorkItemTypeMappingTool>();
            services.AddSingleton<IExportWorkItemMappingTool, MockExportWorkItemMappingTool>();
            services.AddSingleton<IStringManipulatorTool, StringManipulatorTool>();

            services.AddSingleton<AzureDevOpsPipelineProcessor>();
            AddEndpoint(services, options != null ? options.SourceName : "Source", "migrationSource1");
            AddEndpoint(services, options != null ? options.SourceName : "Target", "migrationTarget1");
            services.Configure((System.Action<AzureDevOpsPipelineProcessorOptions>)(o =>
            {
                o.Enabled = options != null ? options.Enabled : true;
                o.BuildPipelines = options != null ? options.BuildPipelines : null;
                o.ReleasePipelines = options != null ? options.ReleasePipelines : null;
                o.SourceName = options != null ? options.SourceName : "Source";
                o.TargetName = options != null ? options.SourceName : "Target";
                o.Enrichers = options != null ? options.Enrichers : null;
                o.MigrateTaskGroups = options != null ? options.MigrateTaskGroups : true;
                o.MigrateBuildPipelines = options != null ? options.MigrateBuildPipelines : true;
                o.MigrateReleasePipelines = options != null ? options.MigrateReleasePipelines : true;
                o.MigrateServiceConnections = options != null ? options.MigrateServiceConnections : true;
                o.MigrateVariableGroups = options != null ? options.MigrateVariableGroups : true;
                o.Enrichers = options != null ? options.Enrichers : null;
                o.RefName = options != null ? options.RefName : null;
                o.RepositoryNameMaps = options != null ? options.RepositoryNameMaps : null;
            }));

            return services.BuildServiceProvider().GetService<AzureDevOpsPipelineProcessor>();
        }

        private static void AddEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddKeyedSingleton(typeof(IEndpoint), name, (sp, key) =>
            {
                var options = GetAzureDevOpsEndpointOptions(project);
                var endpoint = ActivatorUtilities.CreateInstance(sp, typeof(AzureDevOpsEndpoint), options);
                return endpoint;
            });
        }
        private static IOptions<AzureDevOpsEndpointOptions> GetAzureDevOpsEndpointOptions(string project)
        {
            IOptions<AzureDevOpsEndpointOptions> options = Microsoft.Extensions.Options.Options.Create(new AzureDevOpsEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            });
            return options;
        }


        public static object GetValueFromProperty(LogEventPropertyValue value) =>
            value switch
            {
                ScalarValue v => v.Value,
                _ => value.ToString(),
            };
    }
}
